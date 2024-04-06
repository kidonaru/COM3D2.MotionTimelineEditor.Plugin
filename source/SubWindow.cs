using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public interface ISubWindowUI
    {
        string title { get; }
        void OnOpen();
        void OnClose();
        void Update();
        void DrawWindow(int id);
    }

    public enum SubWindowType
    {
        None,
        TimelineLoad,
        TimelineSetting,
        IKHold,
        KeyFrame,
    }

    public class SubWindow
    {
        public readonly static int WINDOW_ID = 485087;
        public readonly static int WINDOW_WIDTH = 280;
        public readonly static int WINDOW_HEIGHT = 480;
        public Rect rc_stgw = new Rect(
            0,
            0,
            WINDOW_WIDTH,
            WINDOW_HEIGHT
        );

        public bool isShowWnd
        {
            get
            {
                return _subWindowType != SubWindowType.None;
            }
            set
            {
                if (value)
                {
                    Extensions.LogError("SubWindow.isShowWnd = true is not supported");
                    return;
                }

                if (ui != null) ui.OnClose();
                _subWindowType = SubWindowType.None;
            }
        }

        GUIStyle gsWin = new GUIStyle("box")
        {
            fontSize = 12,
            alignment = TextAnchor.UpperLeft
        };

        public static TimelineLoadUI timelineLoadUI = new TimelineLoadUI();
        public static TimelineSettingUI timelineSettingUI = new TimelineSettingUI();
        public static IKHoldUI ikHoldUI = new IKHoldUI();
        public static KeyFrameUI keyFrameUI = new KeyFrameUI();

        private SubWindowType _subWindowType = SubWindowType.None;
        public SubWindowType subWindowType
        {
            get
            {
                return _subWindowType;
            }
            set
            {
                if (_subWindowType == value)
                {
                    return;
                }

                if (ui != null) ui.OnClose();
                _subWindowType = value;
                if (ui != null) ui.OnOpen();
            }
        }

        public ISubWindowUI ui
        {
            get
            {
                return GetSubWindowUI(subWindowType);
            }
        }

        public static Config config
        {
            get
            {
                return MotionTimelineEditor.config;
            }
        }

        public void ToggleSubWindow(SubWindowType type)
        {
            subWindowType = subWindowType == type ? SubWindowType.None : type;
        }

        public ISubWindowUI GetSubWindowUI(SubWindowType type)
        {
            switch (type)
            {
                case SubWindowType.TimelineLoad:
                    return timelineLoadUI;
                case SubWindowType.TimelineSetting:
                    return timelineSettingUI;
                case SubWindowType.IKHold:
                    return ikHoldUI;
                case SubWindowType.KeyFrame:
                    return keyFrameUI;
                default:
                    return null;
            }
        }

        public void Update()
        {
            timelineLoadUI.Update();
            timelineSettingUI.Update();
            ikHoldUI.Update();
            keyFrameUI.Update();
        }

        public void OnGUI()
        {
            if (isShowWnd)
            {
                rc_stgw = GUI.Window(WINDOW_ID, rc_stgw, DrawWindow, ui != null ? ui.title : "", gsWin);
            }
        }

        private void DrawWindow(int id)
        {
            {
                var view = new GUIView(0, 0, WINDOW_WIDTH, 20);
                view.padding.y = 0;
                view.padding.x = 0;

                view.currentPos.x = WINDOW_WIDTH - 20;

                if (view.DrawButton("x", 20, 20))
                {
                    isShowWnd = false;
                }
            }

            if (ui != null) ui.DrawWindow(id);
        }

        public void ResetPosition()
        {
            var source = MotionTimelineEditor.instance.rc_stgw;
            rc_stgw.x = source.x - WINDOW_WIDTH;
            rc_stgw.y = source.y;
        }
    }
}