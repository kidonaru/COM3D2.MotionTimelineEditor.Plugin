using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TransformDataCamera : TransformDataBase
    {
        public override TransformType type => TransformType.Camera;

        public override int valueCount => 10;

        public override bool hasPosition => true;
        public override bool hasEulerAngles => true;
        public override bool hasScale => true;
        public override bool hasEasing => !timeline.isTangentCamera;
        public override bool hasTangent => timeline.isTangentCamera;

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

        public override ValueData[] tangentValues => values;

        public override Vector3 initialScale
        {
            get => new Vector3(1f, 35f, 0f); // 距離, FoV, ダミー
        }

        public TransformDataCamera()
        {
        }
    }
}