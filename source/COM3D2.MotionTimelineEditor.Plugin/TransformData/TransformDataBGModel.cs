using System.Collections.Generic;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TransformDataBGModel : TransformDataBase
    {
        public override TransformType type => TransformType.BGModel;

        public override int valueCount => 11;

        public override bool hasPosition => true;
        public override bool hasRotation => true;
        public override bool hasScale => true;
        public override bool hasVisible => true;
        public override bool hasTangent => true;

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
            get => new ValueData[] { values[7], values[8], values[9] };
        }

        public override ValueData visibleValue => values[10];

        public override ValueData[] tangentValues
        {
            get => baseValues;
        }

        public TransformDataBGModel()
        {
        }
    }

}