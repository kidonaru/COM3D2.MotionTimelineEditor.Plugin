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

        private static StudioHackBase studioHack => StudioHackManager.studioHack;

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
        public ITransformData start;
        public ITransformData end;

        public string name
        {
            get
            {
                return start.name;
            }
        }

        public int frameNo
        {
            get
            {
                return stFrame;
            }
        }

        public MotionData(BoneData start, BoneData end)
        {
            this.start = start.transform;
            this.end = end.transform;
            this.stFrame = start.frameNo;
            this.edFrame = end.frameNo;
        }

        public MotionData(
            ITransformData start,
            ITransformData end,
            int stFrame,
            int edFrame)
        {
            this.start = start;
            this.end = end;
            this.stFrame = stFrame;
            this.edFrame = edFrame;
        }

        public MotionData Clone()
        {
            var newStart = start.Clone();
            var newEnd = end.Clone();
            return new MotionData(newStart, newEnd, stFrame, edFrame);
        }
    }
}