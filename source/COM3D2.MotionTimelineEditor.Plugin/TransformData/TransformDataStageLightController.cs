using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TransformDataStageLightController : TransformDataBase
    {
        public enum Index
        {
            PositionX = 0,
            PositionY = 1,
            PositionZ = 2,
            SubPositionX = 3,
            SubPositionY = 4,
            SubPositionZ = 5,
            EulerX = 6,
            EulerY = 7,
            EulerZ = 8,
            SubEulerX = 9,
            SubEulerY = 10,
            SubEulerZ = 11,
            ColorR = 12,
            ColorG = 13,
            ColorB = 14,
            ColorA = 15,
            SubColorR = 16,
            SubColorG = 17,
            SubColorB = 18,
            SubColorA = 19,
            Visible = 20,
            SpotAngle = 21,
            SpotRange = 22,
            RangeMultiplier = 23,
            FalloffExp = 24,
            NoiseStrength = 25,
            NoiseScale = 26,
            CoreRadius = 27,
            OffsetRange = 28,
            SegmentAngle = 29,
            SegmentRange = 30,
            AutoPosition = 31,
            AutoRotation = 32,
            AutoColor = 33,
            AutoLightInfo = 34,
            AutoVisible = 35,
            ZTest = 36
        }

        public static TransformDataStageLightController defaultTrans = new TransformDataStageLightController();

        public override TransformType type => TransformType.StageLightController;

        public override int valueCount => 37;

        public override bool hasPosition => true;
        public override bool hasSubPosition => true;
        public override bool hasEulerAngles => true;
        public override bool hasSubEulerAngles => true;
        public override bool hasColor => true;
        public override bool hasSubColor => true;
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

        public override ValueData[] subPositionValues
        {
            get => new ValueData[] { 
                values[(int)Index.SubPositionX], 
                values[(int)Index.SubPositionY], 
                values[(int)Index.SubPositionZ] 
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

        public override ValueData[] subEulerAnglesValues
        {
            get => new ValueData[] { 
                values[(int)Index.SubEulerX], 
                values[(int)Index.SubEulerY], 
                values[(int)Index.SubEulerZ] 
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
                    _tangentValues.AddRange(positionValues);
                    _tangentValues.AddRange(subPositionValues);
                    _tangentValues.AddRange(eulerAnglesValues);
                    _tangentValues.AddRange(subEulerAnglesValues);
                    _tangentValues.AddRange(new ValueData[] { 
                        values[(int)Index.SpotAngle], 
                        values[(int)Index.SpotRange] 
                    });
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
                "autoPosition", new CustomValueInfo
                {
                    index = (int)Index.AutoPosition,
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
                    index = (int)Index.AutoRotation,
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
                    index = (int)Index.AutoColor,
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
                    index = (int)Index.AutoLightInfo,
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
                    index = (int)Index.AutoVisible,
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
        public ValueData autoPositionValue => values[(int)Index.AutoPosition];
        public ValueData autoRotationValue => values[(int)Index.AutoRotation];
        public ValueData autoColorValue => values[(int)Index.AutoColor];
        public ValueData autoLightInfoValue => values[(int)Index.AutoLightInfo];
        public ValueData autoVisibleValue => values[(int)Index.AutoVisible];
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
        public CustomValueInfo autoPositionInfo => CustomValueInfoMap["autoPosition"];
        public CustomValueInfo autoRotationInfo => CustomValueInfoMap["autoRotation"];
        public CustomValueInfo autoColorInfo => CustomValueInfoMap["autoColor"];
        public CustomValueInfo autoLightInfoInfo => CustomValueInfoMap["autoLightInfo"];
        public CustomValueInfo autoVisibleInfo => CustomValueInfoMap["autoVisible"];
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