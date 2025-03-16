
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TransformDataModelMaterial : TransformDataBase
    {
        public static TransformDataModelMaterial defaultTrans = new TransformDataModelMaterial();

        public override TransformType type => TransformType.ModelMaterial;

        public override int valueCount => 17;

        public override bool hasColor => true;
        public override bool hasEasing => true;

        public override ValueData[] colorValues
        {
            get => new ValueData[] { values[1], values[2], values[3] };
        }

        public override ValueData easingValue => values[0];

        public override Color initialColor
        {
            get => Color.white;
        }

        public TransformDataModelMaterial()
        {
        }

        private readonly static Dictionary<string, CustomValueInfo> CustomValueInfoMap = new Dictionary<string, CustomValueInfo>
        {
            {
                "ShadowColor.r",
                new CustomValueInfo
                {
                    index = 4,
                    name = "影色 R",
                    defaultValue = 0f,
                }
            },
            {
                "ShadowColor.g",
                new CustomValueInfo
                {
                    index = 5,
                    name = "影色 G",
                    defaultValue = 0f,
                }
            },
            {
                "ShadowColor.b",
                new CustomValueInfo
                {
                    index = 6,
                    name = "影色 B",
                    defaultValue = 0f,
                }
            },
            {
                "RimColor.r",
                new CustomValueInfo
                {
                    index = 7,
                    name = "リムライト R",
                    defaultValue = 0f,
                }
            },
            {
                "RimColor.g",
                new CustomValueInfo
                {
                    index = 8,
                    name = "リムライト G",
                    defaultValue = 0f,
                }
            },
            {
                "RimColor.b",
                new CustomValueInfo
                {
                    index = 9,
                    name = "リムライト B",
                    defaultValue = 0f,
                }
            },
            {
                "OutlineColor.r",
                new CustomValueInfo
                {
                    index = 10,
                    name = "アウトライン R",
                    defaultValue = 0f,
                }
            },
            {
                "OutlineColor.g",
                new CustomValueInfo
                {
                    index = 11,
                    name = "アウトライン G",
                    defaultValue = 0f,
                }
            },
            {
                "OutlineColor.b",
                new CustomValueInfo
                {
                    index = 12,
                    name = "アウトライン B",
                    defaultValue = 0f,
                }
            },
            {
                "_Shininess",
                new CustomValueInfo
                {
                    index = 13,
                    name = "_Shininess",
                    min = 0f,
                    max = 10f,
                    step = 0.01f,
                }
            },
            {
                "_OutlineWidth",
                new CustomValueInfo
                {
                    index = 14,
                    name = "_OutlineWidth",
                    min = 0f,
                    max = 1f,
                    step = 0.0001f,
                }
            },
            {
                "_RimPower",
                new CustomValueInfo
                {
                    index = 15,
                    name = "_RimPower",
                    min = 0f,
                    max = 100f,
                    step = 0.01f,
                }
            },
            {
                "_RimShift",
                new CustomValueInfo
                {
                    index = 16,
                    name = "_RimShift",
                    min = 0f,
                    max = 10f,
                    step = 0.01f,
                }
            },
        };

        public override Dictionary<string, CustomValueInfo> GetCustomValueInfoMap()
        {
            return CustomValueInfoMap;
        }

        public ValueData[] ShadowColorValues => new ValueData[] {
            this["ShadowColor.r"], this["ShadowColor.g"], this["ShadowColor.b"] };

        public ValueData[] RimColorValues => new ValueData[] {
            this["RimColor.r"], this["RimColor.g"], this["RimColor.b"] };

        public ValueData[] OutlineColorValues => new ValueData[] {
            this["OutlineColor.r"], this["OutlineColor.g"], this["OutlineColor.b"] };

        public ValueData ShininessValue => this["_Shininess"];
        public ValueData OutlineWidthValue => this["_OutlineWidth"];
        public ValueData RimPowerValue => this["_RimPower"];
        public ValueData RimShiftValue => this["_RimShift"];

        public CustomValueInfo ShininessInfo => CustomValueInfoMap["_Shininess"];
        public CustomValueInfo OutlineWidthInfo => CustomValueInfoMap["_OutlineWidth"];
        public CustomValueInfo RimPowerInfo => CustomValueInfoMap["_RimPower"];
        public CustomValueInfo RimShiftInfo => CustomValueInfoMap["_RimShift"];

        public Color ShadowColor
        {
            get => ShadowColorValues.ToColor();
            set => ShadowColorValues.FromColor(value);
        }

        public Color RimColor
        {
            get => RimColorValues.ToColor();
            set => RimColorValues.FromColor(value);
        }

        public Color OutlineColor
        {
            get => OutlineColorValues.ToColor();
            set => OutlineColorValues.FromColor(value);
        }

        public float Shininess
        {
            get => ShininessValue.value;
            set => ShininessValue.value = value;
        }

        public float OutlineWidth
        {
            get => OutlineWidthValue.value;
            set => OutlineWidthValue.value = value;
        }

        public float RimPower
        {
            get => RimPowerValue.value;
            set => RimPowerValue.value = value;
        }

        public float RimShift
        {
            get => RimShiftValue.value;
            set => RimShiftValue.value = value;
        }

        public CustomValueInfo GetCustomValueInfo(ModelMaterial.ValuePropertyType propertyType)
        {
            switch (propertyType)
            {
                case ModelMaterial.ValuePropertyType._Shininess:
                    return ShininessInfo;
                case ModelMaterial.ValuePropertyType._OutlineWidth:
                    return OutlineWidthInfo;
                case ModelMaterial.ValuePropertyType._RimPower:
                    return RimPowerInfo;
                case ModelMaterial.ValuePropertyType._RimShift:
                    return RimShiftInfo;
                default:
                    return null;
            }
        }
    }
}