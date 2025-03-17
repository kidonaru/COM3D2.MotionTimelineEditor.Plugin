
namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TransformDataBG : TransformDataBase
    {
        public override TransformType type => TransformType.BG;

        public override int valueCount => 9;

        public override bool hasPosition => true;
        public override bool hasEulerAngles => true;
        public override bool hasScale => true;

        public override ValueData[] positionValues
        {
            get => new ValueData[] { values[0], values[1], values[2] };
        }

        public override ValueData[] eulerAnglesValues
        {
            get => new ValueData[] { values[3], values[4], values[5] };
        }

        public override ValueData[] scaleValues
        {
            get => new ValueData[] { values[6], values[7], values[8] };
        }

        public TransformDataBG()
        {
        }
    }
}