using System.Collections.Generic;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public interface IBoneMenuItem
    {
        string name { get; }
        string displayName { get; }
        bool isSelectedMenu { get; set; }
        bool isVisibleMenu { get; set; }
        bool isOpenMenu { get; set; }
        bool isSetMenu { get; }

        IBoneMenuItem parent { get; set; }
        List<IBoneMenuItem> children { get; }

        void SelectMenu(bool isMultiSelect);
        bool HasVisibleBone(FrameData frame);
        bool IsFullBones(FrameData frame);
        bool IsTargetBone(BoneData bone);
        bool IsSelectedFrame(FrameData frame);
        void SelectFrame(FrameData frame, bool isMultiSelect);
        void AddKey();
        void RemoveKey();
    }
}