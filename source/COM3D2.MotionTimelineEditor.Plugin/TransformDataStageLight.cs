using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TransformDataStageLight : TransformDataBase
    {
        public override int valueCount
        {
            get
            {
                return 22;
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

        public override bool hasColor
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
                return new ValueData[] { values[0], values[1], values[2] };
            }
        }

        public override ValueData[] rotationValues
        {
            get
            {
                return new ValueData[] { values[3], values[4], values[5], values[6] };
            }
        }

        public override ValueData[] colorValues
        {
            get
            {
                return new ValueData[] { values[7], values[8], values[9], values[10] };
            }
        }

        public override ValueData visibleValue
        {
            get
            {
                return values[11];
            }
        }

        public override Vector3 initialPosition
        {
            get
            {
                return new Vector3(0f, 10f, 0f);
            }
        }

        public override Vector3 initialEulerAngles
        {
            get
            {
                return new Vector3(90f, 0f, 0f);
            }
        }

        public override Vector3 initialSubEulerAngles
        {
            get
            {
                return new Vector3(90f, 0f, 0f);
            }
        }

        public override Color initialColor
        {
            get
            {
                return new Color(1f, 1f, 1f, 0.3f);
            }
        }

        public TransformDataStageLight()
        {
        }

        private readonly static Dictionary<string, CustomValueInfo> CustomValueInfoMap = new Dictionary<string, CustomValueInfo>
        {
            {
                "spotAngle", new CustomValueInfo
                {
                    index = 12,
                    name = "角度",
                    defaultValue = 10f,
                }
            },
            {
                "spotRange", new CustomValueInfo
                {
                    index = 13,
                    name = "範囲",
                    defaultValue = 10f,
                }
            },
            {
                "rangeMultiplier", new CustomValueInfo
                {
                    index = 14,
                    name = "範囲補正",
                    defaultValue = 0.8f,
                }
            },
            {
                "falloffExp", new CustomValueInfo
                {
                    index = 15,
                    name = "減衰指数",
                    defaultValue = 0.5f,
                }
            },
            {
                "noiseStrength", new CustomValueInfo
                {
                    index = 16,
                    name = "ﾉｲｽﾞ強度",
                    defaultValue = 0.1f,
                }
            },
            {
                "noiseScale", new CustomValueInfo
                {
                    index = 17,
                    name = "ﾉｲｽﾞｻｲｽﾞ",
                    defaultValue = 10f,
                }
            },
            {
                "coreRadius", new CustomValueInfo
                {
                    index = 18,
                    name = "中心半径",
                    defaultValue = 0.8f,
                }
            },
            {
                "offsetRange", new CustomValueInfo
                {
                    index = 19,
                    name = "ｵﾌｾｯﾄ範囲",
                    defaultValue = 0.5f,
                }
            },
            {
                "segmentAngle", new CustomValueInfo
                {
                    index = 20,
                    name = "分割角度",
                    defaultValue = 1f,
                }
            },
            {
                "segmentRange", new CustomValueInfo
                {
                    index = 21,
                    name = "分割範囲",
                    defaultValue = 10,
                }
            },
        };

        public override Dictionary<string, CustomValueInfo> GetCustomValueInfoMap()
        {
            return CustomValueInfoMap;
        }
    }
}