

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TransformDataMove : TransformDataBase
    {
        public override TransformType type => TransformType.Move;

        public override int valueCount => 7;

        public override bool hasPosition => true;

        public override bool hasEulerAngles =>  true;

        public override bool hasEasing => true;

        public override ValueData[] positionValues
        {
            get => new ValueData[] { values[0], values[1], values[2] };
        }

        public override ValueData[] eulerAnglesValues
        {
            get => new ValueData[] { values[3], values[4], values[5] };
        }

        public override ValueData easingValue => values[6];

        public TransformDataMove()
        {
        }
    }
}