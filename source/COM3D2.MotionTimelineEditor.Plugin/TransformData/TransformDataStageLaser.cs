using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TransformDataStageLaser : TransformDataBase
    {
        public static TransformDataStageLaser defaultTrans = new TransformDataStageLaser();

        public override TransformType type => TransformType.StageLaser;

        public override int valueCount => 24;

        public override bool hasEulerAngles => true;
        public override bool hasColor =>  true;
        public override bool hasSubColor =>  true;
        public override bool hasVisible => true;
        public override bool hasTangent => true;

        public override ValueData[] eulerAnglesValues
        {
            get => new ValueData[] { values[1], values[2], values[3] };
        }

        public override ValueData[] colorValues
        {
            get => new ValueData[] { values[4], values[5], values[6], values[7] };
        }

        public override ValueData[] subColorValues
        {
            get => new ValueData[] { values[8], values[9], values[10], values[11] };
        }

        public override ValueData visibleValue => values[12];

        private List<ValueData> _tangentValues = null;
        public override ValueData[] tangentValues
        {
            get
            {
                if (_tangentValues == null)
                {
                    _tangentValues = new List<ValueData>();
                    _tangentValues.AddRange(eulerAnglesValues);
                    _tangentValues.AddRange(new ValueData[] { values[13], values[14], values[15] });
                }
                return _tangentValues.ToArray();
            }
        }

        public override Vector3 initialPosition => StageLaser.DefaultPosition;
        public override Vector3 initialEulerAngles => StageLaser.DefaultEulerAngles;
        public override Color initialColor => StageLaser.DefaultColor1;
        public override Color initialSubColor => StageLaser.DefaultColor2;

        public TransformDataStageLaser()
        {
        }

        private readonly static Dictionary<string, CustomValueInfo> CustomValueInfoMap = new Dictionary<string, CustomValueInfo>
        {
            {
                "intensity", new CustomValueInfo
                {
                    index = 13,
                    name = "強度",
                    min = 0f,
                    max = 1f,
                    step = 0.01f,
                    defaultValue = 1f,
                }
            },
            {
                "laserRange", new CustomValueInfo
                {
                    index = 14,
                    name = "範囲",
                    min = 0f,
                    max = 100f,
                    step = 0.1f,
                    defaultValue = 13f,
                }
            },
            {
                "laserWidth", new CustomValueInfo
                {
                    index = 15,
                    name = "幅",
                    min = 0f,
                    max = 2f,
                    step = 0.01f,
                    defaultValue = 0.05f,
                }
            },
            {
                "falloffExp", new CustomValueInfo
                {
                    index = 16,
                    name = "減衰指数",
                    min = 0f,
                    max = 5f,
                    step = 0.01f,
                    defaultValue = 0.2f,
                }
            },
            {
                "noiseStrength", new CustomValueInfo
                {
                    index = 17,
                    name = "ﾉｲｽﾞ強度",
                    min = 0f,
                    max = 1f,
                    step = 0.01f,
                    defaultValue = 0.1f,
                }
            },
            {
                "noiseScale", new CustomValueInfo
                {
                    index = 18,
                    name = "ﾉｲｽﾞｻｲｽﾞ",
                    min = 1f,
                    max = 100f,
                    step = 0.1f,
                    defaultValue = 10f,
                }
            },
            {
                "coreRadius", new CustomValueInfo
                {
                    index = 19,
                    name = "中心半径",
                    min = 0f,
                    max = 1f,
                    step = 0.01f,
                    defaultValue = 0f,
                }
            },
            {
                "offsetRange", new CustomValueInfo
                {
                    index = 20,
                    name = "ｵﾌｾｯﾄ範囲",
                    min = 0f,
                    max = 10f,
                    step = 0.1f,
                    defaultValue = 0f,
                }
            },
            {
                "glowWidth", new CustomValueInfo
                {
                    index = 21,
                    name = "散乱幅",
                    min = 0f,
                    max = 5f,
                    step = 0.01f,
                    defaultValue = 0.1f,
                }
            },
            {
                "segmentRange", new CustomValueInfo
                {
                    index = 22,
                    name = "分割範囲",
                    min = 1,
                    max = 64,
                    step = 1,
                    defaultValue = 10,
                }
            },
            {
                "zTest", new CustomValueInfo
                {
                    index = 23,
                    name = "Zテスト",
                    min = 0,
                    max = 1,
                    step = 1,
                    defaultValue = 1,
                }
            },
        };

        public override Dictionary<string, CustomValueInfo> GetCustomValueInfoMap()
        {
            return CustomValueInfoMap;
        }

        public ValueData intensityValue => this["intensity"];
        public ValueData laserRangeValue => this["laserRange"];
        public ValueData laserWidthValue => this["laserWidth"];
        public ValueData falloffExpValue => this["falloffExp"];
        public ValueData noiseStrengthValue => this["noiseStrength"];
        public ValueData noiseScaleValue => this["noiseScale"];
        public ValueData coreRadiusValue => this["coreRadius"];
        public ValueData offsetRangeValue => this["offsetRange"];
        public ValueData glowWidthValue => this["glowWidth"];
        public ValueData segmentRangeValue => this["segmentRange"];
        public ValueData zTestValue => this["zTest"];

        public CustomValueInfo intensityInfo => CustomValueInfoMap["intensity"];
        public CustomValueInfo laserRangeInfo => CustomValueInfoMap["laserRange"];
        public CustomValueInfo laserWidthInfo => CustomValueInfoMap["laserWidth"];
        public CustomValueInfo falloffExpInfo => CustomValueInfoMap["falloffExp"];
        public CustomValueInfo noiseStrengthInfo => CustomValueInfoMap["noiseStrength"];
        public CustomValueInfo noiseScaleInfo => CustomValueInfoMap["noiseScale"];
        public CustomValueInfo coreRadiusInfo => CustomValueInfoMap["coreRadius"];
        public CustomValueInfo offsetRangeInfo => CustomValueInfoMap["offsetRange"];
        public CustomValueInfo glowWidthInfo => CustomValueInfoMap["glowWidth"];
        public CustomValueInfo segmentRangeInfo => CustomValueInfoMap["segmentRange"];
        public CustomValueInfo zTestInfo => CustomValueInfoMap["zTest"];

        public float intensity
        {
            get => intensityValue.value;
            set => intensityValue.value = value;
        }

        public float laserRange
        {
            get => laserRangeValue.value;
            set => laserRangeValue.value = value;
        }

        public float laserWidth
        {
            get => laserWidthValue.value;
            set => laserWidthValue.value = value;
        }

        public float falloffExp
        {
            get => falloffExpValue.value;
            set => falloffExpValue.value = value;
        }

        public float noiseStrength
        {
            get => noiseStrengthValue.value;
            set => noiseStrengthValue.value = value;
        }

        public float noiseScale
        {
            get => noiseScaleValue.value;
            set => noiseScaleValue.value = value;
        }

        public float coreRadius
        {
            get => coreRadiusValue.value;
            set => coreRadiusValue.value = value;
        }

        public float offsetRange
        {
            get => offsetRangeValue.value;
            set => offsetRangeValue.value = value;
        }

        public float glowWidth
        {
            get => glowWidthValue.value;
            set => glowWidthValue.value = value;
        }

        public int segmentRange
        {
            get => segmentRangeValue.intValue;
            set => segmentRangeValue.intValue = value;
        }

        public bool zTest
        {
            get => zTestValue.boolValue;
            set => zTestValue.boolValue = value;
        }

        public void FromStageLaser(StageLaser laser)
        {
            eulerAngles = laser.eulerAngles;
            color = laser.color1;
            subColor = laser.color2;
            visible = laser.visible;
            intensity = laser.intensity;
            laserRange = laser.laserRange;
            laserWidth = laser.laserWidth;
            falloffExp = laser.falloffExp;
            noiseStrength = laser.noiseStrength;
            noiseScale = laser.noiseScale;
            coreRadius = laser.coreRadius;
            offsetRange = laser.offsetRange;
            glowWidth = laser.glowWidth;
            segmentRange = laser.segmentRange;
            zTest = laser.zTest;
        }
    }
}