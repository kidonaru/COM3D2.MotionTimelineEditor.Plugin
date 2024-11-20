using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TransformDataStageLightController : TransformDataBase
    {
        public override int valueCount
        {
            get
            {
                return 35;
            }
        }

        public override bool hasPosition
        {
            get
            {
                return true;
            }
        }

        public override bool hasSubPosition
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

        public override bool hasSubEulerAngles
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

        public override ValueData[] positionValues
        {
            get
            {
                return new ValueData[] { values[0], values[1], values[2] };
            }
        }

        public override ValueData[] subPositionValues
        {
            get
            {
                return new ValueData[] { values[3], values[4], values[5] };
            }
        }

        public override ValueData[] eulerAnglesValues
        {
            get
            {
                return new ValueData[] { values[6], values[7], values[8] };
            }
        }

        public override ValueData[] subEulerAnglesValues
        {
            get
            {
                return new ValueData[] { values[9], values[10], values[11] };
            }
        }

        public override ValueData[] colorValues
        {
            get
            {
                return new ValueData[] { values[12], values[13], values[14], values[15] };
            }
        }

        public override ValueData[] subColorValues
        {
            get
            {
                return new ValueData[] { values[16], values[17], values[18], values[19] };
            }
        }

        public override ValueData visibleValue
        {
            get
            {
                return values[20];
            }
        }

        public override Vector3 initialPosition
        {
            get
            {
                return new Vector3(-5f, 10f, 0f);
            }
        }

        public override Vector3 initialSubPosition
        {
            get
            {
                return new Vector3(5f, 10f, 0f);
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

        public override Color initialSubColor
        {
            get
            {
                return new Color(1f, 1f, 1f, 0.3f);
            }
        }

        public TransformDataStageLightController()
        {
        }

        private readonly static Dictionary<string, CustomValueInfo> CustomValueInfoMap = new Dictionary<string, CustomValueInfo>
        {
            {
                "spotAngle", new CustomValueInfo
                {
                    index = 21,
                    name = "角度",
                    defaultValue = 10f,
                }
            },
            {
                "spotRange", new CustomValueInfo
                {
                    index = 22,
                    name = "範囲",
                    defaultValue = 10f,
                }
            },
            {
                "rangeMultiplier", new CustomValueInfo
                {
                    index = 23,
                    name = "範囲補正",
                    defaultValue = 0.8f,
                }
            },
            {
                "falloffExp", new CustomValueInfo
                {
                    index = 24,
                    name = "減衰指数",
                    defaultValue = 0.5f,
                }
            },
            {
                "noiseStrength", new CustomValueInfo
                {
                    index = 25,
                    name = "ﾉｲｽﾞ強度",
                    defaultValue = 0.1f,
                }
            },
            {
                "noiseScale", new CustomValueInfo
                {
                    index = 26,
                    name = "ﾉｲｽﾞｻｲｽﾞ",
                    defaultValue = 10f,
                }
            },
            {
                "coreRadius", new CustomValueInfo
                {
                    index = 27,
                    name = "中心半径",
                    defaultValue = 0.8f,
                }
            },
            {
                "offsetRange", new CustomValueInfo
                {
                    index = 28,
                    name = "ｵﾌｾｯﾄ範囲",
                    defaultValue = 0.5f,
                }
            },
            {
                "segmentAngle", new CustomValueInfo
                {
                    index = 29,
                    name = "分割角度",
                    defaultValue = 1f,
                }
            },
            {
                "segmentRange", new CustomValueInfo
                {
                    index = 30,
                    name = "分割範囲",
                    defaultValue = 10,
                }
            },
            {
                "autoPosition", new CustomValueInfo
                {
                    index = 31,
                    name = "一括位置",
                    defaultValue = 0f,
                }
            },
            {
                "autoRotation", new CustomValueInfo
                {
                    index = 32,
                    name = "一括回転",
                    defaultValue = 0f,
                }
            },
            {
                "autoColor", new CustomValueInfo
                {
                    index = 33,
                    name = "一括色",
                    defaultValue = 0f,
                }
            },
            {
                "autoLightInfo", new CustomValueInfo
                {
                    index = 34,
                    name = "一括情報",
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