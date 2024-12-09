using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TransformDataStageLightController : TransformDataBase
    {
        public static TransformDataStageLightController defaultTrans = new TransformDataStageLightController();

        public override TransformType type => TransformType.StageLightController;

        public override int valueCount => 37;

        public override bool hasPosition => true;

        public override bool hasSubPosition => true;

        public override bool hasEulerAngles =>  true;

        public override bool hasSubEulerAngles => true;

        public override bool hasColor =>  true;

        public override bool hasSubColor => true;

        public override bool hasVisible => true;

        public override bool hasTangent => true;

        public override ValueData[] positionValues
        {
            get => new ValueData[] { values[0], values[1], values[2] };
        }

        public override ValueData[] subPositionValues
        {
            get => new ValueData[] { values[3], values[4], values[5] };
        }

        public override ValueData[] eulerAnglesValues
        {
            get => new ValueData[] { values[6], values[7], values[8] };
        }

        public override ValueData[] subEulerAnglesValues
        {
            get => new ValueData[] { values[9], values[10], values[11] };
        }

        public override ValueData[] colorValues
        {
            get => new ValueData[] { values[12], values[13], values[14], values[15] };
        }

        public override ValueData[] subColorValues
        {
            get => new ValueData[] { values[16], values[17], values[18], values[19] };
        }

        public override ValueData visibleValue => values[20];

        private List<ValueData> _tangentValues = null;
        public override ValueData[] tangentValues
        {
            get
            {
                if (_tangentValues == null)
                {
                    _tangentValues = new List<ValueData>();
                    _tangentValues.AddRange(positionValues);
                    _tangentValues.AddRange(subPositionValues);
                    _tangentValues.AddRange(eulerAnglesValues);
                    _tangentValues.AddRange(subEulerAnglesValues);
                    _tangentValues.AddRange(colorValues);
                    _tangentValues.AddRange(subColorValues);
                    _tangentValues.AddRange(new ValueData[] { values[21], values[22] });
                }
                return _tangentValues.ToArray();
            }
        }

        public override Vector3 initialPosition => new Vector3(-5f, 10f, 0f);

        public override Vector3 initialSubPosition => new Vector3(5f, 10f, 0f);

        public override Vector3 initialEulerAngles => new Vector3(90f, 0f, 0f);

        public override Vector3 initialSubEulerAngles => new Vector3(90f, 0f, 0f);

        public override Color initialColor => new Color(1f, 1f, 1f, 0.3f);

        public override Color initialSubColor => new Color(1f, 1f, 1f, 0.3f);

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
                    min = 1f,
                    max = 179f,
                    step = 0.1f,
                    defaultValue = 10f,
                }
            },
            {
                "spotRange", new CustomValueInfo
                {
                    index = 22,
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
                    index = 23,
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
                    index = 24,
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
                    index = 25,
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
                    index = 26,
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
                    index = 27,
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
                    index = 28,
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
                    index = 29,
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
                    index = 30,
                    name = "分割範囲",
                    min = 1,
                    max = 64,
                    step = 1,
                    defaultValue = 10,
                }
            },
            {
                "autoPosition", new CustomValueInfo
                {
                    index = 31,
                    name = "一括位置",
                    min = 0,
                    max = 1,
                    step = 1,
                    defaultValue = 0f,
                }
            },
            {
                "autoRotation", new CustomValueInfo
                {
                    index = 32,
                    name = "一括回転",
                    min = 0,
                    max = 1,
                    step = 1,
                    defaultValue = 0f,
                }
            },
            {
                "autoColor", new CustomValueInfo
                {
                    index = 33,
                    name = "一括色",
                    min = 0,
                    max = 1,
                    step = 1,
                    defaultValue = 0f,
                }
            },
            {
                "autoLightInfo", new CustomValueInfo
                {
                    index = 34,
                    name = "一括情報",
                    min = 0,
                    max = 1,
                    step = 1,
                    defaultValue = 0f,
                }
            },
            {
                "autoVisible", new CustomValueInfo
                {
                    index = 35,
                    name = "一括表示",
                    min = 0,
                    max = 1,
                    step = 1,
                    defaultValue = 0f,
                }
            },
            {
                "zTest", new CustomValueInfo
                {
                    index = 36,
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
        public ValueData autoPositionValue => this["autoPosition"];
        public ValueData autoRotationValue => this["autoRotation"];
        public ValueData autoColorValue => this["autoColor"];
        public ValueData autoLightInfoValue => this["autoLightInfo"];
        public ValueData autoVisibleValue => this["autoVisible"];
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
        public CustomValueInfo autoPositionInfo => CustomValueInfoMap["autoPosition"];
        public CustomValueInfo autoRotationInfo => CustomValueInfoMap["autoRotation"];
        public CustomValueInfo autoColorInfo => CustomValueInfoMap["autoColor"];
        public CustomValueInfo autoLightInfoInfo => CustomValueInfoMap["autoLightInfo"];
        public CustomValueInfo autoVisibleInfo => CustomValueInfoMap["autoVisible"];
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

        public bool autoPosition
        {
            get => autoPositionValue.boolValue;
            set => autoPositionValue.boolValue = value;
        }

        public bool autoRotation
        {
            get => autoRotationValue.boolValue;
            set => autoRotationValue.boolValue = value;
        }

        public bool autoColor
        {
            get => autoColorValue.boolValue;
            set => autoColorValue.boolValue = value;
        }

        public bool autoLightInfo
        {
            get => autoLightInfoValue.boolValue;
            set => autoLightInfoValue.boolValue = value;
        }

        public bool autoVisible
        {
            get => autoVisibleValue.boolValue;
            set => autoVisibleValue.boolValue = value;
        }

        public bool zTest
        {
            get => zTestValue.boolValue;
            set => zTestValue.boolValue = value;
        }

        public void FromStageLightController(StageLightController controller)
        {
            var lightInfo = controller.lightInfo;

            position = controller.positionMin;
            subPosition = controller.positionMax;
            eulerAngles = controller.rotationMin;
            subEulerAngles = controller.rotationMax;
            color = controller.colorMin;
            subColor = controller.colorMax;
            visible = controller.visible;
            spotAngle = lightInfo.spotAngle;
            spotRange = lightInfo.spotRange;
            rangeMultiplier = lightInfo.rangeMultiplier;
            falloffExp = lightInfo.falloffExp;
            noiseStrength = lightInfo.noiseStrength;
            noiseScale = lightInfo.noiseScale;
            coreRadius = lightInfo.coreRadius;
            offsetRange = lightInfo.offsetRange;
            segmentAngle = lightInfo.segmentAngle;
            segmentRange = lightInfo.segmentRange;
            zTest = lightInfo.zTest;
            autoPosition = controller.autoPosition;
            autoRotation = controller.autoRotation;
            autoColor = controller.autoColor;
            autoLightInfo = controller.autoLightInfo;
            autoVisible = controller.autoVisible;
        }
    }
}