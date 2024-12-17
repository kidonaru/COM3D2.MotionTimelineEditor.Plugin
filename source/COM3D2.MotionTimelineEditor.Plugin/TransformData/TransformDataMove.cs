

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TransformDataMove : TransformDataBase
    {
        public override TransformType type => TransformType.Move;

        public override int valueCount => 10;

        public override bool hasPosition => true;
        public override bool hasEulerAngles => true;
        public override bool hasScale => true;
        public override bool hasEasing => !timeline.isTangentMove;
        public override bool hasTangent => timeline.isTangentMove;

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
            get => new ValueData[] { values[7], values[8], values[9] };
        }

        public override ValueData easingValue => values[6];

        public override ValueData[] tangentValues => baseValues;

        public TransformDataMove()
        {
        }
    }
}