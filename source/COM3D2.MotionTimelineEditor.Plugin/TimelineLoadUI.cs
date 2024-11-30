using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TimelineLoadUI : SubWindowUIBase
    {
        public override string title => "タイムライン ロード";

        private TimelineLoadItem selectedItem
        {
            get
            {
                return timelineLoadManager.selectedItem;
            }
            set
            {
                timelineLoadManager.selectedItem = value;
            }
        }

        private static TimelineLoadManager timelineLoadManager => TimelineLoadManager.instance;

        public TimelineLoadUI(SubWindow subWindow) : base(subWindow)
        {
        }

        public override void DrawContent(GUIView view)
        {
            var directoryName = selectedItem.path.Replace(PluginUtils.TimelineDirPath, "");
            if (directoryName.Length > 0 && directoryName[0] == Path.DirectorySeparatorChar)
            {
                directoryName = directoryName.Substring(1);
            }

            view.BeginHorizontal();
            {
                if (view.DrawButton("<", 20, 20, selectedItem.parent != null))
                {
                    selectedItem = selectedItem.parent;
                }

                view.DrawLabel(selectedItem.name, -1, 20);

                view.currentPos.x = WINDOW_WIDTH - 60 - 60;

                if (view.DrawButton("開く", 50, 20))
                {
                    PluginUtils.OpenDirectory(selectedItem.path);
                }

                if (view.DrawButton("更新", 50, 20))
                {
                    timelineLoadManager.Reload();
                }
            }
            view.EndLayout();

            view.DrawHorizontalLine(Color.gray);

            view.AddSpace(5);

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
                        timelineManager.Play();
                    }
                }

                view.DrawBox(-1, 20);

                if (mouseOverItem != null)
                {
                    view.DrawLabel(mouseOverItem.name, -1, 20);
                }
            }
        }
    }
}