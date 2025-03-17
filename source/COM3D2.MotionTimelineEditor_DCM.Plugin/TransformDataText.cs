using System.Collections.Generic;
using COM3D2.MotionTimelineEditor.Plugin;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor_DCM.Plugin
{
    public class TransformDataText : TransformDataBase
    {
        public enum Index
        {
            PositionX = 0,
            PositionY = 1,
            PositionZ = 2,
            EulerX = 3,
            EulerY = 4,
            EulerZ = 5,
            ScaleX = 6,
            ScaleY = 7,
            ScaleZ = 8,
            ColorR = 9,
            ColorG = 10,
            ColorB = 11,
            ColorA = 12,
            Easing = 13,
            TextIndex = 14,
            FontSize = 15,
            LineSpacing = 16,
            Alignment = 17,
            SizeDeltaX = 18,
            SizeDeltaY = 19
        }

        public enum StrIndex
        {
            Text = 0,
            Font = 1
        }

        public override TransformType type => TransformType.Text;

        public override int valueCount => 20;
        public override int strValueCount => 2;

        public override bool hasPosition => true;
        public override bool hasEulerAngles => true;
        public override bool hasScale => true;
        public override bool hasColor => true;
        public override bool hasEasing => true;

        public override ValueData[] positionValues
        {
            get => new ValueData[] { 
                values[(int)Index.PositionX], 
                values[(int)Index.PositionY], 
                values[(int)Index.PositionZ] 
            };
        }

        public override ValueData[] eulerAnglesValues
        {
            get => new ValueData[] { 
                values[(int)Index.EulerX], 
                values[(int)Index.EulerY], 
                values[(int)Index.EulerZ] 
            };
        }

        public override ValueData[] scaleValues
        {
            get => new ValueData[] { 
                values[(int)Index.ScaleX], 
                values[(int)Index.ScaleY], 
                values[(int)Index.ScaleZ] 
            };
        }

        public override ValueData[] colorValues
        {
            get => new ValueData[] { 
                values[(int)Index.ColorR], 
                values[(int)Index.ColorG], 
                values[(int)Index.ColorB], 
                values[(int)Index.ColorA] 
            };
        }

        public override ValueData easingValue => values[(int)Index.Easing];

        public TransformDataText()
        {
        }

        private readonly static Dictionary<string, CustomValueInfo> CustomValueInfoMap = new Dictionary<string, CustomValueInfo>
        {
            {
                "index",
                new CustomValueInfo
                {
                    index = (int)Index.TextIndex,
                    name = "番号",
                    defaultValue = 0f,
                }
            },
            {
                "fontSize",
                new CustomValueInfo
                {
                    index = (int)Index.FontSize,
                    name = "サイズ",
                    defaultValue = 50f,
                }
            },
            {
                "lineSpacing",
                new CustomValueInfo
                {
                    index = (int)Index.LineSpacing,
                    name = "行間",
                    defaultValue = 50f,
                }
            },
            {
                "alignment",
                new CustomValueInfo
                {
                    index = (int)Index.Alignment,
                    name = "整列",
                    defaultValue = 4f,
                }
            },
            {
                "sizeDeltaX",
                new CustomValueInfo
                {
                    index = (int)Index.SizeDeltaX,
                    name = "幅",
                    defaultValue = 1000f,
                }
            },
            {
                "sizeDeltaY",
                new CustomValueInfo
                {
                    index = (int)Index.SizeDeltaY,
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
                    index = (int)StrIndex.Text,
                    name = "テキスト",
                    defaultValue = "",
                }
            },
            {
                "font",
                new StrValueInfo
                {
                    index = (int)StrIndex.Font,
                    name = "フォント",
                    defaultValue = "Yu Gothic Bold",
                }
            },
        };

        public override Dictionary<string, StrValueInfo> GetStrValueInfoMap()
        {
            return StrValueInfoMap;
        }

        public ValueData indexValue => values[(int)Index.TextIndex];

        public ValueData fontSizeValue => values[(int)Index.FontSize];

        public ValueData lineSpacingValue => values[(int)Index.LineSpacing];

        public ValueData alignmentValue => values[(int)Index.Alignment];

        public ValueData[] sizeDeltaValues
        {
            get => new ValueData[] { 
                values[(int)Index.SizeDeltaX], 
                values[(int)Index.SizeDeltaY] 
            };
        }

        public int index
        {
            get => indexValue.intValue;
            set => indexValue.intValue = value;
        }

        public string text
        {
            get => strValues[(int)StrIndex.Text];
            set => strValues[(int)StrIndex.Text] = value;
        }

        public string font
        {
            get => strValues[(int)StrIndex.Font];
            set => strValues[(int)StrIndex.Font] = value;
        }

        public int fontSize
        {
            get => fontSizeValue.intValue;
            set => fontSizeValue.intValue = value;
        }

        public int lineSpacing
        {
            get => lineSpacingValue.intValue;
            set => lineSpacingValue.intValue = value;
        }

        public TextAnchor alignment
        {
            get => (TextAnchor) alignmentValue.intValue;
            set => alignmentValue.intValue = (int) value;
        }

        public Vector2 sizeDelta
        {
            get => sizeDeltaValues.ToVector2();
            set => sizeDeltaValues.FromVector2(value);
        }
    }
}