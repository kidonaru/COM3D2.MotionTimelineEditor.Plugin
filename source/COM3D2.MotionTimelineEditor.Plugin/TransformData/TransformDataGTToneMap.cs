using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TransformDataGTToneMap : TransformDataBase
    {
        public enum Index
        {
            Visible = 0,
            Easing = 1,
            MaxBrightness = 2,
            Contrast = 3,
            LinearStart = 4,
            LinearLength = 5,
            BlackTightness = 6,
            BlackOffset = 7
        }

        public static TransformDataGTToneMap defaultTrans = new TransformDataGTToneMap();

        public override TransformType type => TransformType.GTToneMap;

        public override int valueCount => 8;

        public override bool hasVisible => true;
        public override bool hasEasing => true;

        public override ValueData visibleValue => values[(int)Index.Visible];
        public override ValueData easingValue => values[(int)Index.Easing];

        public override bool initialVisible => false;

        public TransformDataGTToneMap()
        {
        }

        private readonly static Dictionary<string, CustomValueInfo> CustomValueInfoMap = new Dictionary<string, CustomValueInfo>
        {
            {
                "maxBrightness", new CustomValueInfo
                {
                    index = (int)Index.MaxBrightness,
                    name = "MaxBrightness",
                    min = 1f,
                    max = 100f,
                    step = 0.1f,
                    defaultValue = 1f,
                }
            },
            {
                "contrast", new CustomValueInfo
                {
                    index = (int)Index.Contrast,
                    name = "Contrast",
                    min = 0f,
                    max = 5f,
                    step = 0.01f,
                    defaultValue = 1f,
                }
            },
            {
                "linearStart", new CustomValueInfo
                {
                    index = (int)Index.LinearStart,
                    name = "LinearStart",
                    min = 0f,
                    max = 1f,
                    step = 0.01f,
                    defaultValue = 0.22f,
                }
            },
            {
                "linearLength", new CustomValueInfo
                {
                    index = (int)Index.LinearLength,
                    name = "LinearLength",
                    min = 0f,
                    max = 1f,
                    step = 0.01f,
                    defaultValue = 0.4f,
                }
            },
            {
                "blackTightness", new CustomValueInfo
                {
                    index = (int)Index.BlackTightness,
                    name = "BlackTightness",
                    min = 1f,
                    max = 3f,
                    step = 0.01f,
                    defaultValue = 1.33f,
                }
            },
            {
                "blackOffset", new CustomValueInfo
                {
                    index = (int)Index.BlackOffset,
                    name = "BlackOffset",
                    min = 0f,
                    max = 1f,
                    step = 0.01f,
                    defaultValue = 0f,
                }
            }
        };

        public override Dictionary<string, CustomValueInfo> GetCustomValueInfoMap()
        {
            return CustomValueInfoMap;
        }

        // 値アクセサ
        public ValueData maxBrightnessValue => values[(int)Index.MaxBrightness];
        public ValueData contrastValue => values[(int)Index.Contrast];
        public ValueData linearStartValue => values[(int)Index.LinearStart];
        public ValueData linearLengthValue => values[(int)Index.LinearLength];
        public ValueData blackTightnessValue => values[(int)Index.BlackTightness];
        public ValueData blackOffsetValue => values[(int)Index.BlackOffset];

        // CustomValueInfoアクセサ
        public CustomValueInfo maxBrightnessInfo => GetCustomValueInfo("maxBrightness");
        public CustomValueInfo contrastInfo => GetCustomValueInfo("contrast");
        public CustomValueInfo linearStartInfo => GetCustomValueInfo("linearStart");
        public CustomValueInfo linearLengthInfo => GetCustomValueInfo("linearLength");
        public CustomValueInfo blackTightnessInfo => GetCustomValueInfo("blackTightness");
        public CustomValueInfo blackOffsetInfo => GetCustomValueInfo("blackOffset");

        // プロパティアクセサ
        public float maxBrightness
        {
            get => maxBrightnessValue.value;
            set => maxBrightnessValue.value = value;
        }

        public float contrast
        {
            get => contrastValue.value;
            set => contrastValue.value = value;
        }

        public float linearStart
        {
            get => linearStartValue.value;
            set => linearStartValue.value = value;
        }

        public float linearLength
        {
            get => linearLengthValue.value;
            set => linearLengthValue.value = value;
        }

        public float blackTightness
        {
            get => blackTightnessValue.value;
            set => blackTightnessValue.value = value;
        }

        public float blackOffset
        {
            get => blackOffsetValue.value;
            set => blackOffsetValue.value = value;
        }

        public GTToneMapData data
        {
            get => new GTToneMapData
            {
                enabled = visible,
                maxBrightness = maxBrightness,
                contrast = contrast,
                linearStart = linearStart,
                linearLength = linearLength,
                blackTightness = blackTightness,
                blackOffset = blackOffset
            };
            set
            {
                visible = value.enabled;
                maxBrightness = value.maxBrightness;
                contrast = value.contrast;
                linearStart = value.linearStart;
                linearLength = value.linearLength;
                blackTightness = value.blackTightness;
                blackOffset = value.blackOffset;
            }
        }
    }
} 