using System.Collections.Generic;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TransformDataDepthOfField : TransformDataBase
    {
        public override TransformType type
        {
            get
            {
                return TransformType.DepthOfField;
            }
        }

        public override int valueCount
        {
            get
            {
                return 7;
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

        public override ValueData visibleValue
        {
            get
            {
                return values[1];
            }
        }

        public override ValueData easingValue
        {
            get
            {
                return values[0];
            }
        }

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

        public ValueData focalLengthValue
        {
            get
            {
                return this["focalLength"];
            }
        }

        public ValueData focalSizeValue
        {
            get
            {
                return this["focalSize"];
            }
        }

        public ValueData apertureValue
        {
            get
            {
                return this["aperture"];
            }
        }

        public ValueData maxBlurSizeValue
        {
            get
            {
                return this["maxBlurSize"];
            }
        }

        public ValueData maidSlotNoValue
        {
            get
            {
                return this["maidSlotNo"];
            }
        }

        public float focalLength
        {
            get
            {
                return focalLengthValue.value;
            }
            set
            {
                focalLengthValue.value = value;
            }
        }

        public float focalSize
        {
            get
            {
                return focalSizeValue.value;
            }
            set
            {
                focalSizeValue.value = value;
            }
        }

        public float aperture
        {
            get
            {
                return apertureValue.value;
            }
            set
            {
                apertureValue.value = value;
            }
        }

        public float maxBlurSize
        {
            get
            {
                return maxBlurSizeValue.value;
            }
            set
            {
                maxBlurSizeValue.value = value;
            }
        }

        public int maidSlotNo
        {
            get
            {
                return maidSlotNoValue.intValue;
            }
            set
            {
                maidSlotNoValue.intValue = value;
            }
        }

        public DepthOfFieldData depthOfField
        {
            get
            {
                return new DepthOfFieldData
                {
                    enabled = visible,
                    focalLength = focalLength,
                    focalSize = focalSize,
                    aperture = aperture,
                    maxBlurSize = maxBlurSize,
                    maidSlotNo = maidSlotNo,
                };
            }
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