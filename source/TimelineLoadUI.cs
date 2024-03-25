using System.Linq;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    using SH = StudioHack;

    public class TimelineLoadUI : ISubWindowUI
    {
        public string title
        {
            get
            {
                return "タイムライン リスト";
            }
        }

        public static int WINDOW_WIDTH
        {
            get
            {
                return SubWindow.WINDOW_WIDTH;
            }
        }
        public static int WINDOW_HEIGHT
        {
            get
            {
                return SubWindow.WINDOW_HEIGHT;
            }
        }

        private Vector2 scrollPosition = Vector2.zero;

        private static TimelineManager timelineManager
        {
            get
            {
                return TimelineManager.instance;
            }
        }

        public static Config config
        {
            get
            {
                return MotionTimelineEditor.config;
            }
        }

        public void OnOpen()
        {
            timelineManager.UpdateTimelineFileList(false);
        }

        public void OnClose()
        {
        }

        public void Update()
        {
        }

        public void DrawWindow(int id)
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
                        80,
                        80 * config.thumHeight / config.thumWidth + 20,
                        3);
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