using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TransformDataStageLaserController : TransformDataBase
    {
        public static TransformDataStageLaserController defaultTrans = new TransformDataStageLaserController();

        public override TransformType type => TransformType.StageLaserController;

        public override int valueCount => 37;

        public override bool hasPosition => true;
        public override bool hasEulerAngles => true;
        public override bool hasColor =>  true;
        public override bool hasSubColor => true;
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
            get => new ValueData[] { values[6], values[7], values[8], values[9] };
        }

        public override ValueData[] subColorValues
        {
            get => new ValueData[] { values[10], values[11], values[12], values[13] };
        }

        public override ValueData visibleValue => values[14];

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
                    _tangentValues.AddRange(rotationMinValues);
                    _tangentValues.AddRange(rotationMaxValues);
                    _tangentValues.AddRange(new ValueData[] { values[15], values[16], values[17] });
                }
                return _tangentValues.ToArray();
            }
        }

        public override Vector3 initialPosition => StageLaserController.DefaultPosition;
        public override Vector3 initialEulerAngles => StageLaserController.DefaultEulerAngles;
        public override Color initialColor => StageLaser.DefaultColor1;
        public override Color initialSubColor => StageLaser.DefaultColor2;

        public Vector3 initialRotationMin => new Vector3(0f, 40f, 0f);
        public Vector3 initialRotationMax => new Vector3(0f, -40f, 0f);

        public TransformDataStageLaserController()
        {
        }

        private readonly static Dictionary<string, CustomValueInfo> CustomValueInfoMap = new Dictionary<string, CustomValueInfo>
        {
            {
                "intensity", new CustomValueInfo
                {
                    index = 15,
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
                    index = 16,
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
                    index = 17,
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
                    index = 18,
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
                    index = 19,
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
                    index = 20,
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
                    index = 21,
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
                    index = 22,
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
                    index = 23,
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
                    index = 24,
                    name = "分割範囲",
                    min = 1,
                    max = 64,
                    step = 1,
                    defaultValue = 10,
                }
            },
            {
                "autoPosition", new CustomValueInfo // 未使用
                {
                    index = 25,
                    name = "一括位置",
                    min = 0,
                    max = 1,
                    step = 1,
                    defaultValue = 1f,
                }
            },
            {
                "autoRotation", new CustomValueInfo
                {
                    index = 26,
                    name = "一括回転",
                    min = 0,
                    max = 1,
                    step = 1,
                    defaultValue = 1f,
                }
            },
            {
                "autoColor", new CustomValueInfo
                {
                    index = 27,
                    name = "一括色",
                    min = 0,
                    max = 1,
                    step = 1,
                    defaultValue = 1f,
                }
            },
            {
                "autoLaserInfo", new CustomValueInfo
                {
                    index = 28,
                    name = "一括情報",
                    min = 0,
                    max = 1,
                    step = 1,
                    defaultValue = 1f,
                }
            },
            {
                "autoVisible", new CustomValueInfo
                {
                    index = 29,
                    name = "一括表示",
                    min = 0,
                    max = 1,
                    step = 1,
                    defaultValue = 1f,
                }
            },
            {
                "zTest", new CustomValueInfo
                {
                    index = 30,
                    name = "Zテスト",
                    min = 0,
                    max = 1,
                    step = 1,
                    defaultValue = 1,
                }
            },
            {
                "rotationMinX", new CustomValueInfo
                {
                    index = 31,
                    name = "最小RX",
                    min = -180f,
                    max = 180f,
                    step = 0.1f,
                    defaultValue = 0f,
                }
            },
            {
                "rotationMinY", new CustomValueInfo
                {
                    index = 32,
                    name = "最小RY",
                    min = -180f,
                    max = 180f,
                    step = 0.1f,
                    defaultValue = 0f,
                }
            },
            {
                "rotationMinZ", new CustomValueInfo
                {
                    index = 33,
                    name = "最小RZ",
                    min = -180f,
                    max = 180f,
                    step = 0.1f,
                    defaultValue = 0f,
                }
            },
            {
                "rotationMaxX", new CustomValueInfo
                {
                    index = 34,
                    name = "最大RX",
                    min = -180f,
                    max = 180f,
                    step = 0.1f,
                    defaultValue = 0f,
                }
            },
            {
                "rotationMaxY", new CustomValueInfo
                {
                    index = 35,
                    name = "最大RY",
                    min = -180f,
                    max = 180f,
                    step = 0.1f,
                    defaultValue = 0f,
                }
            },
            {
                "rotationMaxZ", new CustomValueInfo
                {
                    index = 36,
                    name = "最大RZ",
                    min = -180f,
                    max = 180f,
                    step = 0.1f,
                    defaultValue = 0f,
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
        public ValueData autoRotationValue => this["autoRotation"];
        public ValueData autoColorValue => this["autoColor"];
        public ValueData autoLaserInfoValue => this["autoLaserInfo"];
        public ValueData autoVisibleValue => this["autoVisible"];
        public ValueData zTestValue => this["zTest"];
        public ValueData[] rotationMinValues => new ValueData[] { this["rotationMinX"], this["rotationMinY"], this["rotationMinZ"] };
        public ValueData[] rotationMaxValues => new ValueData[] { this["rotationMaxX"], this["rotationMaxY"], this["rotationMaxZ"] };

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
        public CustomValueInfo autoRotationInfo => CustomValueInfoMap["autoRotation"];
        public CustomValueInfo autoColorInfo => CustomValueInfoMap["autoColor"];
        public CustomValueInfo autoLaserInfoInfo => CustomValueInfoMap["autoLaserInfo"];
        public CustomValueInfo autoVisibleInfo => CustomValueInfoMap["autoVisible"];
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

        public bool autoLaserInfo
        {
            get => autoLaserInfoValue.boolValue;
            set => autoLaserInfoValue.boolValue = value;
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

        public Vector3 rotationMin
        {
            get => rotationMinValues.ToVector3();
            set => rotationMinValues.FromVector3(value);
        }

        public Vector3 rotationMax
        {
            get => rotationMaxValues.ToVector3();
            set => rotationMaxValues.FromVector3(value);
        }

        public void FromStageLaserController(StageLaserController controller)
        {
            var laserInfo = controller.laserInfo;

            position = controller.position;
            eulerAngles = controller.eulerAngles;
            color = controller.color1;
            subColor = controller.color2;
            visible = controller.visible;
            intensity = laserInfo.intensity;
            laserRange = laserInfo.laserRange;
            laserWidth = laserInfo.laserWidth;
            falloffExp = laserInfo.falloffExp;
            noiseStrength = laserInfo.noiseStrength;
            noiseScale = laserInfo.noiseScale;
            coreRadius = laserInfo.coreRadius;
            offsetRange = laserInfo.offsetRange;
            glowWidth = laserInfo.glowWidth;
            segmentRange = laserInfo.segmentRange;
            zTest = laserInfo.zTest;
            autoRotation = controller.autoRotation;
            autoColor = controller.autoColor;
            autoLaserInfo = controller.autoLaserInfo;
            autoVisible = controller.autoVisible;
            rotationMin = controller.rotationMin;
            rotationMax = controller.rotationMax;
        }
    }
}