using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public abstract class SubWindowUIBase
    {
        public SubWindow subWindow { get; private set; }
        public abstract string title { get; }

        public static int WINDOW_WIDTH => SubWindow.WINDOW_WIDTH;
        public static int WINDOW_HEIGHT => SubWindow.WINDOW_HEIGHT;
        protected static TimelineManager timelineManager => TimelineManager.instance;
        protected static TimelineData timeline => timelineManager.timeline;
        protected static MaidManager maidManager => MaidManager.instance;
        protected static StudioModelManager modelManager => StudioModelManager.instance;
        protected static StudioHackManager studioHackManager => StudioHackManager.instance;
        protected static StudioHackBase studioHack => StudioHackManager.instance.studioHack;
        protected static ModelHackManager modelHackManager => ModelHackManager.instance;
        protected static WindowManager windowManager => WindowManager.instance;
        protected static MovieManager movieManager => MovieManager.instance;
        protected static BGMManager bgmManager => BGMManager.instance;
        protected static GridViewManager gridViewManager => GridViewManager.instance;
        protected static CameraManager cameraManager =>  CameraManager.instance;
        protected static TimelineBundleManager bundleManager => TimelineBundleManager.instance;
        protected static ITimelineLayer currentLayer => timelineManager.currentLayer;
        protected static Config config => ConfigManager.instance.config;

        protected Rect windowRect => subWindow.windowRect;

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

                var icon = subWindow.isPositionLocked ? bundleManager.lockIcon : bundleManager.unlockIcon;
                if (view.DrawTextureButton(icon, 20, 20))
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