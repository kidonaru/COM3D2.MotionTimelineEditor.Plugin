using System.Collections.Generic;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TransformDataGrounding : TransformDataBase
    {
        public override TransformType type
        {
            get
            {
                return TransformType.Grounding;
            }
        }

        public override int valueCount
        {
            get
            {
                return 7;
            }
        }

        public TransformDataGrounding()
        {
        }

        private readonly static Dictionary<string, CustomValueInfo> CustomValueInfoMap = new Dictionary<string, CustomValueInfo>
        {
            {
                "isGroundingFootL",
                new CustomValueInfo
                {
                    index = 0,
                    name = "左足の接地",
                    defaultValue = 0,
                }
            },
            {
                "floorHeight",
                new CustomValueInfo
                {
                    index = 1,
                    name = "床の高さ",
                    defaultValue = 0f,
                }
            },
            {
                "footBaseOffset",
                new CustomValueInfo
                {
                    index = 2,
                    name = "足首の高さ",
                    defaultValue = 0.05f,
                }
            },
            {
                "footStretchHeight",
                new CustomValueInfo
                {
                    index = 3,
                    name = "伸ばす高さ",
                    defaultValue = 0.1f,
                }
            },
            {
                "footStretchAngle",
                new CustomValueInfo
                {
                    index = 4,
                    name = "伸ばす角度",
                    defaultValue = 45f,
                }
            },
            {
                "footGroundAngle",
                new CustomValueInfo
                {
                    index = 5,
                    name = "接地時角度",
                    defaultValue = 90f,
                }
            },
            {
                "isGroundingFootR",
                new CustomValueInfo
                {
                    index = 6,
                    name = "右足の接地",
                    defaultValue = 0,
                }
            },
        };

        public override Dictionary<string, CustomValueInfo> GetCustomValueInfoMap()
        {
            return CustomValueInfoMap;
        }

        public ValueData isGroundingFootLValue
        {
            get
            {
                return this["isGroundingFootL"];
            }
        }

        public ValueData floorHeightValue
        {
            get
            {
                return this["floorHeight"];
            }
        }

        public ValueData footBaseOffsetValue
        {
            get
            {
                return this["footBaseOffset"];
            }
        }

        public ValueData footStretchHeightValue
        {
            get
            {
                return this["footStretchHeight"];
            }
        }

        public ValueData footStretchAngleValue
        {
            get
            {
                return this["footStretchAngle"];
            }
        }

        public ValueData footGroundAngleValue
        {
            get
            {
                return this["footGroundAngle"];
            }
        }

        public ValueData isGroundingFootRValue
        {
            get
            {
                return this["isGroundingFootR"];
            }
        }

        public bool isGroundingFootL
        {
            get
            {
                return isGroundingFootLValue.boolValue;
            }
            set
            {
                isGroundingFootLValue.boolValue = value;
            }
        }

        public float floorHeight
        {
            get
            {
                return floorHeightValue.value;
            }
            set
            {
                floorHeightValue.value = value;
            }
        }

        public float footBaseOffset
        {
            get
            {
                return footBaseOffsetValue.value;
            }
            set
            {
                footBaseOffsetValue.value = value;
            }
        }

        public float footStretchHeight
        {
            get
            {
                return footStretchHeightValue.value;
            }
            set
            {
                footStretchHeightValue.value = value;
            }
        }

        public float footStretchAngle
        {
            get
            {
                return footStretchAngleValue.value;
            }
            set
            {
                footStretchAngleValue.value = value;
            }
        }

        public float footGroundAngle
        {
            get
            {
                return footGroundAngleValue.value;
            }
            set
            {
                footGroundAngleValue.value = value;
            }
        }

        public bool isGroundingFootR
        {
            get
            {
                return isGroundingFootRValue.boolValue;
            }
            set
            {
                isGroundingFootRValue.boolValue = value;
            }
        }
    }
}