using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TransformDataPsylliumBar : TransformDataBase
    {
        public enum Index
        {
            Color1aR = 0,
            Color1aG = 1,
            Color1aB = 2,
            Color1aA = 3,
            Color1bR = 4,
            Color1bG = 5,
            Color1bB = 6,
            Color1bA = 7,
            Color1cR = 8,
            Color1cG = 9,
            Color1cB = 10,
            Color1cA = 11,
            Color2aR = 12,
            Color2aG = 13,
            Color2aB = 14,
            Color2aA = 15,
            Color2bR = 16,
            Color2bG = 17,
            Color2bB = 18,
            Color2bA = 19,
            Color2cR = 20,
            Color2cG = 21,
            Color2cB = 22,
            Color2cA = 23,
            BaseScale = 24,
            Width = 25,
            Height = 26,
            PositionY = 27,
            Radius = 28,
            TopThreshold = 29,
            CutoffAlpha = 30
        }

        public static TransformDataPsylliumBar defaultTrans = new TransformDataPsylliumBar();
        public static PsylliumBarConfig defaultConfig = new PsylliumBarConfig();

        public override TransformType type => TransformType.PsylliumBar;

        public override int valueCount => 31;

        public TransformDataPsylliumBar()
        {
        }

        private readonly static Dictionary<string, CustomValueInfo> CustomValueInfoMap = new Dictionary<string, CustomValueInfo>
        {
            {
                "color1aR", new CustomValueInfo
                {
                    index = (int)Index.Color1aR,
                    name = "中心色1R",
                    min = 0f,
                    max = 1f,
                    step = 0.01f,
                    defaultValue = defaultConfig.color1a.r,
                }
            },
            {
                "color1aG", new CustomValueInfo
                {
                    index = (int)Index.Color1aG,
                    name = "中心色1G",
                    min = 0f,
                    max = 1f,
                    step = 0.01f,
                    defaultValue = defaultConfig.color1a.g,
                }
            },
            {
                "color1aB", new CustomValueInfo
                {
                    index = (int)Index.Color1aB,
                    name = "中心色1B",
                    min = 0f,
                    max = 1f,
                    step = 0.01f,
                    defaultValue = defaultConfig.color1a.b,
                }
            },
            {
                "color1aA", new CustomValueInfo
                {
                    index = (int)Index.Color1aA,
                    name = "中心色1A",
                    min = 0f,
                    max = 1f,
                    step = 0.01f,
                    defaultValue = defaultConfig.color1a.a,
                }
            },
            {
                "color1bR", new CustomValueInfo
                {
                    index = (int)Index.Color1bR,
                    name = "縁色1R",
                    min = 0f,
                    max = 1f,
                    step = 0.01f,
                    defaultValue = defaultConfig.color1b.r,
                }
            },
            {
                "color1bG", new CustomValueInfo
                {
                    index = (int)Index.Color1bG,
                    name = "縁色1G",
                    min = 0f,
                    max = 1f,
                    step = 0.01f,
                    defaultValue = defaultConfig.color1b.g,
                }
            },
            {
                "color1bB", new CustomValueInfo
                {
                    index = (int)Index.Color1bB,
                    name = "縁色1B",
                    min = 0f,
                    max = 1f,
                    step = 0.01f,
                    defaultValue = defaultConfig.color1b.b,
                }
            },
            {
                "color1bA", new CustomValueInfo
                {
                    index = (int)Index.Color1bA,
                    name = "縁色1A",
                    min = 0f,
                    max = 1f,
                    step = 0.01f,
                    defaultValue = defaultConfig.color1b.a,
                }
            },
            {
                "color1cR", new CustomValueInfo
                {
                    index = (int)Index.Color1cR,
                    name = "散乱色1R",
                    min = 0f,
                    max = 1f,
                    step = 0.01f,
                    defaultValue = defaultConfig.color1c.r,
                }
            },
            {
                "color1cG", new CustomValueInfo
                {
                    index = (int)Index.Color1cG,
                    name = "散乱色1G",
                    min = 0f,
                    max = 1f,
                    step = 0.01f,
                    defaultValue = defaultConfig.color1c.g,
                }
            },
            {
                "color1cB", new CustomValueInfo
                {
                    index = (int)Index.Color1cB,
                    name = "散乱色1B",
                    min = 0f,
                    max = 1f,
                    step = 0.01f,
                    defaultValue = defaultConfig.color1c.b,
                }
            },
            {
                "color1cA", new CustomValueInfo
                {
                    index = (int)Index.Color1cA,
                    name = "散乱色1A",
                    min = 0f,
                    max = 1f,
                    step = 0.01f,
                    defaultValue = defaultConfig.color1c.a,
                }
            },
            {
                "color2aR", new CustomValueInfo
                {
                    index = (int)Index.Color2aR,
                    name = "中心色2R",
                    min = 0f,
                    max = 1f,
                    step = 0.01f,
                    defaultValue = defaultConfig.color2a.r,
                }
            },
            {
                "color2aG", new CustomValueInfo
                {
                    index = (int)Index.Color2aG,
                    name = "中心色2G",
                    min = 0f,
                    max = 1f,
                    step = 0.01f,
                    defaultValue = defaultConfig.color2a.g,
                }
            },
            {
                "color2aB", new CustomValueInfo
                {
                    index = (int)Index.Color2aB,
                    name = "中心色2B",
                    min = 0f,
                    max = 1f,
                    step = 0.01f,
                    defaultValue = defaultConfig.color2a.b,
                }
            },
            {
                "color2aA", new CustomValueInfo
                {
                    index = (int)Index.Color2aA,
                    name = "中心色2A",
                    min = 0f,
                    max = 1f,
                    step = 0.01f,
                    defaultValue = defaultConfig.color2a.a,
                }
            },
            {
                "color2bR", new CustomValueInfo
                {
                    index = (int)Index.Color2bR,
                    name = "縁色2R",
                    min = 0f,
                    max = 1f,
                    step = 0.01f,
                    defaultValue = defaultConfig.color2b.r,
                }
            },
            {
                "color2bG", new CustomValueInfo
                {
                    index = (int)Index.Color2bG,
                    name = "縁色2G",
                    min = 0f,
                    max = 1f,
                    step = 0.01f,
                    defaultValue = defaultConfig.color2b.g,
                }
            },
            {
                "color2bB", new CustomValueInfo
                {
                    index = (int)Index.Color2bB,
                    name = "縁色2B",
                    min = 0f,
                    max = 1f,
                    step = 0.01f,
                    defaultValue = defaultConfig.color2b.b,
                }
            },
            {
                "color2bA", new CustomValueInfo
                {
                    index = (int)Index.Color2bA,
                    name = "縁色2A",
                    min = 0f,
                    max = 1f,
                    step = 0.01f,
                    defaultValue = defaultConfig.color2b.a,
                }
            },
            {
                "color2cR", new CustomValueInfo
                {
                    index = (int)Index.Color2cR,
                    name = "散乱色2R",
                    min = 0f,
                    max = 1f,
                    step = 0.01f,
                    defaultValue = defaultConfig.color2c.r,
                }
            },
            {
                "color2cG", new CustomValueInfo
                {
                    index = (int)Index.Color2cG,
                    name = "散乱色2G",
                    min = 0f,
                    max = 1f,
                    step = 0.01f,
                    defaultValue = defaultConfig.color2c.g,
                }
            },
            {
                "color2cB", new CustomValueInfo
                {
                    index = (int)Index.Color2cB,
                    name = "散乱色2B",
                    min = 0f,
                    max = 1f,
                    step = 0.01f,
                    defaultValue = defaultConfig.color2c.b,
                }
            },
            {
                "color2cA", new CustomValueInfo
                {
                    index = (int)Index.Color2cA,
                    name = "散乱色2A",
                    min = 0f,
                    max = 1f,
                    step = 0.01f,
                    defaultValue = defaultConfig.color2c.a,
                }
            },
            {
                "baseScale", new CustomValueInfo
                {
                    index = (int)Index.BaseScale,
                    name = "スケール",
                    min = 0f,
                    max = 5f,
                    step = 0.01f,
                    defaultValue = defaultConfig.baseScale,
                }
            },
            {
                "width", new CustomValueInfo
                {
                    index = (int)Index.Width,
                    name = "幅",
                    min = 0f,
                    max = 5f,
                    step = 0.01f,
                    defaultValue = defaultConfig.width,
                }
            },
            {
                "height", new CustomValueInfo
                {
                    index = (int)Index.Height,
                    name = "高さ",
                    min = 0f,
                    max = 5f,
                    step = 0.01f,
                    defaultValue = defaultConfig.height,
                }
            },
            {
                "positionY", new CustomValueInfo
                {
                    index = (int)Index.PositionY,
                    name = "Y",
                    min = 0f,
                    max = 5f,
                    step = 0.01f,
                    defaultValue = defaultConfig.positionY,
                }
            },
            {
                "radius", new CustomValueInfo
                {
                    index = (int)Index.Radius,
                    name = "半径",
                    min = 0f,
                    max = 1f,
                    step = 0.01f,
                    defaultValue = defaultConfig.radius,
                }
            },
            {
                "topThreshold", new CustomValueInfo
                {
                    index = (int)Index.TopThreshold,
                    name = "上部閾値",
                    min = 0f,
                    max = 1f,
                    step = 0.01f,
                    defaultValue = defaultConfig.topThreshold,
                }
            },
            {
                "cutoffAlpha", new CustomValueInfo
                {
                    index = (int)Index.CutoffAlpha,
                    name = "A閾値",
                    min = 0f,
                    max = 1f,
                    step = 0.01f,
                    defaultValue = defaultConfig.cutoffAlpha,
                }
            },
        };

        public override Dictionary<string, CustomValueInfo> GetCustomValueInfoMap()
        {
            return CustomValueInfoMap;
        }

        public ValueData[] color1aValues
        {
            get => new ValueData[] { 
                values[(int)Index.Color1aR], 
                values[(int)Index.Color1aG], 
                values[(int)Index.Color1aB], 
                values[(int)Index.Color1aA] 
            };
        }
        public ValueData[] color1bValues
        {
            get => new ValueData[] { 
                values[(int)Index.Color1bR], 
                values[(int)Index.Color1bG], 
                values[(int)Index.Color1bB], 
                values[(int)Index.Color1bA] 
            };
        }
        public ValueData[] color1cValues
        {
            get => new ValueData[] { 
                values[(int)Index.Color1cR], 
                values[(int)Index.Color1cG], 
                values[(int)Index.Color1cB], 
                values[(int)Index.Color1cA] 
            };
        }
        public ValueData[] color2aValues
        {
            get => new ValueData[] { 
                values[(int)Index.Color2aR], 
                values[(int)Index.Color2aG], 
                values[(int)Index.Color2aB], 
                values[(int)Index.Color2aA] 
            };
        }
        public ValueData[] color2bValues
        {
            get => new ValueData[] { 
                values[(int)Index.Color2bR], 
                values[(int)Index.Color2bG], 
                values[(int)Index.Color2bB], 
                values[(int)Index.Color2bA] 
            };
        }
        public ValueData[] color2cValues
        {
            get => new ValueData[] { 
                values[(int)Index.Color2cR], 
                values[(int)Index.Color2cG], 
                values[(int)Index.Color2cB], 
                values[(int)Index.Color2cA] 
            };
        }
        public ValueData baseScaleValue => values[(int)Index.BaseScale];
        public ValueData widthValue => values[(int)Index.Width];
        public ValueData heightValue => values[(int)Index.Height];
        public ValueData positionYValue => values[(int)Index.PositionY];
        public ValueData radiusValue => values[(int)Index.Radius];
        public ValueData topThresholdValue => values[(int)Index.TopThreshold];
        public ValueData cutoffAlphaValue => values[(int)Index.CutoffAlpha];

        public CustomValueInfo baseScaleInfo => CustomValueInfoMap["baseScale"];
        public CustomValueInfo widthInfo => CustomValueInfoMap["width"];
        public CustomValueInfo heightInfo => CustomValueInfoMap["height"];
        public CustomValueInfo positionYInfo => CustomValueInfoMap["positionY"];
        public CustomValueInfo radiusInfo => CustomValueInfoMap["radius"];
        public CustomValueInfo topThresholdInfo => CustomValueInfoMap["topThreshold"];
        public CustomValueInfo cutoffAlphaInfo => CustomValueInfoMap["cutoffAlpha"];

        public Color color1a
        {
            get => color1aValues.ToColor();
            set => color1aValues.FromColor(value);
        }
        public Color color1b
        {
            get => color1bValues.ToColor();
            set => color1bValues.FromColor(value);
        }
        public Color color1c
        {
            get => color1cValues.ToColor();
            set => color1cValues.FromColor(value);
        }
        public Color color2a
        {
            get => color2aValues.ToColor();
            set => color2aValues.FromColor(value);
        }
        public Color color2b
        {
            get => color2bValues.ToColor();
            set => color2bValues.FromColor(value);
        }
        public Color color2c
        {
            get => color2cValues.ToColor();
            set => color2cValues.FromColor(value);
        }
        public float baseScale
        {
            get => baseScaleValue.value;
            set => baseScaleValue.value = value;
        }
        public float width
        {
            get => widthValue.value;
            set => widthValue.value = value;
        }
        public float height
        {
            get => heightValue.value;
            set => heightValue.value = value;
        }
        public float positionY
        {
            get => positionYValue.value;
            set => positionYValue.value = value;
        }
        public float radius
        {
            get => radiusValue.value;
            set => radiusValue.value = value;
        }
        public float topThreshold
        {
            get => topThresholdValue.value;
            set => topThresholdValue.value = value;
        }
        public float cutoffAlpha
        {
            get => cutoffAlphaValue.value;
            set => cutoffAlphaValue.value = value;
        }

        public void FromConfig(PsylliumBarConfig config)
        {
            color1a = config.color1a;
            color1b = config.color1b;
            color1c = config.color1c;
            color2a = config.color2a;
            color2b = config.color2b;
            color2c = config.color2c;
            baseScale = config.baseScale;
            width = config.width;
            height = config.height;
            positionY = config.positionY;
            radius = config.radius;
            topThreshold = config.topThreshold;
            cutoffAlpha = config.cutoffAlpha;
        }

        private PsylliumBarConfig _config = new PsylliumBarConfig();

        public PsylliumBarConfig ToConfig()
        {
            _config.color1a = color1a;
            _config.color1b = color1b;
            _config.color1c = color1c;
            _config.color2a = color2a;
            _config.color2b = color2b;
            _config.color2c = color2c;
            _config.baseScale = baseScale;
            _config.width = width;
            _config.height = height;
            _config.positionY = positionY;
            _config.radius = radius;
            _config.topThreshold = topThreshold;
            _config.cutoffAlpha = cutoffAlpha;
            return _config;
        }
    }
}