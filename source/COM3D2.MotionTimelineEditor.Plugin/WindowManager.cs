using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class WindowManager
    {
        public MainWindow mainWindow = null;
        public List<SubWindow> subWindows = new List<SubWindow>();
        private int _screenWidth = 0;
        private int _screenHeight = 0;

        private static WindowManager _instance = null;
        public static WindowManager instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new WindowManager();
                }
                return _instance;
            }
        }

        private static Config config
        {
            get
            {
                return ConfigManager.config;
            }
        }

        private static Maid maid
        {
            get
            {
                return MaidManager.instance.maid;
            }
        }

        private static StudioHackBase studioHack
        {
            get
            {
                return StudioHackManager.studioHack;
            }
        }

        private WindowManager()
        {
        }

        public void Init()
        {
            TimelineManager.onRefresh += OnRefreshTimeline;

            mainWindow = new MainWindow();
            mainWindow.Init();

            subWindows.Clear();
            var subWindowCount = Mathf.Max(config.subWindowCount, 1);
            for (var i = 0; i < subWindowCount; i++)
            {
                AddSubWindow();
            };
        }

        public void Update()
        {
            mainWindow.Update();

            var removeWindows = new List<SubWindow>();

            foreach (var subWindow in subWindows)
            {
                if (subWindow.dirty)
                {
                    subWindow.UpdateInfo();
                    subWindow.dirty = false;
                }

                if (subWindow.isShowWnd)
                {
                    subWindow.Update();
                }
                else if (subWindow.windowIndex > 1)
                {
                    removeWindows.Add(subWindow);
                }
            }

            foreach (var subWindow in removeWindows)
            {
                RemoveSubWindow(subWindow.windowIndex);
            }

            bool isScreenSizeChanged = _screenWidth != Screen.width || _screenHeight != Screen.height;
            if (isScreenSizeChanged)
            {
                mainWindow.OnScreenSizeChanged();

                foreach (var subWindow in subWindows)
                {
                    subWindow.OnScreenSizeChanged();
                }

                _screenWidth = Screen.width;
                _screenHeight = Screen.height;
            }
        }

        public void OnGUI()
        {
            mainWindow.OnGUI();

            if (studioHack == null || maid == null)
            {
                return;
            }

            foreach (var subWindow in subWindows)
            {
                subWindow.OnGUI();
            }
        }

        public void AddSubWindow()
        {
            var subWindow = new SubWindow(subWindows.Count + 1);
            subWindows.Add(subWindow);
        }

        public void RemoveSubWindow(int windowIndex)
        {
            if (windowIndex <= 1)
            {
                return;
            }

            var subWindow = subWindows[windowIndex - 1];
            subWindows.Remove(subWindow);

            for (var i = 0; i < subWindows.Count; i++)
            {
                subWindows[i].windowIndex = i + 1;
            }
        }

        public IWindow GetWindow(int windowIndex)
        {
            if (windowIndex == 0)
            {
                return mainWindow;
            }

            return subWindows[windowIndex - 1];
        }

        public void UpdateConfig()
        {
            foreach (var subWindow in subWindows)
            {
                subWindow.UpdateInfo();
            }

            config.subWindowCount = subWindows.Count;
        }

        private void OnRefreshTimeline()
        {
            mainWindow.UpdateTexture();
        }
    }
}