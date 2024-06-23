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

        private readonly static Dictionary<string, CustomValueInfo> CustomValueInfoMap = new Dictionary<string, CustomValueInfo>
        {
            {
                "distance", new CustomValueInfo
                {
                    index = 7,
                    name = "距離",
                    defaultValue = 1f,
                }
            },
            {
                "viewAngle", new CustomValueInfo
                {
                    index = 8,
                    name = "FoV",
                    defaultValue = 35f,
                }
            },
        };

        public override Dictionary<string, CustomValueInfo> GetCustomValueInfoMap()
        {
            return CustomValueInfoMap;
        }
    }
}