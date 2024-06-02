using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TimelineLoadUI : SubWindowUIBase
    {
        public override string title
        {
            get
            {
                return "タイムライン ロード";
            }
        }

        private Vector2 scrollPosition = Vector2.zero;

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

        private static TimelineLoadManager timelineLoadManager
        {
            get
            {
                return TimelineLoadManager.instance;
            }
        }

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

            view.BeginLayout(GUIView.LayoutDirection.Horizontal);
            {
                if (view.DrawButton("<", 20, 20, selectedItem.parent != null))
                {
                    selectedItem = selectedItem.parent;
                }

                view.DrawLabel(directoryName, -1, 20);

                view.currentPos.x = WINDOW_WIDTH - 60;

                if (view.DrawButton("更新", 50, 20))
                {
                    timelineLoadManager.Reload();
                }
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
    }
}