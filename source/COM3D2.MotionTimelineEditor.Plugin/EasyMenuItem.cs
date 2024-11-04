using System.Collections.Generic;
using System.Linq;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class EasyMenuItem : IBoneMenuItem
    {
        public string name
        {
            get
            {
                return "KeyFrame";
            }
        }

        public string displayName
        {
            get
            {
                return "キーフレーム";
            }
        }

        public bool isSelectedMenu
        {
            get
            {
                return false;
            }
            set {}
        }

        public bool isVisibleMenu
        {
            get
            {
                return true;
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

        private static BoneMenuManager boneItemManager
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

        protected static ITimelineLayer currentLayer
        {
            get
            {
                return timelineManager.currentLayer;
            }
        }

        public void SelectMenu(bool isMultiSelect)
        {
            // do nothing
        }

        public bool HasVisibleBone(FrameData frame)
        {
            return frame != null && frame.bones.Count > 0;
        }

        public bool IsFullBones(FrameData frame)
        {
            return frame != null && frame.isFullBone;
        }

        public bool IsTargetBone(BoneData bone)
        {
            return bone != null;
        }

        public bool IsSelectedFrame(FrameData frame)
        {
            return frame.bones.Any(bone => timelineManager.IsSelectedBone(bone));
        }

        public void SelectFrame(FrameData frame, bool isMultiSelect)
        {
            timelineManager.SelectBones(frame.bones.ToList(), isMultiSelect);
        }

        public void AddKey()
        {
            currentLayer.AddKeyFrameAll();
        }

        public void RemoveKey()
        {
            currentLayer.RemoveKeyFrames(currentLayer.allBoneNames);
        }
    }
}