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
            get => parent == null || parent.isOpenMenu;
            set {}
        }

        public bool isOpenMenu
        {
            get => false;
            set {}
        }

        public bool isSetMenu
        {
            get => false;
        }

        public IBoneMenuItem parent { get; set; }

        public List<IBoneMenuItem> children => null;

        protected static StudioHackBase studioHack => StudioHackManager.instance.studioHack;

        protected static BoneMenuManager boneMenuManager => BoneMenuManager.Instance;

        protected static TimelineManager timelineManager => TimelineManager.instance;

        protected static ITimelineLayer currentLayer => timelineManager.currentLayer;

        protected static StudioModelManager modelManager => StudioModelManager.instance;

        protected static IPartsEditHack partsEditHack => PartsEditHackManager.instance.partsEditHack;

        protected static MaidCache maidCache => MaidManager.instance.maidCache;

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

        public bool HasVisibleBone(FrameData frame)
        {
            var bone = frame != null ? frame.GetBone(name) : null;
            return bone != null;
        }

        public bool IsFullBones(FrameData frame)
        {
            return HasVisibleBone(frame);
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

        public void AddKey()
        {
            var boneNames = new List<string> { name };
            currentLayer.AddKeyFrames(boneNames);
        }

        public void RemoveKey()
        {
            var boneNames = new List<string> { name };
            currentLayer.RemoveKeyFrames(boneNames);
        }
    }
}