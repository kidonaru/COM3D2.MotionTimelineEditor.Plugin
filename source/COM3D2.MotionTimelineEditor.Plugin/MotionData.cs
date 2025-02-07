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

        private static StudioHackManager studioHackManager => StudioHackManager.instance;

        public int stFrameActive
        {
            get => studioHackManager.isPoseEditing ? stFrameInEdit : stFrame;
        }

        public int edFrameActive
        {
            get => studioHackManager.isPoseEditing ? edFrameInEdit : edFrame;
        }
    }

    public class MotionData : MotionDataBase
    {
        public ITransformData start;
        public ITransformData end;

        public string name => start.name;
        public int frameNo => stFrame;
        public int easing
        {
            get
            {
                if (timeline?.isEasingAppliedToNextKeyframe ?? false)
                {
                    return start.easing;
                }

                return end.easing;
            }
        }

        private static TimelineData timeline => TimelineManager.instance.timeline;

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