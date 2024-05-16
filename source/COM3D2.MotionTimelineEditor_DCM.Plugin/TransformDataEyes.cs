using System.Collections.Generic;
using COM3D2.MotionTimelineEditor.Plugin;

namespace COM3D2.MotionTimelineEditor_DCM.Plugin
{
    public class TransformDataEyes : TransformDataBase
    {
        public override int valueCount
        {
            get
            {
                return 3;
            }
        }

        public override bool hasPosition
        {
            get
            {
                return false;
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
                return false;
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

        public TransformDataEyes()
        {
        }

        public override ValueData[] GetPositionValues()
        {
            return new ValueData[0];
        }

        public override ValueData[] GetRotationValues()
        {
            return new ValueData[0];
        }

        public override ValueData[] GetEulerAnglesValues()
        {
            return new ValueData[0];
        }

        public override ValueData[] GetScaleValues()
        {
            return new ValueData[0];
        }

        public override ValueData GetEasingValue()
        {
            return values[0];
        }

        private readonly static Dictionary<string, int> CustomValueIndexMap = new Dictionary<string, int>
        {
            { "horizon", 1 },
            { "vertical", 2 },
        };

        public override Dictionary<string, int> GetCustomValueIndexMap()
        {
            return CustomValueIndexMap;
        }

        public override float GetResetCustomValue(string customName)
        {
            if (this.name == "EyesScaL" || this.name == "EyesScaR")
            {
                return 1f;
            }
            return 0f;
        }
    }
}