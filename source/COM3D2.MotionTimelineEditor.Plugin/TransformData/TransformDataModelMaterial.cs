
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TransformDataModelMaterial : TransformDataBase
    {
        public override TransformType type => TransformType.ModelMaterial;

        public override int valueCount => 13;

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

        public override void FromXml(TransformXml xml)
        {
            base.FromXml(xml);

            if (name.EndsWith(".menu", System.StringComparison.Ordinal))
            {
                name = Path.GetFileName(name);
            }
        }
    }
}