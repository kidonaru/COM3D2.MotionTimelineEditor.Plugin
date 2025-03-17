using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TransformDataStageLight : TransformDataBase
    {
        public enum Index
        {
            PositionX = 0,
            PositionY = 1,
            PositionZ = 2,
            EulerX = 3,
            EulerY = 4,
            EulerZ = 5,
            ColorR = 7,
            ColorG = 8,
            ColorB = 9,
            ColorA = 10,
            Visible = 11,
            SpotAngle = 12,
            SpotRange = 13,
            RangeMultiplier = 14,
            FalloffExp = 15,
            NoiseStrength = 16,
            NoiseScale = 17,
            CoreRadius = 18,
            OffsetRange = 19,
            SegmentAngle = 20,
            SegmentRange = 21,
            ZTest = 22
        }

        public static TransformDataStageLight defaultTrans = new TransformDataStageLight();

        public override TransformType type => TransformType.StageLight;

        public override int valueCount => 23;

        public override bool hasPosition => true;
        public override bool hasEulerAngles => true;
        public override bool hasColor => true;
        public override bool hasVisible => true;
        public override bool hasTangent => true;

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

        public override ValueData[] colorValues
        {
            get => new ValueData[] { 
                values[(int)Index.ColorR], 
                values[(int)Index.ColorG], 
                values[(int)Index.ColorB], 
                values[(int)Index.ColorA] 
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
                    _tangentValues.AddRange(positionValues);
                    _tangentValues.AddRange(eulerAnglesValues);
                    _tangentValues.AddRange(new ValueData[] { 
                        values[(int)Index.SpotAngle], 
                        values[(int)Index.SpotRange] 
                    });
                }
                return _tangentValues.ToArray();
            }
        }

        public override Vector3 initialPosition => new Vector3(0f, 10f, 0f);
        public override Vector3 initialEulerAngles => new Vector3(90f, 0f, 0f);
        public override Quaternion initialSubRotation => Quaternion.Euler(90f, 0f, 0f);
        public override Color initialColor => new Color(1f, 1f, 1f, 0.3f);

        public TransformDataStageLight()
        {
        }

        private readonly static Dictionary<string, CustomValueInfo> CustomValueInfoMap = new Dictionary<string, CustomValueInfo>
        {
            {
                "spotAngle", new CustomValueInfo
                {
                    index = (int)Index.SpotAngle,
                    name = "角度",
                    min = 1f,
                    max = 179f,
                    step = 0.1f,
                    defaultValue = 10f,
                }
            },
            {
                "spotRange", new CustomValueInfo
                {
                    index = (int)Index.SpotRange,
                    name = "範囲",
                    min = 0f,
                    max = 100f,
                    step = 0.1f,
                    defaultValue = 10f,
                }
            },
            {
                "rangeMultiplier", new CustomValueInfo
                {
                    index = (int)Index.RangeMultiplier,
                    name = "範囲補正",
                    min = 0f,
                    max = 1f,
                    step = 0.01f,
                    defaultValue = 0.8f,
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
                    defaultValue = 0.5f,
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
                    defaultValue = 0.2f,
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
                    defaultValue = 0.5f,
                }
            },
            {
                "segmentAngle", new CustomValueInfo
                {
                    index = (int)Index.SegmentAngle,
                    name = "分割角度",
                    min = 1,
                    max = 64,
                    step = 1,
                    defaultValue = 10,
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
        public ValueData spotAngleValue => values[(int)Index.SpotAngle];
        public ValueData spotRangeValue => values[(int)Index.SpotRange];
        public ValueData rangeMultiplierValue => values[(int)Index.RangeMultiplier];
        public ValueData falloffExpValue => values[(int)Index.FalloffExp];
        public ValueData noiseStrengthValue => values[(int)Index.NoiseStrength];
        public ValueData noiseScaleValue => values[(int)Index.NoiseScale];
        public ValueData coreRadiusValue => values[(int)Index.CoreRadius];
        public ValueData offsetRangeValue => values[(int)Index.OffsetRange];
        public ValueData segmentAngleValue => values[(int)Index.SegmentAngle];
        public ValueData segmentRangeValue => values[(int)Index.SegmentRange];
        public ValueData zTestValue => values[(int)Index.ZTest];

        // CustomValueInfoアクセサ
        public CustomValueInfo spotAngleInfo => CustomValueInfoMap["spotAngle"];
        public CustomValueInfo spotRangeInfo => CustomValueInfoMap["spotRange"];
        public CustomValueInfo rangeMultiplierInfo => CustomValueInfoMap["rangeMultiplier"];
        public CustomValueInfo falloffExpInfo => CustomValueInfoMap["falloffExp"];
        public CustomValueInfo noiseStrengthInfo => CustomValueInfoMap["noiseStrength"];
        public CustomValueInfo noiseScaleInfo => CustomValueInfoMap["noiseScale"];
        public CustomValueInfo coreRadiusInfo => CustomValueInfoMap["coreRadius"];
        public CustomValueInfo offsetRangeInfo => CustomValueInfoMap["offsetRange"];
        public CustomValueInfo segmentAngleInfo => CustomValueInfoMap["segmentAngle"];
        public CustomValueInfo segmentRangeInfo => CustomValueInfoMap["segmentRange"];
        public CustomValueInfo zTestInfo => CustomValueInfoMap["zTest"];

        // プロパティアクセサ
        public float spotAngle
        {
            get => spotAngleValue.value;
            set => spotAngleValue.value = value;
        }

        public float spotRange
        {
            get => spotRangeValue.value;
            set => spotRangeValue.value = value;
        }

        public float rangeMultiplier
        {
            get => rangeMultiplierValue.value;
            set => rangeMultiplierValue.value = value;
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

        public int segmentAngle
        {
            get => segmentAngleValue.intValue;
            set => segmentAngleValue.intValue = value;
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

        public void FromStageLight(StageLight light)
        {
            position = light.position;
            eulerAngles = light.eulerAngles;
            color = light.color;
            visible = light.visible;
            spotAngle = light.spotAngle;
            spotRange = light.spotRange;
            rangeMultiplier = light.rangeMultiplier;
            falloffExp = light.falloffExp;
            noiseStrength = light.noiseStrength;
            noiseScale = light.noiseScale;
            coreRadius = light.coreRadius;
            offsetRange = light.offsetRange;
            segmentAngle = light.segmentAngle;
            segmentRange = light.segmentRange;
            zTest = light.zTest;
        }
    }
}