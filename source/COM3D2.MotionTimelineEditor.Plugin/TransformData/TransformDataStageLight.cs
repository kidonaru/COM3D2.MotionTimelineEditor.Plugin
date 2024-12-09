using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TransformDataStageLight : TransformDataBase
    {
        public static TransformDataStageLight defaultTrans = new TransformDataStageLight();

        public override TransformType type => TransformType.StageLight;

        public override int valueCount => 23;

        public override bool hasPosition => true;

        public override bool hasEulerAngles => true;

        public override bool hasColor =>  true;

        public override bool hasVisible => true;

        public override bool hasTangent => true;

        public override ValueData[] positionValues
        {
            get => new ValueData[] { values[0], values[1], values[2] };
        }

        public override ValueData[] eulerAnglesValues
        {
            get => new ValueData[] { values[3], values[4], values[5] };
        }

        public override ValueData[] colorValues
        {
            get => new ValueData[] { values[7], values[8], values[9], values[10] };
        }

        public override ValueData visibleValue => values[11];

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
                    _tangentValues.AddRange(colorValues);
                    _tangentValues.AddRange(new ValueData[] { values[12], values[13] });
                }
                return _tangentValues.ToArray();
            }
        }

        public override Vector3 initialPosition
        {
            get => new Vector3(0f, 10f, 0f);
        }

        public override Vector3 initialEulerAngles
        {
            get => new Vector3(90f, 0f, 0f);
        }

        public override Quaternion initialSubRotation
        {
            get => Quaternion.Euler(90f, 0f, 0f);
        }

        public override Color initialColor
        {
            get => new Color(1f, 1f, 1f, 0.3f);
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
                    min = 1f,
                    max = 179f,
                    step = 0.1f,
                    defaultValue = 10f,
                }
            },
            {
                "spotRange", new CustomValueInfo
                {
                    index = 13,
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
                    index = 14,
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
                    index = 15,
                    name = "減衰指数",
                    min = 0f,
                    max = 1f,
                    step = 0.01f,
                    defaultValue = 0.5f,
                }
            },
            {
                "noiseStrength", new CustomValueInfo
                {
                    index = 16,
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
                    index = 17,
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
                    index = 18,
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
                    index = 19,
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
                    index = 20,
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
                    index = 21,
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
                    index = 22,
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

        public ValueData spotAngleValue => this["spotAngle"];
        public ValueData spotRangeValue => this["spotRange"];
        public ValueData rangeMultiplierValue => this["rangeMultiplier"];
        public ValueData falloffExpValue => this["falloffExp"];
        public ValueData noiseStrengthValue => this["noiseStrength"];
        public ValueData noiseScaleValue => this["noiseScale"];
        public ValueData coreRadiusValue => this["coreRadius"];
        public ValueData offsetRangeValue => this["offsetRange"];
        public ValueData segmentAngleValue => this["segmentAngle"];
        public ValueData segmentRangeValue => this["segmentRange"];
        public ValueData zTestValue => this["zTest"];

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