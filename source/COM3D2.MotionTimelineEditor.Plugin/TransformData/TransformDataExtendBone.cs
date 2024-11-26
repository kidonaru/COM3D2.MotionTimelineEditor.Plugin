using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TransformDataExtendBone : TransformDataBase
    {
        public override int valueCount
        {
            get
            {
                return 10;
            }
        }

        public override bool hasPosition
        {
            get
            {
                return true;
            }
        }

        public override bool hasRotation
        {
            get
            {
                return true;
            }
        }

        public override bool hasScale
        {
            get
            {
                return true;
            }
        }

        public override bool hasTangent
        {
            get
            {
                return true;
            }
        }

        public override ValueData[] positionValues
        {
            get
            {
                return new ValueData[] { values[4], values[5], values[6] };
            }
        }

        public override ValueData[] rotationValues
        {
            get
            {
                return new ValueData[] { values[0], values[1], values[2], values[3] };
            }
        }

        public override ValueData[] scaleValues
        {
            get
            {
                return new ValueData[] { values[7], values[8], values[9] };
            }
        }

        public override ValueData[] tangentValues
        {
            get
            {
                return values;
            }
        }

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
            get
            {
                return Quaternion.Euler(initialEulerAngles);
            }
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

        public TransformDataExtendBone()
        {
        }
    }
}