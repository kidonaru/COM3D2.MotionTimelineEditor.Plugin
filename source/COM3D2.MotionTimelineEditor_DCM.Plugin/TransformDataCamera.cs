using System.Collections.Generic;
using COM3D2.MotionTimelineEditor.Plugin;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor_DCM.Plugin
{
    public class TransformDataCamera : TransformDataBase
    {
        public override int valueCount
        {
            get
            {
                return 9;
            }
        }

        public override bool hasPosition
        {
            get
            {
                return true;
            }
        }

        public override bool hasEulerAngles
        {
            get
            {
                return true;
            }
        }

        public override bool hasEasing
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
                return new ValueData[] { values[0], values[1], values[2] };
            }
        }

        public override ValueData[] eulerAnglesValues
        {
            get
            {
                return new ValueData[] { values[3], values[4], values[5] };
            }
        }

        public override ValueData easingValue
        {
            get
            {
                return values[6];
            }
        }

        public TransformDataCamera()
        {
        }

        private readonly static Dictionary<string, int> CustomValueIndexMap = new Dictionary<string, int>
        {
            { "distance", 7 },
            { "viewAngle", 8 }
        };

        public override Dictionary<string, int> GetCustomValueIndexMap()
        {
            return CustomValueIndexMap;
        }

        public override float GetInitialCustomValue(string customName)
        {
            if (customName == "distance")
            {
                return 1f;
            }
            if (customName == "viewAngle")
            {
                return 35f;
            }
            return 0f;
        }
    }
}