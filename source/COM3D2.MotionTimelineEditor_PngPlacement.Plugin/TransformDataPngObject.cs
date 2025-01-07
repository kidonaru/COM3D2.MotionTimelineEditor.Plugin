using COM3D2.MotionTimelineEditor.Plugin;
using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor_PngPlacement.Plugin
{
    public class TransformDataPngObject : TransformDataBase
    {
        public static TransformDataPngObject defaultTrans = new TransformDataPngObject();

        public override TransformType type => TransformType.PngObject;

        public override int valueCount => 32;

        public override bool hasPosition => true;
        public override bool hasEulerAngles => true;
        public override bool hasColor => true;
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

        public override ValueData visibleValue => values[10];

        private List<ValueData> _tangentValues = null;
        public override ValueData[] tangentValues
        {
            get
            {
                if (_tangentValues == null)
                {
                    _tangentValues = new List<ValueData>();
                    _tangentValues.AddRange(baseValues);
                    _tangentValues.AddRange(new ValueData[] { values[13], values[20] });
                }
                return _tangentValues.ToArray();
            }
        }

        public TransformDataPngObject()
        {
        }

        private readonly static Dictionary<string, CustomValueInfo> CustomValueInfoMap = new Dictionary<string, CustomValueInfo>
        {
            {
                "inversion", new CustomValueInfo
                {
                    index = 11,
                    name = "左右反転",
                    min = 0,
                    max = 1,
                    step = 1,
                    defaultValue = 0,
                }
            },
            {
                "stoprotation", new CustomValueInfo
                {
                    index = 12,
                    name = "カメラ追従回転停止",
                    min = 0,
                    max = 1,
                    step = 1,
                    defaultValue = 0,
                }
            },
            {
                "scalex", new CustomValueInfo
                {
                    index = 13,
                    name = "Scale",
                    min = 0,
                    max = 10f,
                    step = 0.01f,
                    defaultValue = 1f,
                }
            },
            {
                "scalemag", new CustomValueInfo
                {
                    index = 14,
                    name = "S倍率",
                    min = 1,
                    max = 100,
                    step = 1,
                    defaultValue = 1,
                }
            },
            {
                "rq", new CustomValueInfo
                {
                    index = 15,
                    name = "RQ",
                    min = 0,
                    max = 10000,
                    step = 1,
                    defaultValue = 3200,
                }
            },
            {
                "fixcamera", new CustomValueInfo
                {
                    index = 16,
                    name = "カメラ相対位置固定",
                    min = 0,
                    max = 1,
                    step = 1,
                    defaultValue = 0,
                }
            },
            {
                "attach", new CustomValueInfo
                {
                    index = 17,
                    name = "アタッチ",
                    min = 0,
                    max = (int) PngAttachPoint.leg2R,
                    step = 1,
                    defaultValue = 0,
                }
            },
            {
                "attachrotation", new CustomValueInfo
                {
                    index = 18,
                    name = "カメラ追従回転",
                    min = 0,
                    max = 1,
                    step = 1,
                    defaultValue = 0,
                }
            },
            {
                "brightness", new CustomValueInfo
                {
                    index = 19,
                    name = "明度",
                    min = 0,
                    max = 255,
                    step = 1,
                    defaultValue = 255,
                }
            },
            {
                "scalez", new CustomValueInfo
                {
                    index = 20,
                    name = "SZ",
                    min = 0,
                    max = 10f,
                    step = 0.01f,
                    defaultValue = 1f,
                }
            },
            {
                "primitivereferencex", new CustomValueInfo
                {
                    index = 21,
                    name = "primitivereferencex",
                    min = 0,
                    max = 1,
                    step = 1,
                    defaultValue = 1,
                }
            },
            {
                "squareuv", new CustomValueInfo
                {
                    index = 22,
                    name = "squareuv",
                    min = 0,
                    max = 1,
                    step = 1,
                    defaultValue = 0,
                }
            },
            {
                "maid", new CustomValueInfo
                {
                    index = 23,
                    name = "maid",
                    min = -1,
                    max = 10,
                    step = 1,
                    defaultValue = -1,
                }
            },
            {
                "apngspeed", new CustomValueInfo
                {
                    index = 24,
                    name = "ASpeed",
                    min = 0,
                    max = 5f,
                    step = 0.01f,
                    defaultValue = 1f,
                }
            },
            {
                "apngisfixedspeed", new CustomValueInfo
                {
                    index = 25,
                    name = "固定速度",
                    min = 0,
                    max = 1,
                    step = 1,
                    defaultValue = 0,
                }
            },
            {
                "stoprotationvx", new CustomValueInfo
                {
                    index = 26,
                    name = "SRX",
                    min = -180f,
                    max = 180f,
                    step = 0.1f,
                    defaultValue = 0,
                }
            },
            {
                "stoprotationvy", new CustomValueInfo
                {
                    index = 27,
                    name = "SRY",
                    min = -180f,
                    max = 180f,
                    step = 0.1f,
                    defaultValue = 0,
                }
            },
            {
                "stoprotationvz", new CustomValueInfo
                {
                    index = 28,
                    name = "SRZ",
                    min = -180f,
                    max = 180f,
                    step = 0.1f,
                    defaultValue = 0,
                }
            },
            {
                "fixedposx", new CustomValueInfo
                {
                    index = 29,
                    name = "FX",
                    min = -100f,
                    max = 100f,
                    step = 0.01f,
                    defaultValue = 0,
                }
            },
            {
                "fixedposy", new CustomValueInfo
                {
                    index = 30,
                    name = "FY",
                    min = -100f,
                    max = 100f,
                    step = 0.01f,
                    defaultValue = 0,
                }
            },
            {
                "fixedposz", new CustomValueInfo
                {
                    index = 31,
                    name = "FZ",
                    min = -100f,
                    max = 100f,
                    step = 0.01f,
                    defaultValue = 0,
                }
            },
        };

        public override Dictionary<string, CustomValueInfo> GetCustomValueInfoMap()
        {
            return CustomValueInfoMap;
        }

        public ValueData inversionValue => this["inversion"];
        public ValueData stoprotationValue => this["stoprotation"];
        public ValueData scalexValue => this["scalex"];
        public ValueData scalemagValue => this["scalemag"];
        public ValueData rqValue => this["rq"];
        public ValueData fixcameraValue => this["fixcamera"];
        public ValueData attachValue => this["attach"];
        public ValueData attachrotationValue => this["attachrotation"];
        public ValueData brightnessValue => this["brightness"];
        public ValueData scalezValue => this["scalez"];
        public ValueData primitivereferencexValue => this["primitivereferencex"];
        public ValueData squareuvValue => this["squareuv"];
        public ValueData maidValue => this["maid"];
        public ValueData apngspeedValue => this["apngspeed"];
        public ValueData apngisfixedspeedValue => this["apngisfixedspeed"];
        public ValueData[] stoprotationvValues
        {
            get => new ValueData[] { this["stoprotationvx"], this["stoprotationvy"], this["stoprotationvz"] };
        }
        public ValueData[] fixedposValues
        {
            get => new ValueData[] { this["fixedposx"], this["fixedposy"], this["fixedposz"] };
        }

        public CustomValueInfo inversionInfo => CustomValueInfoMap["inversion"];
        public CustomValueInfo stoprotationInfo => CustomValueInfoMap["stoprotation"];
        public CustomValueInfo scalexInfo => CustomValueInfoMap["scalex"];
        public CustomValueInfo scalemagInfo => CustomValueInfoMap["scalemag"];
        public CustomValueInfo rqInfo => CustomValueInfoMap["rq"];
        public CustomValueInfo fixcameraInfo => CustomValueInfoMap["fixcamera"];
        public CustomValueInfo attachInfo => CustomValueInfoMap["attach"];
        public CustomValueInfo attachrotationInfo => CustomValueInfoMap["attachrotation"];
        public CustomValueInfo brightnessInfo => CustomValueInfoMap["brightness"];
        public CustomValueInfo scalezInfo => CustomValueInfoMap["scalez"];
        public CustomValueInfo primitivereferencexInfo => CustomValueInfoMap["primitivereferencex"];
        public CustomValueInfo squareuvInfo => CustomValueInfoMap["squareuv"];
        public CustomValueInfo maidInfo => CustomValueInfoMap["maid"];
        public CustomValueInfo apngspeedInfo => CustomValueInfoMap["apngspeed"];
        public CustomValueInfo apngisfixedspeedInfo => CustomValueInfoMap["apngisfixedspeed"];

        public bool inversion
        {
            get => inversionValue.boolValue;
            set => inversionValue.boolValue = value;
        }
        public bool stoprotation
        {
            get => stoprotationValue.boolValue;
            set => stoprotationValue.boolValue = value;
        }
        public float scalex
        {
            get => scalexValue.value;
            set => scalexValue.value = value;
        }
        public int scalemag
        {
            get => scalemagValue.intValue;
            set => scalemagValue.intValue = value;
        }
        public int rq
        {
            get => rqValue.intValue;
            set => rqValue.intValue = value;
        }
        public bool fixcamera
        {
            get => fixcameraValue.boolValue;
            set => fixcameraValue.boolValue = value;
        }
        public PngAttachPoint attach
        {
            get => (PngAttachPoint) attachValue.intValue;
            set => attachValue.intValue = (int) value;
        }
        public bool attachrotation
        {
            get => attachrotationValue.boolValue;
            set => attachrotationValue.boolValue = value;
        }
        public byte brightness
        {
            get => (byte) brightnessValue.intValue;
            set => brightnessValue.intValue = value;
        }
        public float scalez
        {
            get => scalezValue.value;
            set => scalezValue.value = value;
        }
        public bool primitivereferencex
        {
            get => primitivereferencexValue.boolValue;
            set => primitivereferencexValue.boolValue = value;
        }
        public bool squareuv
        {
            get => squareuvValue.boolValue;
            set => squareuvValue.boolValue = value;
        }
        public int maid
        {
            get => maidValue.intValue;
            set => maidValue.intValue = value;
        }
        public float apngspeed
        {
            get => apngspeedValue.value;
            set => apngspeedValue.value = value;
        }
        public bool apngisfixedspeed
        {
            get => apngisfixedspeedValue.boolValue;
            set => apngisfixedspeedValue.boolValue = value;
        }
        public Vector3 stoprotationv
        {
            get => stoprotationvValues.ToVector3();
            set => stoprotationvValues.FromVector3(value);
        }
        public Vector3 fixedpos
        {
            get => fixedposValues.ToVector3();
            set => fixedposValues.FromVector3(value);
        }
    }
}