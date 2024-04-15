using System.Linq;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    using SH = StudioHack;

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

        public override void OnOpen()
        {
            timelineManager.UpdateTimelineFileList(false);
        }

        public override void DrawWindow(int id)
        {
            {
                var view = new GUIView(0, 0, WINDOW_WIDTH, 20);
                view.padding = Vector2.zero;

                view.currentPos.x = WINDOW_WIDTH - 100;
                if (view.DrawButton("更新", 50, 20))
                {
                    timelineManager.UpdateTimelineFileList(true);
                }
            }

            {
                var view = new GUIView(0, 20, WINDOW_WIDTH, WINDOW_HEIGHT - 20);

                var fileList = timelineManager.timelineFileList;
                if (fileList.Count == 0)
                {
                    view.DrawLabel("タイムラインがありません", -1, 20);
                }
                else
                {
                    view.padding = Vector2.zero;

                    var index = view.DrawTileView(
                        fileList.Cast<ITileViewContent>(),
                        -1,
                        -1,
                        ref scrollPosition,
                        120,
                        120 * config.thumHeight / config.thumWidth + 20,
                        2);
                    if (index != -1)
                    {
                        timelineManager.LoadTimeline(fileList[index].name);
                    }
                }
            }

            GUI.DragWindow();
        }
    }
}