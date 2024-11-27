using System.Collections.Generic;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TransformDataLight : TransformDataBase
    {
        public override TransformType type
        {
            get
            {
                return TransformType.Light;
            }
        }

        public override int valueCount
        {
            get
            {
                return 16;
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
            {
                "maidSlotNo", new CustomValueInfo
                {
                    index = 15,
                    name = "追従",
                    defaultValue = -1f,
                }
            },
        };

        public override Dictionary<string, CustomValueInfo> GetCustomValueInfoMap()
        {
            return CustomValueInfoMap;
        }

        public ValueData rangeValue
        {
            get
            {
                return this["range"];
            }
        }

        public ValueData intensityValue
        {
            get
            {
                return this["intensity"];
            }
        }

        public ValueData spotAngleValue
        {
            get
            {
                return this["spotAngle"];
            }
        }

        public ValueData shadowStrengthValue
        {
            get
            {
                return this["shadowStrength"];
            }
        }

        public ValueData shadowBiasValue
        {
            get
            {
                return this["shadowBias"];
            }
        }

        public ValueData maidSlotNoValue
        {
            get
            {
                return this["maidSlotNo"];
            }
        }

        public float range
        {
            get
            {
                return rangeValue.value;
            }
            set
            {
                rangeValue.value = value;
            }
        }

        public float intensity
        {
            get
            {
                return intensityValue.value;
            }
            set
            {
                intensityValue.value = value;
            }
        }

        public float spotAngle
        {
            get
            {
                return spotAngleValue.value;
            }
            set
            {
                spotAngleValue.value = value;
            }
        }

        public float shadowStrength
        {
            get
            {
                return shadowStrengthValue.value;
            }
            set
            {
                shadowStrengthValue.value = value;
            }
        }

        public float shadowBias
        {
            get
            {
                return shadowBiasValue.value;
            }
            set
            {
                shadowBiasValue.value = value;
            }
        }

        public int maidSlotNo
        {
            get
            {
                return maidSlotNoValue.intValue;
            }
            set
            {
                maidSlotNoValue.intValue = value;
            }
        }
    }
}