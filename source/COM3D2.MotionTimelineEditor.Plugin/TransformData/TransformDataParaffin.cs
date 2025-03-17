using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TransformDataParaffin : TransformDataBase
    {
        public enum Index
        {
            ColorR = 0,
            ColorG = 1,
            ColorB = 2,
            ColorA = 3,
            SubColorR = 4,
            SubColorG = 5,
            SubColorB = 6,
            SubColorA = 7,
            Visible = 8,
            Easing = 9,
            CenterPositionX = 10,
            CenterPositionY = 11,
            RadiusFar = 12,
            RadiusNear = 13,
            RadiusScaleX = 14,
            RadiusScaleY = 15,
            UseNormal = 16,
            UseAdd = 17,
            UseMultiply = 18,
            UseOverlay = 19,
            UseSubstruct = 20,
            DepthMin = 21,
            DepthMax = 22,
            DepthFade = 23
        }

        public static TransformDataParaffin defaultTrans = new TransformDataParaffin();

        public override TransformType type => TransformType.Paraffin;

        public override int valueCount => 24;

        public override bool hasColor => true;
        public override bool hasSubColor => true;
        public override bool hasVisible => true;
        public override bool hasEasing => true;

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

        public override ValueData easingValue => values[(int)Index.Easing];

        public override Color initialColor => new Color(1f, 1f, 1f, 1f);

        public override Color initialSubColor => new Color(1f, 1f, 1f, 0f);

        public TransformDataParaffin()
        {
        }

        public override void Initialize(string name)
        {
            base.Initialize(name);
            index = PostEffectUtils.GetEffectIndex(name);
        }

        private readonly static Dictionary<string, CustomValueInfo> CustomValueInfoMap = new Dictionary<string, CustomValueInfo>
        {
            {
                "centerPositionX", new CustomValueInfo
                {
                    index = (int)Index.CenterPositionX,
                    name = "X",
                    min = -1f,
                    max = 2f,
                    step = 0.01f,
                    defaultValue = 0.5f,
                }
            },
            {
                "centerPositionY", new CustomValueInfo
                {
                    index = (int)Index.CenterPositionY,
                    name = "Y",
                    min = -1f,
                    max = 2f,
                    step = 0.01f,
                    defaultValue = 0.5f,
                }
            },
            {
                "radiusFar", new CustomValueInfo
                {
                    index = (int)Index.RadiusFar,
                    name = "外半径",
                    min = 0f,
                    max = 1f,
                    step = 0.01f,
                    defaultValue = 1f,
                }
            },
            {
                "radiusNear", new CustomValueInfo
                {
                    index = (int)Index.RadiusNear,
                    name = "内半径",
                    min = 0f,
                    max = 1f,
                    step = 0.01f,
                    defaultValue = 1f,
                }
            },
            {
                "radiusScaleX", new CustomValueInfo
                {
                    index = (int)Index.RadiusScaleX,
                    name = "SX",
                    min = 0f,
                    max = 5f,
                    step = 0.01f,
                    defaultValue = 1f,
                }
            },
            {
                "radiusScaleY", new CustomValueInfo
                {
                    index = (int)Index.RadiusScaleY,
                    name = "SY",
                    min = 0f,
                    max = 5f,
                    step = 0.01f,
                    defaultValue = 1f,
                }
            },
            {
                "useNormal", new CustomValueInfo
                {
                    index = (int)Index.UseNormal,
                    name = "通常",
                    min = 0f,
                    max = 2f,
                    step = 0.01f,
                    defaultValue = 0f,
                }
            },
            {
                "useAdd", new CustomValueInfo
                {
                    index = (int)Index.UseAdd,
                    name = "加算",
                    min = 0f,
                    max = 2f,
                    step = 0.01f,
                    defaultValue = 1f,
                }
            },
            {
                "useMultiply", new CustomValueInfo
                {
                    index = (int)Index.UseMultiply,
                    name = "乗算",
                    min = 0f,
                    max = 2f,
                    step = 0.01f,
                    defaultValue = 0f,
                }
            },
            {
                "useOverlay", new CustomValueInfo
                {
                    index = (int)Index.UseOverlay,
                    name = "オーバーレイ",
                    min = 0f,
                    max = 2f,
                    step = 0.01f,
                    defaultValue = 0f,
                }
            },
            {
                "useSubstruct", new CustomValueInfo
                {
                    index = (int)Index.UseSubstruct,
                    name = "減算",
                    min = 0f,
                    max = 2f,
                    step = 0.01f,
                    defaultValue = 0f,
                }
            },
            {
                "depthMin", new CustomValueInfo
                {
                    index = (int)Index.DepthMin,
                    name = "最小深度",
                    min = 0f,
                    max = 100f,
                    step = 0.1f,
                    defaultValue = 0f,
                }
            },
            {
                "depthMax", new CustomValueInfo
                {
                    index = (int)Index.DepthMax,
                    name = "最大深度",
                    min = 0f,
                    max = 100f,
                    step = 0.1f,
                    defaultValue = 0f,
                }
            },
            {
                "depthFade", new CustomValueInfo
                {
                    index = (int)Index.DepthFade,
                    name = "深度幅",
                    min = 0f,
                    max = 10f,
                    step = 0.01f,
                    defaultValue = 0f,
                }
            }
        };

        public override Dictionary<string, CustomValueInfo> GetCustomValueInfoMap()
        {
            return CustomValueInfoMap;
        }

        // Vector2アクセス用の配列
        public ValueData[] centerPositionValues
        {
            get => new ValueData[] { 
                values[(int)Index.CenterPositionX], 
                values[(int)Index.CenterPositionY] 
            };
        }

        // 値アクセサ
        public ValueData radiusFarValue => values[(int)Index.RadiusFar];
        public ValueData radiusNearValue => values[(int)Index.RadiusNear];
        
        // Vector2アクセス用の配列
        public ValueData[] radiusScaleValues
        {
            get => new ValueData[] { 
                values[(int)Index.RadiusScaleX], 
                values[(int)Index.RadiusScaleY] 
            };
        }
        
        public ValueData useNormalValue => values[(int)Index.UseNormal];
        public ValueData useAddValue => values[(int)Index.UseAdd];
        public ValueData useMultiplyValue => values[(int)Index.UseMultiply];
        public ValueData useOverlayValue => values[(int)Index.UseOverlay];
        public ValueData useSubstructValue => values[(int)Index.UseSubstruct];
        public ValueData depthMinValue => values[(int)Index.DepthMin];
        public ValueData depthMaxValue => values[(int)Index.DepthMax];
        public ValueData depthFadeValue => values[(int)Index.DepthFade];

        // CustomValueInfoアクセサ
        public CustomValueInfo centerPositionXInfo => CustomValueInfoMap["centerPositionX"];
        public CustomValueInfo centerPositionYInfo => CustomValueInfoMap["centerPositionY"];
        public CustomValueInfo radiusFarInfo => CustomValueInfoMap["radiusFar"];
        public CustomValueInfo radiusNearInfo => CustomValueInfoMap["radiusNear"];
        public CustomValueInfo radiusScaleXInfo => CustomValueInfoMap["radiusScaleX"];
        public CustomValueInfo radiusScaleYInfo => CustomValueInfoMap["radiusScaleY"];
        public CustomValueInfo useNormalInfo => CustomValueInfoMap["useNormal"];
        public CustomValueInfo useAddInfo => CustomValueInfoMap["useAdd"];
        public CustomValueInfo useMultiplyInfo => CustomValueInfoMap["useMultiply"];
        public CustomValueInfo useOverlayInfo => CustomValueInfoMap["useOverlay"];
        public CustomValueInfo useSubstructInfo => CustomValueInfoMap["useSubstruct"];
        public CustomValueInfo depthMinInfo => CustomValueInfoMap["depthMin"];
        public CustomValueInfo depthMaxInfo => CustomValueInfoMap["depthMax"];
        public CustomValueInfo depthFadeInfo => CustomValueInfoMap["depthFade"];

        // プロパティアクセサ
        public Vector2 centerPosition
        {
            get => centerPositionValues.ToVector2();
            set => centerPositionValues.FromVector2(value);
        }

        public float radiusFar
        {
            get => radiusFarValue.value;
            set => radiusFarValue.value = value;
        }

        public float radiusNear
        {
            get => radiusNearValue.value;
            set => radiusNearValue.value = value;
        }

        public Vector2 radiusScale
        {
            get => radiusScaleValues.ToVector2();
            set => radiusScaleValues.FromVector2(value);
        }

        public float useNormal
        {
            get => useNormalValue.value;
            set => useNormalValue.value = value;
        }

        public float useAdd
        {
            get => useAddValue.value;
            set => useAddValue.value = value;
        }   

        public float useMultiply
        {
            get => useMultiplyValue.value;
            set => useMultiplyValue.value = value;
        }

        public float useOverlay
        {
            get => useOverlayValue.value;
            set => useOverlayValue.value = value;
        }

        public float useSubstruct
        {
            get => useSubstructValue.value;
            set => useSubstructValue.value = value;
        }

        public float depthMin
        {
            get => depthMinValue.value;
            set => depthMinValue.value = value;
        }

        public float depthMax
        {
            get => depthMaxValue.value;
            set => depthMaxValue.value = value;
        }

        public float depthFade
        {
            get => depthFadeValue.value;
            set => depthFadeValue.value = value;
        }

        public ColorParaffinData paraffin
        {
            get => new ColorParaffinData
            {
                enabled = visible,
                color1 = color,
                color2 = subColor,
                centerPosition = centerPosition,
                radiusFar = radiusFar,
                radiusNear = radiusNear,
                radiusScale = radiusScale,
                depthMin = depthMin,
                depthMax = depthMax,
                depthFade = depthFade,
                useNormal = useNormal,
                useAdd = useAdd,
                useMultiply = useMultiply,
                useOverlay = useOverlay,
                useSubstruct = useSubstruct,
            };
            set
            {
                visible = value.enabled;
                color = value.color1;
                subColor = value.color2;
                centerPosition = value.centerPosition;
                radiusFar = value.radiusFar;
                radiusNear = value.radiusNear;
                radiusScale = value.radiusScale;
                depthMin = value.depthMin;
                depthMax = value.depthMax;
                depthFade = value.depthFade;
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