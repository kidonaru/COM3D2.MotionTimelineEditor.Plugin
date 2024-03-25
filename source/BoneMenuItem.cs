namespace COM3D2.MotionTimelineEditor.Plugin
{
    using System.Collections.Generic;
    using SH = StudioHack;

    public class BoneMenuItem : IBoneMenuItem
    {
        public readonly IKManager.BoneType boneType;
        public readonly string bonePath;

        private bool _isSelectedMenu = false;
        public bool isSelectedMenu
        {
            get
            {
                if (!SH.HasBoneRotateVisible(boneType))
                {
                    return _isSelectedMenu;
                }
                return SH.IsBoneRotateVisible(boneType);
            }
            set
            {
                if (!SH.HasBoneRotateVisible(boneType))
                {
                    _isSelectedMenu = value;
                    return;
                }
                SH.SetBoneRotateVisible(boneType, value);
            }
        }

        private bool _isVisibleMenu = true;
        public bool isVisibleMenu
        {
            get
            {
                return _isVisibleMenu;
            }
            set
            {
                _isVisibleMenu = value;
            }
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

        public string diplayName
        {
            get
            {
                return BoneUtils.GetBoneJpName(boneType);
            }
        }

        private static BoneMenuManager boneMenuManager
        {
            get
            {
                return BoneMenuManager.Instance;
            }
        }

        private static TimelineManager timelineManager
        {
            get
            {
                return TimelineManager.instance;
            }
        }

        public BoneMenuItem(IKManager.BoneType boneType)
        {
            this.boneType = boneType;
            this.bonePath = BoneUtils.GetBonePath(boneType);
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
            var bone = frame.GetBone(bonePath);
            return bone != null;
        }

        public bool HasFullBone(FrameData frame)
        {
            return HasBone(frame);
        }

        public bool IsTargetBone(BoneData bone)
        {
            return bone.bonePath == bonePath;
        }

        public bool IsSelectedFrame(FrameData frame)
        {
            var bone = frame.GetBone(bonePath);
            return timelineManager.IsSelectedBone(frame, bone);
        }

        public void SelectFrame(FrameData frame, bool isMultiSelect)
        {
            var bone = frame.GetBone(bonePath);
            if (bone == null)
            {
                return;
            }

            var bones = new List<BoneData> { bone };
            timelineManager.SelectBones(frame, bones, isMultiSelect);
        }
    }
}