using System.Collections.Generic;
using COM3D2.MotionTimelineEditor.Plugin;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor_DCM.Plugin
{
    public class TransformDataLight : TransformDataBase
    {
        public override int valueCount
        {
            get
            {
                return 15;
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

        public override bool hasColor
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

        public override ValueData[] colorValues
        {
            get
            {
                return new ValueData[] { values[6], values[7], values[8] };
            }
        }

        public override ValueData easingValue
        {
            get
            {
                return values[9];
            }
        }

        public TransformDataLight()
        {
        }

        private readonly static Dictionary<string, CustomValueInfo> CustomValueInfoMap = new Dictionary<string, CustomValueInfo>
        {
            {
                "range", new CustomValueInfo
                {
                    index = 10,
                    name = "範囲",
                    defaultValue = 3f,
                }
            },
            {
                "intensity", new CustomValueInfo
                {
                    index = 11,
                    name = "強度",
                    defaultValue = 0.95f,
                }
            },
            {
                "spotAngle", new CustomValueInfo
                {
                    index = 12,
                    name = "角度",
                    defaultValue = 50f,
                }
            },
            {
                "shadowStrength", new CustomValueInfo
                {
                    index = 13,
                    name = "影濃",
                    defaultValue = 0.1f,
                }
            },
            {
                "shadowBias", new CustomValueInfo
                {
                    index = 14,
                    name = "影距",
                    defaultValue = 0.01f,
                }
            },
        };

        public override Dictionary<string, CustomValueInfo> GetCustomValueInfoMap()
        {
            return CustomValueInfoMap;
        }
    }
}