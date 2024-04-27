
using COM3D2.MotionTimelineEditor.Plugin;

namespace COM3D2.MotionTimelineEditor_DCM.Plugin
{
    public class TransformDataMove : TransformDataBase
    {
        public override int valueCount
        {
            get
            {
                return 7;
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
                return false;
            }
        }

        public override bool hasEulerAngles
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
                return false;
            }
        }

        public override bool hasEasing
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
                return false;
            }
        }

        public TransformDataMove()
        {
        }

        public override ValueData[] GetPositionValues()
        {
            return new ValueData[] { values[0], values[1], values[2] };
        }

        public override ValueData[] GetRotationValues()
        {
            return new ValueData[0];
        }

        public override ValueData[] GetEulerAnglesValues()
        {
            return new ValueData[] { values[3], values[4], values[5] };
        }

        public override ValueData[] GetScaleValues()
        {
            return new ValueData[0];
        }

        public override ValueData GetEasingValue()
        {
            return values[6];
        }
    }
}