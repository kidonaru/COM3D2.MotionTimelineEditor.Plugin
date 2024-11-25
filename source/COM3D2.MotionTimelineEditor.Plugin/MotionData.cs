namespace COM3D2.MotionTimelineEditor.Plugin
{
    public interface IMotionData
    {
        int stFrame { get; set; }
        int edFrame { get; set; }
        int stFrameInEdit { get; set; }
        int edFrameInEdit { get; set; }
        int stFrameActive { get; }
        int edFrameActive { get; }
    }

    public abstract class MotionDataBase : IMotionData
    {
        public int stFrame { get; set; }
        public int edFrame { get; set; }
        public int stFrameInEdit { get; set; }
        public int edFrameInEdit { get; set; }

        private static StudioHackBase studioHack
        {
            get
            {
                return StudioHackManager.studioHack;
            }
        }

        public int stFrameActive
        {
            get
            {
                return studioHack.isPoseEditing ? stFrameInEdit : stFrame;
            }
        }

        public int edFrameActive
        {
            get
            {
                return studioHack.isPoseEditing ? edFrameInEdit : edFrame;
            }
        }
    }

    public class MotionData : MotionDataBase
    {
        public BoneData start;
        public BoneData end;

        public string name
        {
            get
            {
                return start.name;
            }
        }

        public MotionData(BoneData start, BoneData end)
        {
            this.start = start;
            this.end = end;
            stFrame = start.frameNo;
            edFrame = end.frameNo;
        }
    }
}