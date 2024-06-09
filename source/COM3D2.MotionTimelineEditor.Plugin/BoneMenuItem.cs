using System.Collections.Generic;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class BoneMenuItem : IBoneMenuItem
    {
        public string name { get; private set; }
        public string displayName { get; private set; }

        public virtual bool isSelectedMenu { get; set; }

        public bool isVisibleMenu
        {
            get
            {
                return parent == null || parent.isOpenMenu;
            }
            set {}
        }

        public bool isOpenMenu
        {
            get
            {
                return false;
            }
            set {}
        }

        public bool isSetMenu
        {
            get
            {
                return false;
            }
        }

        public IBoneMenuItem parent { get; set; }

        public List<IBoneMenuItem> children
        {
            get
            {
                return null;
            }
        }

        protected static StudioHackBase studioHack
        {
            get
            {
                return StudioHackManager.studioHack;
            }
        }

        protected static BoneMenuManager boneMenuManager
        {
            get
            {
                return BoneMenuManager.Instance;
            }
        }

        protected static TimelineManager timelineManager
        {
            get
            {
                return TimelineManager.instance;
            }
        }

        public BoneMenuItem(string name, string displayName)
        {
            this.name = name;
            this.displayName = displayName;
        }

        public void SelectMenu(bool isMultiSelect)
        {
            var prevSelected = isSelectedMenu;

            if (!isMultiSelect)
            {
                boneMenuManager.UnselectAll();
            }

            isSelectedMenu = !prevSelected;
        }

        public bool HasBone(FrameData frame)
        {
            var bone = frame.GetBone(name);
            return bone != null;
        }

        public bool HasFullBone(FrameData frame)
        {
            return HasBone(frame);
        }

        public bool IsTargetBone(BoneData bone)
        {
            return bone.name == name;
        }

        public bool IsSelectedFrame(FrameData frame)
        {
            var bone = frame.GetBone(name);
            return timelineManager.IsSelectedBone(bone);
        }

        public void SelectFrame(FrameData frame, bool isMultiSelect)
        {
            var bone = frame.GetBone(name);
            if (bone == null)
            {
                return;
            }

            var bones = new List<BoneData> { bone };
            timelineManager.SelectBones(bones, isMultiSelect);
        }
    }
}