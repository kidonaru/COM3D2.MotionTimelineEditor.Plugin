using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TransformDataStageLightController : TransformDataBase
    {
        public override TransformType type
        {
            get
            {
                return TransformType.StageLightController;
            }
        }

        public override int valueCount
        {
            get
            {
                return 36;
            }
        }

        public override bool hasPosition
        {
            get
            {
                return true;
            }
        }

        public override bool hasSubPosition
        {
            get
            {
                return true;
            }
        }

        public override bool hasEulerAngles
        {
            get
            {
                return true;
            }
        }

        public override bool hasSubEulerAngles
        {
            get
            {
                return true;
            }
        }

        public override bool hasColor
        {
            get
            {
                return true;
            }
        }

        public override bool hasSubColor
        {
            get
            {
                return true;
            }
        }

        public override bool hasVisible
        {
            get
            {
                return true;
            }
        }

        public override bool hasTangent
        {
            get
            {
                return true;
            }
        }

        public override ValueData[] positionValues
        {
            get
            {
                return new ValueData[] { values[0], values[1], values[2] };
            }
        }

        public override ValueData[] subPositionValues
        {
            get
            {
                return new ValueData[] { values[3], values[4], values[5] };
            }
        }

        public override ValueData[] eulerAnglesValues
        {
            get
            {
                return new ValueData[] { values[6], values[7], values[8] };
            }
        }

        public override ValueData[] subEulerAnglesValues
        {
            get
            {
                return new ValueData[] { values[9], values[10], values[11] };
            }
        }

        public override ValueData[] colorValues
        {
            get
            {
                return new ValueData[] { values[12], values[13], values[14], values[15] };
            }
        }

        public override ValueData[] subColorValues
        {
            get
            {
                return new ValueData[] { values[16], values[17], values[18], values[19] };
            }
        }

        public override ValueData visibleValue
        {
            get
            {
                return values[20];
            }
        }

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

        public override Vector3 initialPosition
        {
            get
            {
                return new Vector3(-5f, 10f, 0f);
            }
        }

        public override Vector3 initialSubPosition
        {
            get
            {
                return new Vector3(5f, 10f, 0f);
            }
        }

        public override Vector3 initialEulerAngles
        {
            get
            {
                return new Vector3(90f, 0f, 0f);
            }
        }

        public override Vector3 initialSubEulerAngles
        {
            get
            {
                return new Vector3(90f, 0f, 0f);
            }
        }

        public override Color initialColor
        {
            get
            {
                return new Color(1f, 1f, 1f, 0.3f);
            }
        }

        public override Color initialSubColor
        {
            get
            {
                return new Color(1f, 1f, 1f, 0.3f);
            }
        }

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
                    defaultValue = 10f,
                }
            },
            {
                "spotRange", new CustomValueInfo
                {
                    index = 22,
                    name = "範囲",
                    defaultValue = 10f,
                }
            },
            {
                "rangeMultiplier", new CustomValueInfo
                {
                    index = 23,
                    name = "範囲補正",
                    defaultValue = 0.8f,
                }
            },
            {
                "falloffExp", new CustomValueInfo
                {
                    index = 24,
                    name = "減衰指数",
                    defaultValue = 0.5f,
                }
            },
            {
                "noiseStrength", new CustomValueInfo
                {
                    index = 25,
                    name = "ﾉｲｽﾞ強度",
                    defaultValue = 0.1f,
                }
            },
            {
                "noiseScale", new CustomValueInfo
                {
                    index = 26,
                    name = "ﾉｲｽﾞｻｲｽﾞ",
                    defaultValue = 10f,
                }
            },
            {
                "coreRadius", new CustomValueInfo
                {
                    index = 27,
                    name = "中心半径",
                    defaultValue = 0.8f,
                }
            },
            {
                "offsetRange", new CustomValueInfo
                {
                    index = 28,
                    name = "ｵﾌｾｯﾄ範囲",
                    defaultValue = 0.5f,
                }
            },
            {
                "segmentAngle", new CustomValueInfo
                {
                    index = 29,
                    name = "分割角度",
                    defaultValue = 1f,
                }
            },
            {
                "segmentRange", new CustomValueInfo
                {
                    index = 30,
                    name = "分割範囲",
                    defaultValue = 10,
                }
            },
            {
                "autoPosition", new CustomValueInfo
                {
                    index = 31,
                    name = "一括位置",
                    defaultValue = 0f,
                }
            },
            {
                "autoRotation", new CustomValueInfo
                {
                    index = 32,
                    name = "一括回転",
                    defaultValue = 0f,
                }
            },
            {
                "autoColor", new CustomValueInfo
                {
                    index = 33,
                    name = "一括色",
                    defaultValue = 0f,
                }
            },
            {
                "autoLightInfo", new CustomValueInfo
                {
                    index = 34,
                    name = "一括情報",
                    defaultValue = 0f,
                }
            },
            {
                "autoVisible", new CustomValueInfo
                {
                    index = 35,
                    name = "一括表示",
                    defaultValue = 0f,
                }
            },
        };

        public override Dictionary<string, CustomValueInfo> GetCustomValueInfoMap()
        {
            return CustomValueInfoMap;
        }

        public ValueData spotAngleValue
        {
            get
            {
                return this["spotAngle"];
            }
        }

        public ValueData spotRangeValue
        {
            get
            {
                return this["spotRange"];
            }
        }

        public ValueData rangeMultiplierValue
        {
            get
            {
                return this["rangeMultiplier"];
            }
        }

        public ValueData falloffExpValue
        {
            get
            {
                return this["falloffExp"];
            }
        }

        public ValueData noiseStrengthValue
        {
            get
            {
                return this["noiseStrength"];
            }
        }

        public ValueData noiseScaleValue
        {
            get
            {
                return this["noiseScale"];
            }
        }

        public ValueData coreRadiusValue
        {
            get
            {
                return this["coreRadius"];
            }
        }

        public ValueData offsetRangeValue
        {
            get
            {
                return this["offsetRange"];
            }
        }

        public ValueData segmentAngleValue
        {
            get
            {
                return this["segmentAngle"];
            }
        }

        public ValueData segmentRangeValue
        {
            get
            {
                return this["segmentRange"];
            }
        }

        public ValueData autoPositionValue
        {
            get
            {
                return this["autoPosition"];
            }
        }

        public ValueData autoRotationValue
        {
            get
            {
                return this["autoRotation"];
            }
        }

        public ValueData autoColorValue
        {
            get
            {
                return this["autoColor"];
            }
        }

        public ValueData autoLightInfoValue
        {
            get
            {
                return this["autoLightInfo"];
            }
        }

        public ValueData autoVisibleValue
        {
            get
            {
                return this["autoVisible"];
            }
        }

        public float spotAngle
        {
            get
            {
                return spotAngleValue.value;
            }
            set
            {
                spotAngleValue.value = value;
            }
        }

        public float spotRange
        {
            get
            {
                return spotRangeValue.value;
            }
            set
            {
                spotRangeValue.value = value;
            }
        }

        public float rangeMultiplier
        {
            get
            {
                return rangeMultiplierValue.value;
            }
            set
            {
                rangeMultiplierValue.value = value;
            }
        }

        public float falloffExp
        {
            get
            {
                return falloffExpValue.value;
            }
            set
            {
                falloffExpValue.value = value;
            }
        }

        public float noiseStrength
        {
            get
            {
                return noiseStrengthValue.value;
            }
            set
            {
                noiseStrengthValue.value = value;
            }
        }

        public float noiseScale
        {
            get
            {
                return noiseScaleValue.value;
            }
            set
            {
                noiseScaleValue.value = value;
            }
        }

        public float coreRadius
        {
            get
            {
                return coreRadiusValue.value;
            }
            set
            {
                coreRadiusValue.value = value;
            }
        }

        public float offsetRange
        {
            get
            {
                return offsetRangeValue.value;
            }
            set
            {
                offsetRangeValue.value = value;
            }
        }

        public float segmentAngle
        {
            get
            {
                return segmentAngleValue.value;
            }
            set
            {
                segmentAngleValue.value = value;
            }
        }

        public int segmentRange
        {
            get
            {
                return segmentRangeValue.intValue;
            }
            set
            {
                segmentRangeValue.intValue = value;
            }
        }

        public bool autoPosition
        {
            get
            {
                return autoPositionValue.boolValue;
            }
            set
            {
                autoPositionValue.boolValue = value;
            }
        }

        public bool autoRotation
        {
            get
            {
                return autoRotationValue.boolValue;
            }
            set
            {
                autoRotationValue.boolValue = value;
            }
        }

        public bool autoColor
        {
            get
            {
                return autoColorValue.boolValue;
            }
            set
            {
                autoColorValue.boolValue = value;
            }
        }

        public bool autoLightInfo
        {
            get
            {
                return autoLightInfoValue.boolValue;
            }
            set
            {
                autoLightInfoValue.boolValue = value;
            }
        }

        public bool autoVisible
        {
            get
            {
                return autoVisibleValue.boolValue;
            }
            set
            {
                autoVisibleValue.boolValue = value;
            }
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
            autoPosition = controller.autoPosition;
            autoRotation = controller.autoRotation;
            autoColor = controller.autoColor;
            autoLightInfo = controller.autoLightInfo;
            autoVisible = controller.autoVisible;
        }
    }
}