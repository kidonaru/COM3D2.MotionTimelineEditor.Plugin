using System.Collections.Generic;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TransformDataDepthOfField : TransformDataBase
    {
        public override TransformType type => TransformType.DepthOfField;

        public override int valueCount => 7;

        public override bool hasVisible => true;

        public override bool hasEasing => true;

        public override ValueData visibleValue => values[1];

        public override ValueData easingValue => values[0];

        public TransformDataDepthOfField()
        {
        }

        private readonly static Dictionary<string, CustomValueInfo> CustomValueInfoMap = new Dictionary<string, CustomValueInfo>
        {
            {
                "focalLength", new CustomValueInfo
                {
                    index = 2,
                    name = "焦点距離",
                    defaultValue = 10f,
                }
            },
            {
                "focalSize", new CustomValueInfo
                {
                    index = 3,
                    name = "焦点サイズ",
                    defaultValue = 0.05f,
                }
            },
            {
                "aperture", new CustomValueInfo
                {
                    index = 4,
                    name = "絞り値",
                    defaultValue = 11.5f,
                }
            },
            {
                "maxBlurSize", new CustomValueInfo
                {
                    index = 5,
                    name = "ブラーサイズ",
                    defaultValue = 2f,
                }
            },
            {
                "maidSlotNo", new CustomValueInfo
                {
                    index = 6,
                    name = "追従",
                    defaultValue = -1f,
                }
            },
        };

        public override Dictionary<string, CustomValueInfo> GetCustomValueInfoMap()
        {
            return CustomValueInfoMap;
        }

        public ValueData focalLengthValue => this["focalLength"];

        public ValueData focalSizeValue => this["focalSize"];

        public ValueData apertureValue => this["aperture"];

        public ValueData maxBlurSizeValue => this["maxBlurSize"];

        public ValueData maidSlotNoValue => this["maidSlotNo"];

        public float focalLength
        {
            get => focalLengthValue.value;
            set => focalLengthValue.value = value;
        }

        public float focalSize
        {
            get => focalSizeValue.value;
            set => focalSizeValue.value = value;
        }

        public float aperture
        {
            get => apertureValue.value;
            set => apertureValue.value = value;
        }

        public float maxBlurSize
        {
            get => maxBlurSizeValue.value;
            set => maxBlurSizeValue.value = value;
        }

        public int maidSlotNo
        {
            get => maidSlotNoValue.intValue;
            set => maidSlotNoValue.intValue = value;
        }

        public DepthOfFieldData depthOfField
        {
            get => new DepthOfFieldData
            {
                enabled = visible,
                focalLength = focalLength,
                focalSize = focalSize,
                aperture = aperture,
                maxBlurSize = maxBlurSize,
                maidSlotNo = maidSlotNo,
            };
            set
            {
                visible = value.enabled;
                focalLength = value.focalLength;
                focalSize = value.focalSize;
                aperture = value.aperture;
                maxBlurSize = value.maxBlurSize;
                maidSlotNo = value.maidSlotNo;
            }
        }
    }
}