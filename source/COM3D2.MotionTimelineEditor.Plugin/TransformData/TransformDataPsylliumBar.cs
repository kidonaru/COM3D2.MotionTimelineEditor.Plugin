using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TransformDataPsylliumBar : TransformDataBase
    {
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
                    index = 0,
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
                    index = 1,
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
                    index = 2,
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
                    index = 3,
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
                    index = 4,
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
                    index = 5,
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
                    index = 6,
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
                    index = 7,
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
                    index = 8,
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
                    index = 9,
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
                    index = 10,
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
                    index = 11,
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
                    index = 12,
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
                    index = 13,
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
                    index = 14,
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
                    index = 15,
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
                    index = 16,
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
                    index = 17,
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
                    index = 18,
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
                    index = 19,
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
                    index = 20,
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
                    index = 21,
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
                    index = 22,
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
                    index = 23,
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
                    index = 24,
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
                    index = 25,
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
                    index = 26,
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
                    index = 27,
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
                    index = 28,
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
                    index = 29,
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
                    index = 30,
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
            get => new ValueData[] { this["color1aR"], this["color1aG"], this["color1aB"], this["color1aA"] };
        }
        public ValueData[] color1bValues
        {
            get => new ValueData[] { this["color1bR"], this["color1bG"], this["color1bB"], this["color1bA"] };
        }
        public ValueData[] color1cValues
        {
            get => new ValueData[] { this["color1cR"], this["color1cG"], this["color1cB"], this["color1cA"] };
        }
        public ValueData[] color2aValues
        {
            get => new ValueData[] { this["color2aR"], this["color2aG"], this["color2aB"], this["color2aA"] };
        }
        public ValueData[] color2bValues
        {
            get => new ValueData[] { this["color2bR"], this["color2bG"], this["color2bB"], this["color2bA"] };
        }
        public ValueData[] color2cValues
        {
            get => new ValueData[] { this["color2cR"], this["color2cG"], this["color2cB"], this["color2cA"] };
        }
        public ValueData baseScaleValue => this["baseScale"];
        public ValueData widthValue => this["width"];
        public ValueData heightValue => this["height"];
        public ValueData positionYValue => this["positionY"];
        public ValueData radiusValue => this["radius"];
        public ValueData topThresholdValue => this["topThreshold"];
        public ValueData cutoffAlphaValue => this["cutoffAlpha"];

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