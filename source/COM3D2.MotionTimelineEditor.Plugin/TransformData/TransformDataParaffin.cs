using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TransformDataParaffin : TransformDataBase
    {
        public override int valueCount
        {
            get
            {
                return 21;
            }
        }

        public override bool hasColor
        {
            get
            {
                return true;
            }
        }

        public override bool hasSubColor
        {
            get
            {
                return true;
            }
        }

        public override bool hasVisible
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

        public override ValueData[] colorValues
        {
            get
            {
                return new ValueData[] { values[0], values[1], values[2], values[3] };
            }
        }

        public override ValueData[] subColorValues
        {
            get
            {
                return new ValueData[] { values[4], values[5], values[6], values[7] };
            }
        }

        public override ValueData visibleValue
        {
            get
            {
                return values[8];
            }
        }

        public override ValueData easingValue
        {
            get
            {
                return values[9];
            }
        }

        public override Color initialColor
        {
            get
            {
                return new Color(1f, 1f, 1f, 1f);
            }
        }

        public override Color initialSubColor
        {
            get
            {
                return new Color(1f, 1f, 1f, 0f);
            }
        }

        public TransformDataParaffin()
        {
        }

        private readonly static Dictionary<string, CustomValueInfo> CustomValueInfoMap = new Dictionary<string, CustomValueInfo>
        {
            {
                "centerPositionX", new CustomValueInfo
                {
                    index = 10,
                    name = "X",
                    defaultValue = 0.5f,
                }
            },
            {
                "centerPositionY", new CustomValueInfo
                {
                    index = 11,
                    name = "Y",
                    defaultValue = 0.5f,
                }
            },
            {
                "radiusFar", new CustomValueInfo
                {
                    index = 12,
                    name = "外半径",
                    defaultValue = 1f,
                }
            },
            {
                "radiusNear", new CustomValueInfo
                {
                    index = 13,
                    name = "内半径",
                    defaultValue = 1f,
                }
            },
            {
                "radiusScaleX", new CustomValueInfo
                {
                    index = 14,
                    name = "SX",
                    defaultValue = 1f,
                }
            },
            {
                "radiusScaleY", new CustomValueInfo
                {
                    index = 15,
                    name = "SY",
                    defaultValue = 1f,
                }
            },
            {
                "useNormal", new CustomValueInfo
                {
                    index = 16,
                    name = "通常",
                    defaultValue = 0f,
                }
            },
            {
                "useAdd", new CustomValueInfo
                {
                    index = 17,
                    name = "加算",
                    defaultValue = 0f,
                }
            },
            {
                "useMultiply", new CustomValueInfo
                {
                    index = 18,
                    name = "乗算",
                    defaultValue = 0f,
                }
            },
            {
                "useOverlay", new CustomValueInfo
                {
                    index = 19,
                    name = "オーバーレイ",
                    defaultValue = 0f,
                }
            },
            {
                "useSubstruct", new CustomValueInfo
                {
                    index = 20,
                    name = "減算",
                    defaultValue = 0f,
                }
            },
        };

        public override Dictionary<string, CustomValueInfo> GetCustomValueInfoMap()
        {
            return CustomValueInfoMap;
        }
    }
}