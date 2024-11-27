using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TransformDataStageLight : TransformDataBase
    {
        public override int valueCount
        {
            get
            {
                return 22;
            }
        }

        public override bool hasPosition
        {
            get
            {
                return true;
            }
        }

        public override bool hasRotation
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

        public override ValueData[] rotationValues
        {
            get
            {
                return new ValueData[] { values[3], values[4], values[5], values[6] };
            }
        }

        public override ValueData[] colorValues
        {
            get
            {
                return new ValueData[] { values[7], values[8], values[9], values[10] };
            }
        }

        public override ValueData visibleValue
        {
            get
            {
                return values[11];
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
                    _tangentValues.AddRange(rotationValues);
                    _tangentValues.AddRange(colorValues);
                    _tangentValues.AddRange(new ValueData[] { values[12], values[13] });
                }
                return _tangentValues.ToArray();
            }
        }

        public override Vector3 initialPosition
        {
            get
            {
                return new Vector3(0f, 10f, 0f);
            }
        }

        public override Quaternion initialRotation
        {
            get
            {
                return Quaternion.Euler(90f, 0f, 0f);
            }
        }

        public override Quaternion initialSubRotation
        {
            get
            {
                return Quaternion.Euler(90f, 0f, 0f);
            }
        }

        public override Color initialColor
        {
            get
            {
                return new Color(1f, 1f, 1f, 0.3f);
            }
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
                    defaultValue = 10f,
                }
            },
            {
                "spotRange", new CustomValueInfo
                {
                    index = 13,
                    name = "範囲",
                    defaultValue = 10f,
                }
            },
            {
                "rangeMultiplier", new CustomValueInfo
                {
                    index = 14,
                    name = "範囲補正",
                    defaultValue = 0.8f,
                }
            },
            {
                "falloffExp", new CustomValueInfo
                {
                    index = 15,
                    name = "減衰指数",
                    defaultValue = 0.5f,
                }
            },
            {
                "noiseStrength", new CustomValueInfo
                {
                    index = 16,
                    name = "ﾉｲｽﾞ強度",
                    defaultValue = 0.1f,
                }
            },
            {
                "noiseScale", new CustomValueInfo
                {
                    index = 17,
                    name = "ﾉｲｽﾞｻｲｽﾞ",
                    defaultValue = 10f,
                }
            },
            {
                "coreRadius", new CustomValueInfo
                {
                    index = 18,
                    name = "中心半径",
                    defaultValue = 0.8f,
                }
            },
            {
                "offsetRange", new CustomValueInfo
                {
                    index = 19,
                    name = "ｵﾌｾｯﾄ範囲",
                    defaultValue = 0.5f,
                }
            },
            {
                "segmentAngle", new CustomValueInfo
                {
                    index = 20,
                    name = "分割角度",
                    defaultValue = 1f,
                }
            },
            {
                "segmentRange", new CustomValueInfo
                {
                    index = 21,
                    name = "分割範囲",
                    defaultValue = 10,
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

        public void FromStageLight(StageLight light)
        {
            position = light.transform.localPosition;
            rotation = light.transform.localRotation;
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
        }
    }
}