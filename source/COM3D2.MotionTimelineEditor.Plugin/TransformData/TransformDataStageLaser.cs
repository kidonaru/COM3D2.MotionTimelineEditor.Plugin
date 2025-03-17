using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TransformDataStageLaser : TransformDataBase
    {
        public enum Index
        {
            PositionX = 0,
            EulerX = 1,
            EulerY = 2,
            EulerZ = 3,
            ColorR = 4,
            ColorG = 5,
            ColorB = 6,
            ColorA = 7,
            SubColorR = 8,
            SubColorG = 9,
            SubColorB = 10,
            SubColorA = 11,
            Visible = 12,
            Intensity = 13,
            LaserRange = 14,
            LaserWidth = 15,
            FalloffExp = 16,
            NoiseStrength = 17,
            NoiseScale = 18,
            CoreRadius = 19,
            OffsetRange = 20,
            GlowWidth = 21,
            SegmentRange = 22,
            ZTest = 23
        }

        public static TransformDataStageLaser defaultTrans = new TransformDataStageLaser();

        public override TransformType type => TransformType.StageLaser;

        public override int valueCount => 24;

        public override bool hasEulerAngles => true;
        public override bool hasColor => true;
        public override bool hasSubColor =>  true;
        public override bool hasVisible => true;
        public override bool hasTangent => true;

        public override ValueData[] eulerAnglesValues
        {
            get => new ValueData[] { 
                values[(int)Index.EulerX], 
                values[(int)Index.EulerY], 
                values[(int)Index.EulerZ] 
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

        public override ValueData[] subColorValues
        {
            get => new ValueData[] { 
                values[(int)Index.SubColorR], 
                values[(int)Index.SubColorG], 
                values[(int)Index.SubColorB], 
                values[(int)Index.SubColorA] 
            };
        }

        public override ValueData visibleValue => values[(int)Index.Visible];

        private List<ValueData> _tangentValues = null;
        public override ValueData[] tangentValues
        {
            get
            {
                if (_tangentValues == null)
                {
                    _tangentValues = new List<ValueData>();
                    _tangentValues.AddRange(eulerAnglesValues);
                    _tangentValues.AddRange(new ValueData[] { 
                        values[(int)Index.Intensity], 
                        values[(int)Index.LaserRange], 
                        values[(int)Index.LaserWidth] 
                    });
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
                    index = (int)Index.Intensity,
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
                    index = (int)Index.LaserRange,
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
                    index = (int)Index.LaserWidth,
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
                    index = (int)Index.FalloffExp,
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
                    index = (int)Index.NoiseStrength,
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
                    index = (int)Index.NoiseScale,
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
                    index = (int)Index.CoreRadius,
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
                    index = (int)Index.OffsetRange,
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
                    index = (int)Index.GlowWidth,
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
                    index = (int)Index.SegmentRange,
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
                    index = (int)Index.ZTest,
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

        // 値アクセサ
        public ValueData intensityValue => values[(int)Index.Intensity];
        public ValueData laserRangeValue => values[(int)Index.LaserRange];
        public ValueData laserWidthValue => values[(int)Index.LaserWidth];
        public ValueData falloffExpValue => values[(int)Index.FalloffExp];
        public ValueData noiseStrengthValue => values[(int)Index.NoiseStrength];
        public ValueData noiseScaleValue => values[(int)Index.NoiseScale];
        public ValueData coreRadiusValue => values[(int)Index.CoreRadius];
        public ValueData offsetRangeValue => values[(int)Index.OffsetRange];
        public ValueData glowWidthValue => values[(int)Index.GlowWidth];
        public ValueData segmentRangeValue => values[(int)Index.SegmentRange];
        public ValueData zTestValue => values[(int)Index.ZTest];

        // CustomValueInfoアクセサ
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

        // プロパティアクセサ
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