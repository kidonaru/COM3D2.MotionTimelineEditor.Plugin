

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TransformDataMove : TransformDataBase
    {
        public override TransformType type => TransformType.Move;

        public override int valueCount => 11;

        public override bool hasPosition => true;
        public override bool hasRotation => true;
        public override bool hasScale => true;
        public override bool hasEasing => !timeline.isTangentMove;
        public override bool hasTangent => timeline.isTangentMove;

        public override ValueData[] positionValues
        {
            get => new ValueData[] { values[0], values[1], values[2] };
        }
        public override ValueData[] rotationValues
        {
            get => new ValueData[] { values[3], values[4], values[5], values[6] };
        }
        public override ValueData[] scaleValues
        {
            get => new ValueData[] { values[8], values[9], values[10] };
        }

        public override ValueData easingValue => values[7];

        public override ValueData[] tangentValues => baseValues;

        public TransformDataMove()
        {
        }
    }
}