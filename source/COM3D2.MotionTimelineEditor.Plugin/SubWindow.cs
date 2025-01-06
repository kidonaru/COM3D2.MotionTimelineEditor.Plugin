using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public enum SubWindowType
    {
        TimelineLoad,
        KeyFrame,
        TimelineLayer,
        IKHold,
        Track,
        History,
        TimelineSetting,
        Max,

        MediaPlayer = TimelineSetting, // dummy
        StudioModel = TimelineLayer, // dummy
    }

    public class SubWindowInfo
    {
        public int windowIndex = 0;
        public Vector2 position = Vector2.zero;
        public SubWindowType subWindowType = SubWindowType.TimelineLoad;
        public bool isPositionLocked = true;
    }

    public class SubWindow : IWindow
    {
        public int WINDOW_ID
        {
            get
            {
                return 485087 + windowIndex;
            }
        }

        public readonly static int WINDOW_WIDTH = 280;
        public readonly static int WINDOW_HEIGHT = 480;

        private SubWindowUIBase[] _uiList = null;

        public static readonly List<SubWindowType> SubWindowTypes =
                Enumerable.Range(0, (int) SubWindowType.Max)
                .Cast<SubWindowType>()
                .ToList();

        public int windowIndex { get; set; }

        private Rect _windowRect = new Rect(
            0,
            0,
            WINDOW_WIDTH,
            WINDOW_HEIGHT
        );
        public Rect windowRect
        {
            get
            {
                return _windowRect;
            }
            set
            {
                _windowRect = value;
            }
        }

        private bool _isShowWnd;
        public bool isShowWnd
        {
            get
            {
                return _isShowWnd;
            }
            set
            {
                if (value == _isShowWnd)
                {
                    return;
                }

                _isShowWnd = value;
                dirty = true;
            }
        }

        private SubWindowType _subWindowType;
        public SubWindowType subWindowType
        {
            get
            {
                return _subWindowType;
            }
            set
            {
                if (value == _subWindowType)
                {
                    return;
                }

                _subWindowType = value;
                dirty = true;
            }
        }

        private bool _isPositionLocked;
        public bool isPositionLocked
        {
            get
            {
                return _isPositionLocked;
            }
            set
            {
                if (value == _isPositionLocked)
                {
                    return;
                }

                _isPositionLocked = value;
                dirty = true;
            }
        }

        public bool dirty;

        public SubWindowUIBase ui
        {
            get
            {
                return GetSubWindowUI(subWindowType);
            }
        }

        public SubWindowInfo info
        {
            get
            {
                return config.GetSubWindowInfo(windowIndex);
            }
        }

        public static Config config => ConfigManager.instance.config;

        private static StudioHackBase studioHack => StudioHackManager.instance.studioHack;

        private static MaidManager maidManager => MaidManager.instance;

        private static Maid maid => maidManager.maid;

        private static WindowManager windowManager => WindowManager.instance;

        public SubWindow(int windowIndex)
        {
            this.windowIndex = windowIndex;
            this._isShowWnd = true;

            var info = this.info;
            this._windowRect.position = info.position;
            this._subWindowType = info.subWindowType;
            this._isPositionLocked = info.isPositionLocked;

            this._uiList = new SubWindowUIBase[]
            {
                new TimelineLoadUI(this),
                new KeyFrameUI(this),
                new TimelineLayerUI(this),
                new IKHoldUI(this),
                new TimelineTrackUI(this),
                new TimelineHistoryUI(this),
                new TimelineSettingUI(this),
            };
        }

        public void Update()
        {
            UpdatePosition();
        }

        public void SetSubWindowType(SubWindowType type)
        {
            subWindowType = type;
            isShowWnd = true;
        }

        public SubWindowUIBase GetSubWindowUI(SubWindowType type)
        {
            return _uiList[(int) type];
        }

        public void OnGUI()
        {
            if (isShowWnd)
            {
                var gsWin = windowManager.mainWindow.gsWin;
                _windowRect = GUI.Window(WINDOW_ID, windowRect, DrawWindow, ui.title, gsWin);
                PluginUtils.ResetInputOnScroll(_windowRect);
            }
        }

        private void DrawWindow(int id)
        {
            if (studioHack == null || maid == null)
            {
                return;
            }
            if (!studioHack.IsValid())
            {
                return;
            }

            ui.DrawWindow(id);
        }

        private Rect _prevFocusedRect = new Rect(0, 0, 0, 0);

        public void UpdatePosition()
        {
            if (isPositionLocked)
            {
                var focusedWindow = windowManager.GetWindow(windowIndex - 1);

                if (!Input.GetMouseButton(0) || _prevFocusedRect != focusedWindow.windowRect)
                {
                    var source = focusedWindow.windowRect;
                    _windowRect.x = source.x - WINDOW_WIDTH;
                    _windowRect.y = source.y;
                    _prevFocusedRect = focusedWindow.windowRect;
                }
            }
        }

        public void UpdateInfo()
        {
            var info = this.info;
            info.position = _windowRect.position;
            info.subWindowType = subWindowType;
            info.isPositionLocked = isPositionLocked;
        }

        public void OnScreenSizeChanged()
        {
            PluginUtils.AdjustWindowPosition(ref _windowRect);
        }
    }
}