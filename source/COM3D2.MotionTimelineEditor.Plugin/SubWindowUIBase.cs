using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public abstract class SubWindowUIBase
    {
        public SubWindow subWindow { get; private set; }
        public abstract string title { get; }

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

        protected static TimelineManager timelineManager
        {
            get
            {
                return TimelineManager.instance;
            }
        }

        protected static TimelineData timeline
        {
            get
            {
                return timelineManager.timeline;
            }
        }

        protected static MaidManager maidManager
        {
            get
            {
                return MaidManager.instance;
            }
        }

        protected static StudioModelManager modelManager
        {
            get
            {
                return StudioModelManager.instance;
            }
        }

        protected static StudioHackBase studioHack
        {
            get
            {
                return StudioHackManager.studioHack;
            }
        }
        
        protected static ModelHackManager modelHackManager
        {
            get
            {
                return ModelHackManager.instance;
            }
        }

        protected static WindowManager windowManager
        {
            get
            {
                return WindowManager.instance;
            }
        }

        protected static MovieManager movieManager
        {
            get
            {
                return MovieManager.instance;
            }
        }

        protected static BGMManager bgmManager
        {
            get
            {
                return BGMManager.instance;
            }
        }
        
        protected static ITimelineLayer currentLayer
        {
            get
            {
                return timelineManager.currentLayer;
            }
        }

        protected static Config config
        {
            get
            {
                return ConfigManager.config;
            }
        }

        protected Rect windowRect
        {
            get
            {
                return subWindow.windowRect;
            }
        }

        public SubWindowUIBase(SubWindow subWindow)
        {
            this.subWindow = subWindow;
        }

        private GUIView _headerView = new GUIView(0, 0, WINDOW_WIDTH, 20)
        {
            padding = Vector2.zero,
            margin = 0,
        };
        private GUIView _contentView = new GUIView(0, 0, WINDOW_WIDTH, WINDOW_HEIGHT);
        private GUIComboBox<SubWindowType> _subWindowTypeComboBox = null;

        public static Texture2D texLock = null;

        public virtual void InitWindow()
        {
            if (_subWindowTypeComboBox == null)
            {
                _subWindowTypeComboBox = new GUIComboBox<SubWindowType>
                {
                    defaultName = "切替",
                    items = SubWindow.SubWindowTypes,
                    getName = (type, index) => subWindow.GetSubWindowUI(type).title,
                    onSelected = (type, index) => subWindow.subWindowType = type,
                    buttonSize = new Vector2(40, 20),
                    contentSize = new Vector2(140, 300),
                };
            }

            if (texLock == null)
            {
                texLock = new Texture2D(0, 0);
                texLock.LoadImage(PluginUtils.LockIcon);
            }

            _contentView.ResetLayout();
            _contentView.SetEnabled(!_contentView.IsComboBoxFocused());
            _contentView.padding = GUIView.defaultPadding;
            _contentView.margin = GUIView.defaultMargin;
            _contentView.currentPos.y = 20;

            _headerView.ResetLayout();
            _headerView.parent = _contentView;
        }

        public virtual void DrawWindow(int id)
        {
            InitWindow();

            DrawHeader(_headerView);
            DrawContent(_contentView);

            _contentView.DrawComboBox();

            if (!IsDragging())
            {
                GUI.DragWindow();
            }
        }

        public virtual void DrawHeader(GUIView view)
        {
            view.BeginHorizontal();
            {
                view.currentPos.x = 140;

                _subWindowTypeComboBox.currentIndex = (int) subWindow.subWindowType;
                _subWindowTypeComboBox.DrawButton(view);

                if (view.DrawButton("+", 20, 20))
                {
                    windowManager.AddSubWindow();
                }

                var lockColor = subWindow.isPositionLocked ? Color.white : Color.gray;
                if (view.DrawTextureButton(texLock, 20, 20, lockColor))
                {
                    subWindow.isPositionLocked = !subWindow.isPositionLocked;
                }

                if (view.DrawButton("x", 20, 20))
                {
                    subWindow.isShowWnd = false;
                }
            }
            view.EndLayout();
        }

        public abstract void DrawContent(GUIView view);

        public virtual bool IsDragging()
        {
            return false;
        }
    }
}