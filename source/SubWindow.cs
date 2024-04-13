using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    using MTE = MotionTimelineEditor;

    public enum SubWindowType
    {
        None,
        TimelineLoad,
        TimelineSetting,
        IKHold,
        KeyFrame,
        MoviePlayer,
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
                    PluginUtils.LogError("SubWindow.isShowWnd = true is not supported");
                    return;
                }

                if (ui != null) ui.OnClose();
                _subWindowType = SubWindowType.None;
            }
        }

        public static SubWindowUIBase[] uiList = new SubWindowUIBase[]
        {
            null,
            new TimelineLoadUI(),
            new TimelineSettingUI(),
            new IKHoldUI(),
            new KeyFrameUI(),
            new MoviePlayerUI(),
        };

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

        public SubWindowUIBase ui
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

        private static MaidHackBase maidHack
        {
            get
            {
                return MTE.maidHack;
            }
        }

        public void ToggleSubWindow(SubWindowType type)
        {
            subWindowType = subWindowType == type ? SubWindowType.None : type;
        }

        public SubWindowUIBase GetSubWindowUI(SubWindowType type)
        {
            return uiList[(int) type];
        }

        public void Init()
        {
            foreach (var ui in uiList)
            {
                if (ui != null)
                {
                    ui.Init();
                }
            }
        }

        public void Update()
        {
            foreach (var ui in uiList)
            {
                if (ui != null)
                {
                    ui.Update();
                }
            }
        }

        public void LateUpdate()
        {
            foreach (var ui in uiList)
            {
                if (ui != null)
                {
                    ui.LateUpdate();
                }
            }
        }

        public void OnGUI()
        {
            if (isShowWnd)
            {
                var gsWin = MTE.instance.gsWin;
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

            if (!MTE.instance.isPluginActive)
            {
                return;
            }
            if (!maidHack.IsValid())
            {
                return;
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