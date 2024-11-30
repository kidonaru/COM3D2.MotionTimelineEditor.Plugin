using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TransformDataRoot : TransformDataBase
    {
        public override TransformType type => TransformType.Root;

        public override int valueCount => 7;

        public override bool hasPosition => true;

        public override bool hasRotation => true;

        public override bool hasTangent => true;

        public override ValueData[] positionValues
        {
            get => new ValueData[] { values[4], values[5], values[6] };
        }

        public override ValueData[] rotationValues
        {
            get => new ValueData[] { values[0], values[1], values[2], values[3] };
        }

        public override ValueData[] tangentValues => values;

        public override Vector3 initialPosition
        {
            get
            {
                if (maidCache != null)
                {
                    return maidCache.GetInitialPosition(name);
                }
                return Vector3.zero;
            }
        }

        public override Quaternion initialRotation
        {
            get => Quaternion.Euler(initialEulerAngles);
        }

        public override Vector3 initialEulerAngles
        {
            get
            {
                if (maidCache != null)
                {
                    return maidCache.GetInitialEulerAngles(name);
                }
                return Vector3.zero;
            }
        }

        public override SingleFrameType singleFrameType => SingleFrameType.None;

        public TransformDataRoot()
        {
        }
    }
}