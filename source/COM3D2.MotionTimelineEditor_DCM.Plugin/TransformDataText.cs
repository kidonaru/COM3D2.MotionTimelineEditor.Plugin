
using System.Collections.Generic;
using COM3D2.MotionTimelineEditor.Plugin;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor_DCM.Plugin
{
    public class TransformDataText : TransformDataBase
    {
        public override TransformType type
        {
            get
            {
                return TransformType.Text;
            }
        }

        public override int valueCount
        {
            get
            {
                return 20;
            }
        }

        public override int strValueCount
        {
            get
            {
                return 2;
            }
        }

        public override bool hasPosition => true;

        public override bool hasEulerAngles =>  true;

        public override bool hasScale => true;

        public override bool hasColor =>  true;

        public override bool hasEasing => true;

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

        public override ValueData[] scaleValues
        {
            get
            {
                return new ValueData[] { values[6], values[7], values[8] };
            }
        }

        public override ValueData[] colorValues
        {
            get
            {
                return new ValueData[] { values[9], values[10], values[11], values[12] };
            }
        }

        public override ValueData easingValue
        {
            get
            {
                return values[13];
            }
        }

        public TransformDataText()
        {
        }

        private readonly static Dictionary<string, CustomValueInfo> CustomValueInfoMap = new Dictionary<string, CustomValueInfo>
        {
            {
                "index",
                new CustomValueInfo
                {
                    index = 14,
                    name = "番号",
                    defaultValue = 0f,
                }
            },
            {
                "fontSize",
                new CustomValueInfo
                {
                    index = 15,
                    name = "サイズ",
                    defaultValue = 50f,
                }
            },
            {
                "lineSpacing",
                new CustomValueInfo
                {
                    index = 16,
                    name = "行間",
                    defaultValue = 50f,
                }
            },
            {
                "alignment",
                new CustomValueInfo
                {
                    index = 17,
                    name = "整列",
                    defaultValue = 4f,
                }
            },
            {
                "sizeDeltaX",
                new CustomValueInfo
                {
                    index = 18,
                    name = "幅",
                    defaultValue = 1000f,
                }
            },
            {
                "sizeDeltaY",
                new CustomValueInfo
                {
                    index = 19,
                    name = "高さ",
                    defaultValue = 1000f,
                }
            },
        };

        public override Dictionary<string, CustomValueInfo> GetCustomValueInfoMap()
        {
            return CustomValueInfoMap;
        }

        private readonly static Dictionary<string, StrValueInfo> StrValueInfoMap = new Dictionary<string, StrValueInfo>
        {
            {
                "text",
                new StrValueInfo
                {
                    index = 0,
                    name = "テキスト",
                    defaultValue = "",
                }
            },
            {
                "font",
                new StrValueInfo
                {
                    index = 1,
                    name = "フォント",
                    defaultValue = "Yu Gothic Bold",
                }
            },
        };

        public override Dictionary<string, StrValueInfo> GetStrValueInfoMap()
        {
            return StrValueInfoMap;
        }

        public ValueData indexValue
        {
            get
            {
                return this["index"];
            }
        }

        public ValueData fontSizeValue
        {
            get
            {
                return this["fontSize"];
            }
        }

        public ValueData lineSpacingValue
        {
            get
            {
                return this["lineSpacing"];
            }
        }

        public ValueData alignmentValue
        {
            get
            {
                return this["alignment"];
            }
        }

        public ValueData[] sizeDeltaValues
        {
            get
            {
                return new ValueData[] { this["sizeDeltaX"], this["sizeDeltaY"] };
            }
        }

        public int index
        {
            get
            {
                return indexValue.intValue;
            }
            set
            {
                indexValue.intValue = value;
            }
        }

        public string text
        {
            get
            {
                return GetStrValue("text");
            }
            set
            {
                SetStrValue("text", value);
            }
        }

        public string font
        {
            get
            {
                return GetStrValue("font");
            }
            set
            {
                SetStrValue("font", value);
            }
        }

        public int fontSize
        {
            get
            {
                return fontSizeValue.intValue;
            }
            set
            {
                fontSizeValue.intValue = value;
            }
        }

        public int lineSpacing
        {
            get
            {
                return lineSpacingValue.intValue;
            }
            set
            {
                lineSpacingValue.intValue = value;
            }
        }

        public TextAnchor alignment
        {
            get
            {
                return (TextAnchor) alignmentValue.intValue;
            }
            set
            {
                alignmentValue.intValue = (int) value;
            }
        }

        public Vector2 sizeDelta
        {
            get
            {
                return sizeDeltaValues.ToVector2();
            }
            set
            {
                sizeDeltaValues.FromVector2(value);
            }
        }
    }
}