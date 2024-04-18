namespace COM3D2.MotionTimelineEditor.Plugin
{
    public interface IBoneMenuItem
    {
        bool isSelectedMenu { get; set; }
        bool isVisibleMenu { get; set; }
        bool isOpenMenu { get; set; }
        bool isSetMenu { get; }
        string diplayName { get; }

        void SelectMenu(bool isMultiSelect);
        bool HasBone(FrameData frame);
        bool HasFullBone(FrameData frame);
        bool IsTargetBone(BoneData bone);
        bool IsSelectedFrame(FrameData frame);
        void SelectFrame(FrameData frame, bool isMultiSelect);
    }
}