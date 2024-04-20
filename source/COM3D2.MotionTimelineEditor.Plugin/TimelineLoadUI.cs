using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TimelineLoadItem : ITileViewContent
    {
        public string name { get; set; }
        public Texture2D thum { get; set; }
        public bool isDir { get; set; }
        public List<ITileViewContent> children { get; set; }

        public string path;
        public TimelineLoadItem parent;
    }

    public class TimelineLoadUI : SubWindowUIBase
    {
        public override string title
        {
            get
            {
                return "タイムライン リスト";
            }
        }

        private Vector2 scrollPosition = Vector2.zero;
        private TimelineLoadItem rootItem = new TimelineLoadItem();
        private TimelineLoadItem selectedItem = null;

        public override void OnOpen()
        {
            UpdateItems(false);
        }

        public override void DrawWindow(int id)
        {
            {
                var view = new GUIView(0, 0, WINDOW_WIDTH, 20);
                view.padding = Vector2.zero;

                view.currentPos.x = WINDOW_WIDTH - 100;
                if (view.DrawButton("更新", 50, 20))
                {
                    UpdateItems(true);
                }
            }

            {
                var view = new GUIView(0, 20, WINDOW_WIDTH, WINDOW_HEIGHT - 20);

                var directoryName = selectedItem.path.Replace(PluginUtils.TimelineDirPath, "");
                if (directoryName.Length > 0 && directoryName[0] == Path.DirectorySeparatorChar)
                {
                    directoryName = directoryName.Substring(1);
                }

                view.BeginLayout(GUIView.LayoutDirection.Horizontal);
                {
                    if (view.DrawButton("<", 20, 20, selectedItem.parent != null))
                    {
                        selectedItem = selectedItem.parent;
                    }

                    view.DrawLabel(directoryName, -1, 20);
                }
                view.EndLayout();

                view.AddSpace(10);

                var itemList = selectedItem.children;
                if (itemList.Count == 0)
                {
                    view.DrawLabel("タイムラインがありません", -1, 20);
                }
                else
                {
                    view.padding = Vector2.zero;

                    ITileViewContent mouseOverItem = null;

                    var index = view.DrawTileView(
                        itemList,
                        -1,
                        WINDOW_HEIGHT - 20 - 20 - 10 - 20 - 10,
                        ref scrollPosition,
                        120,
                        120 * config.thumHeight / config.thumWidth + 20,
                        2,
                        item =>
                        {
                            mouseOverItem = item;
                        });
                    if (index != -1)
                    {
                        var item = itemList[index] as TimelineLoadItem;
                        if (item.isDir)
                        {
                            selectedItem = item;
                        }
                        else
                        {
                            timelineManager.LoadTimeline(item.name, directoryName);
                        }
                    }

                    view.currentPos.y = WINDOW_HEIGHT - 20 - 20;

                    view.DrawBox(-1, 20);

                    if (mouseOverItem != null)
                    {
                        view.DrawLabel(mouseOverItem.name, -1, 20);
                    }
                }
            }

            GUI.DragWindow();
        }

        public void ClearAllItems(TimelineLoadItem item)
        {
            if (item == null)
            {
                return;
            }

            if (item.thum != null)
            {
                UnityEngine.Object.Destroy(item.thum);
                item.thum = null;
            }

            if (item.children != null)
            {
                foreach (var child in item.children)
                {
                    ClearAllItems(child as TimelineLoadItem);
                }

                item.children.Clear();
            }
        }

        private void UpdateItems(bool reload)
        {
            if (reload)
            {
                selectedItem = null;
                ClearAllItems(rootItem);
            }

            SearchItems(rootItem, PluginUtils.TimelineDirPath);

            if (selectedItem == null)
            {
                selectedItem = rootItem;
            }
        }

        private void SearchItems(TimelineLoadItem item, string basePath)
        {
            if (item == null)
            {
                return;
            }

            item.path = basePath;

            if (item.children == null)
            {
                item.children = new List<ITileViewContent>(16);
            }

            var files = Directory.GetFiles(basePath, "*.xml");
            foreach (var path in files)
            {
                var anmName = Path.GetFileNameWithoutExtension(path);
                if (item.children.Exists(t => t.name == anmName && !t.isDir))
                {
                    continue;
                }

                var thumPath = PluginUtils.ConvertThumPath(path);
                var thum = PluginUtils.LoadTexture(thumPath);

                var fileItem = new TimelineLoadItem
                {
                    name = anmName,
                    thum = thum,
                };
                item.children.Add(fileItem);
            }

            var dirs = Directory.GetDirectories(basePath);
            foreach (var path in dirs)
            {
                var dirName = Path.GetFileName(path);
                var dirItem = item.children.Find(t => t.name == dirName && t.isDir) as TimelineLoadItem;
                if (dirItem == null)
                {
                    dirItem = new TimelineLoadItem
                    {
                        name = dirName,
                        parent = item,
                        isDir = true,
                    };
                    item.children.Add(dirItem);
                }

                SearchItems(dirItem, path);
            }
        }
    }
}