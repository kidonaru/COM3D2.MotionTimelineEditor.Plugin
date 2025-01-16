using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TimelineLoadUI : SubWindowUIBase
    {
        public override string title => "タイムライン ロード";

        private TimelineLoadItem currentDirItem
        {
            get => timelineLoadManager.selectedItem;
            set => timelineLoadManager.selectedItem = value;
        }

        private static TimelineLoadManager timelineLoadManager => TimelineLoadManager.instance;

        public TimelineLoadUI(SubWindow subWindow) : base(subWindow)
        {
        }

        public override void DrawContent(GUIView view)
        {
            var directoryName = currentDirItem.path.Replace(PluginUtils.TimelineDirPath, "");
            if (directoryName.Length > 0 && directoryName[0] == Path.DirectorySeparatorChar)
            {
                directoryName = directoryName.Substring(1);
            }

            view.BeginHorizontal();
            {
                if (view.DrawButton("<", 20, 20, currentDirItem.parent != null))
                {
                    currentDirItem = currentDirItem.parent as TimelineLoadItem;
                }

                view.DrawLabel(currentDirItem.name, -1, 20);

                view.currentPos.x = WINDOW_WIDTH - 60 - 60;

                if (view.DrawButton("開く", 50, 20))
                {
                    MTEUtils.OpenDirectory(currentDirItem.path);
                }

                if (view.DrawButton("更新", 50, 20))
                {
                    timelineLoadManager.Reload();
                }
            }
            view.EndLayout();

            view.DrawHorizontalLine(Color.gray);

            view.AddSpace(5);

            if (currentDirItem.children.Count == 0)
            {
                view.DrawLabel("タイムラインがありません", -1, 20);
            }
            else
            {
                view.padding = Vector2.zero;

                TimelineLoadItem selectedItem = null;
                TimelineLoadItem mouseOverItem = null;

                view.DrawTileView(
                    currentDirItem,
                    -1,
                    WINDOW_HEIGHT - 20 - 20 - 10 - 20 - 10,
                    120,
                    120 * config.thumHeight / config.thumWidth + 20,
                    false,
                    item =>
                    {
                        selectedItem = item as TimelineLoadItem;
                    },
                    item =>
                    {
                        mouseOverItem = item as TimelineLoadItem;
                    });

                if (selectedItem != null)
                {
                    if (selectedItem.isDir)
                    {
                        currentDirItem = selectedItem;
                    }
                    else
                    {
                        timelineManager.LoadTimeline(selectedItem.name, directoryName);
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