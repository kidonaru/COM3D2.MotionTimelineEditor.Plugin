using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TransformDataParaffin : TransformDataBase
    {
        public override TransformType type
        {
            get
            {
                return TransformType.Paraffin;
            }
        }

        public override int valueCount
        {
            get
            {
                return 21;
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

        public override bool hasEasing
        {
            get
            {
                return true;
            }
        }

        public override ValueData[] colorValues
        {
            get
            {
                return new ValueData[] { values[0], values[1], values[2], values[3] };
            }
        }

        public override ValueData[] subColorValues
        {
            get
            {
                return new ValueData[] { values[4], values[5], values[6], values[7] };
            }
        }

        public override ValueData visibleValue
        {
            get
            {
                return values[8];
            }
        }

        public override ValueData easingValue
        {
            get
            {
                return values[9];
            }
        }

        public override Color initialColor
        {
            get
            {
                return new Color(1f, 1f, 1f, 1f);
            }
        }

        public override Color initialSubColor
        {
            get
            {
                return new Color(1f, 1f, 1f, 0f);
            }
        }

        public TransformDataParaffin()
        {
        }

        private readonly static Dictionary<string, CustomValueInfo> CustomValueInfoMap = new Dictionary<string, CustomValueInfo>
        {
            {
                "centerPositionX", new CustomValueInfo
                {
                    index = 10,
                    name = "X",
                    defaultValue = 0.5f,
                }
            },
            {
                "centerPositionY", new CustomValueInfo
                {
                    index = 11,
                    name = "Y",
                    defaultValue = 0.5f,
                }
            },
            {
                "radiusFar", new CustomValueInfo
                {
                    index = 12,
                    name = "外半径",
                    defaultValue = 1f,
                }
            },
            {
                "radiusNear", new CustomValueInfo
                {
                    index = 13,
                    name = "内半径",
                    defaultValue = 1f,
                }
            },
            {
                "radiusScaleX", new CustomValueInfo
                {
                    index = 14,
                    name = "SX",
                    defaultValue = 1f,
                }
            },
            {
                "radiusScaleY", new CustomValueInfo
                {
                    index = 15,
                    name = "SY",
                    defaultValue = 1f,
                }
            },
            {
                "useNormal", new CustomValueInfo
                {
                    index = 16,
                    name = "通常",
                    defaultValue = 0f,
                }
            },
            {
                "useAdd", new CustomValueInfo
                {
                    index = 17,
                    name = "加算",
                    defaultValue = 0f,
                }
            },
            {
                "useMultiply", new CustomValueInfo
                {
                    index = 18,
                    name = "乗算",
                    defaultValue = 0f,
                }
            },
            {
                "useOverlay", new CustomValueInfo
                {
                    index = 19,
                    name = "オーバーレイ",
                    defaultValue = 0f,
                }
            },
            {
                "useSubstruct", new CustomValueInfo
                {
                    index = 20,
                    name = "減算",
                    defaultValue = 0f,
                }
            },
        };

        public override Dictionary<string, CustomValueInfo> GetCustomValueInfoMap()
        {
            return CustomValueInfoMap;
        }

        public ValueData[] centerPositionValues
        {
            get
            {
                return new ValueData[] { this["centerPositionX"], this["centerPositionY"] };
            }
        }

        public ValueData radiusFarValue
        {
            get
            {
                return this["radiusFar"];
            }
        }

        public ValueData radiusNearValue
        {
            get
            {
                return this["radiusNear"];
            }
        }

        public ValueData[] radiusScaleValues
        {
            get
            {
                return new ValueData[] { this["radiusScaleX"], this["radiusScaleY"] };
            }
        }

        public ValueData useNormalValue
        {
            get
            {
                return this["useNormal"];
            }
        }

        public ValueData useAddValue
        {
            get
            {
                return this["useAdd"];
            }
        }

        public ValueData useMultiplyValue
        {
            get
            {
                return this["useMultiply"];
            }
        }

        public ValueData useOverlayValue
        {
            get
            {
                return this["useOverlay"];
            }
        }

        public ValueData useSubstructValue
        {
            get
            {
                return this["useSubstruct"];
            }
        }

        public Vector2 centerPosition
        {
            get
            {
                return centerPositionValues.ToVector2();
            }
            set
            {
                centerPositionValues.FromVector2(value);
            }
        }

        public float radiusFar
        {
            get
            {
                return radiusFarValue.value;
            }
            set
            {
                radiusFarValue.value = value;
            }
        }

        public float radiusNear
        {
            get
            {
                return radiusNearValue.value;
            }
            set
            {
                radiusNearValue.value = value;
            }
        }

        public Vector2 radiusScale
        {
            get
            {
                return radiusScaleValues.ToVector2();
            }
            set
            {
                radiusScaleValues.FromVector2(value);
            }
        }

        public float useNormal
        {
            get
            {
                return useNormalValue.value;
            }
            set
            {
                useNormalValue.value = value;
            }
        }

        public float useAdd
        {
            get
            {
                return useAddValue.value;
            }
            set
            {
                useAddValue.value = value;
            }
        }   

        public float useMultiply
        {
            get
            {
                return useMultiplyValue.value;
            }
            set
            {
                useMultiplyValue.value = value;
            }
        }

        public float useOverlay
        {
            get
            {
                return useOverlayValue.value;
            }
            set
            {
                useOverlayValue.value = value;
            }
        }

        public float useSubstruct
        {
            get
            {
                return useSubstructValue.value;
            }
            set
            {
                useSubstructValue.value = value;
            }
        }

        public ParaffinData paraffin
        {
            get
            {
                return new ParaffinData
                {
                    enabled = visible,
                    color1 = color,
                    color2 = subColor,
                    centerPosition = centerPosition,
                    radiusFar = radiusFar,
                    radiusNear = radiusNear,
                    radiusScale = radiusScale,
                    useNormal = useNormal,
                    useAdd = useAdd,
                    useMultiply = useMultiply,
                    useOverlay = useOverlay,
                    useSubstruct = useSubstruct,
                };
            }
            set
            {
                visible = value.enabled;
                color = value.color1;
                subColor = value.color2;
                centerPosition = value.centerPosition;
                radiusFar = value.radiusFar;
                radiusNear = value.radiusNear;
                radiusScale = value.radiusScale;
                useNormal = value.useNormal;
                useAdd = value.useAdd;
                useMultiply = value.useMultiply;
                useOverlay = value.useOverlay;
                useSubstruct = value.useSubstruct;
            }
        }

        public int index;
    }
}