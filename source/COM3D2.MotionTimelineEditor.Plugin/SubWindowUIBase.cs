namespace COM3D2.MotionTimelineEditor.Plugin
{
    using UnityEngine;
    using MTE = MotionTimelineEditor;

    public abstract class SubWindowUIBase
    {
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
                return MTE.studioHack;
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
                return MTE.config;
            }
        }

        protected static Rect rc_stgw
        {
            get
            {
                return SubWindow.rc_stgw;
            }
        }

        public virtual void OnOpen()
        {
        }

        public virtual void OnClose()
        {
        }

        public virtual void Init()
        {
        }

        public virtual void Update()
        {
        }

        public virtual void LateUpdate()
        {
        }

        public abstract void DrawWindow(int id);
    }
}