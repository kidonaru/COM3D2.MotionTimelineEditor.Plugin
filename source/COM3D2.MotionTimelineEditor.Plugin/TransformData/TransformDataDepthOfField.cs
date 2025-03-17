using System.Collections.Generic;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TransformDataDepthOfField : TransformDataBase
    {
        public enum Index
        {
            Easing = 0,
            Visible = 1,
            FocalLength = 2,
            FocalSize = 3,
            Aperture = 4,
            MaxBlurSize = 5,
            MaidSlotNo = 6
        }

        public static TransformDataDepthOfField defaultTrans = new TransformDataDepthOfField();

        public override TransformType type => TransformType.DepthOfField;

        public override int valueCount => 7;

        public override bool hasVisible => true;
        public override bool hasEasing => true;

        public override ValueData visibleValue => values[(int)Index.Visible];

        public override ValueData easingValue => values[(int)Index.Easing];

        public TransformDataDepthOfField()
        {
        }

        private readonly static Dictionary<string, CustomValueInfo> CustomValueInfoMap = new Dictionary<string, CustomValueInfo>
        {
            {
                "focalLength", new CustomValueInfo
                {
                    index = (int)Index.FocalLength,
                    name = "ﾋﾟﾝﾄ距離",
                    min = 0f,
                    max = config.positionRange,
                    step = 0.1f,
                    defaultValue = 10f,
                }
            },
            {
                "focalSize", new CustomValueInfo
                {
                    index = (int)Index.FocalSize,
                    name = "焦点距離",
                    min = 0f,
                    max = 2f,
                    step = 0.01f,
                    defaultValue = 0.05f,
                }
            },
            {
                "aperture", new CustomValueInfo
                {
                    index = (int)Index.Aperture,
                    name = "絞り値",
                    min = 0f,
                    max = 60f,
                    step = 0.1f,
                    defaultValue = 11.5f,
                }
            },
            {
                "maxBlurSize", new CustomValueInfo
                {
                    index = (int)Index.MaxBlurSize,
                    name = "ﾌﾞﾗｰｻｲｽﾞ",
                    min = 0f,
                    max = 10f,
                    step = 0.1f,
                    defaultValue = 2f,
                }
            },
            {
                "maidSlotNo", new CustomValueInfo
                {
                    index = (int)Index.MaidSlotNo,
                    name = "追従",
                    defaultValue = -1f,
                }
            },
        };

        public override Dictionary<string, CustomValueInfo> GetCustomValueInfoMap()
        {
            return CustomValueInfoMap;
        }

        public ValueData focalLengthValue => values[(int)Index.FocalLength];
        public ValueData focalSizeValue => values[(int)Index.FocalSize];
        public ValueData apertureValue => values[(int)Index.Aperture];
        public ValueData maxBlurSizeValue => values[(int)Index.MaxBlurSize];
        public ValueData maidSlotNoValue => values[(int)Index.MaidSlotNo];

        public CustomValueInfo focalLengthInfo => CustomValueInfoMap["focalLength"];
        public CustomValueInfo focalSizeInfo => CustomValueInfoMap["focalSize"];
        public CustomValueInfo apertureInfo => CustomValueInfoMap["aperture"];
        public CustomValueInfo maxBlurSizeInfo => CustomValueInfoMap["maxBlurSize"];
        public CustomValueInfo maidSlotNoInfo => CustomValueInfoMap["maidSlotNo"];

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