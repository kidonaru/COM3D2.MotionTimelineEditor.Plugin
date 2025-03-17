using COM3D2.MotionTimelineEditor.Plugin;
using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor_PngPlacement.Plugin
{
    public class TransformDataPngObject : TransformDataBase
    {
        public enum Index
        {
            PositionX = 0,
            PositionY = 1,
            PositionZ = 2,
            EulerX = 3,
            EulerY = 4,
            EulerZ = 5,
            ColorR = 6,
            ColorG = 7,
            ColorB = 8,
            ColorA = 9,
            Visible = 10,
            Inversion = 11,
            StopRotation = 12,
            ScaleX = 13,
            ScaleMag = 14,
            FixCamera = 15,
            Attach = 16,
            AttachRotation = 17,
            Brightness = 18,
            ScaleZ = 19,
            PrimitiveReferenceX = 20,
            SquareUV = 21,
            Maid = 22,
            APngSpeed = 23,
            APngIsFixedSpeed = 24,
            StopRotationVX = 25,
            StopRotationVY = 26,
            StopRotationVZ = 27,
            FixedPosX = 28,
            FixedPosY = 29,
            FixedPosZ = 30
        }

        public static TransformDataPngObject defaultTrans = new TransformDataPngObject();

        public override TransformType type => TransformType.PngObject;

        public override int valueCount => 31;

        public override bool hasPosition => true;
        public override bool hasEulerAngles => true;
        public override bool hasColor => true;
        public override bool hasVisible => true;
        public override bool hasTangent => true;

        public override ValueData[] positionValues
        {
            get => new ValueData[] { 
                values[(int)Index.PositionX], 
                values[(int)Index.PositionY], 
                values[(int)Index.PositionZ] 
            };
        }

        public override ValueData[] eulerAnglesValues
        {
            get => new ValueData[] { 
                values[(int)Index.EulerX], 
                values[(int)Index.EulerY], 
                values[(int)Index.EulerZ] 
            };
        }

        public override ValueData[] colorValues
        {
            get => new ValueData[] { 
                values[(int)Index.ColorR], 
                values[(int)Index.ColorG], 
                values[(int)Index.ColorB], 
                values[(int)Index.ColorA] 
            };
        }

        public override ValueData visibleValue => values[(int)Index.Visible];

        private List<ValueData> _tangentValues = null;
        public override ValueData[] tangentValues
        {
            get
            {
                if (_tangentValues == null)
                {
                    _tangentValues = new List<ValueData>();
                    _tangentValues.AddRange(baseValues);
                    _tangentValues.AddRange(new ValueData[] { 
                        values[(int)Index.ScaleX], 
                        values[(int)Index.PrimitiveReferenceX] 
                    });
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
                    index = (int)Index.Inversion,
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
                    index = (int)Index.StopRotation,
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
                    index = (int)Index.ScaleX,
                    name = "拡縮",
                    min = 0,
                    max = 10f,
                    step = 0.01f,
                    defaultValue = 1f,
                }
            },
            {
                "scalemag", new CustomValueInfo
                {
                    index = (int)Index.ScaleMag,
                    name = "拡縮率",
                    min = 1,
                    max = 100,
                    step = 1,
                    defaultValue = 1,
                }
            },
            {
                "fixcamera", new CustomValueInfo
                {
                    index = (int)Index.FixCamera,
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
                    index = (int)Index.Attach,
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
                    index = (int)Index.AttachRotation,
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
                    index = (int)Index.Brightness,
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
                    index = (int)Index.ScaleZ,
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
                    index = (int)Index.PrimitiveReferenceX,
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
                    index = (int)Index.SquareUV,
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
                    index = (int)Index.Maid,
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
                    index = (int)Index.APngSpeed,
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
                    index = (int)Index.APngIsFixedSpeed,
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
                    index = (int)Index.StopRotationVX,
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
                    index = (int)Index.StopRotationVY,
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
                    index = (int)Index.StopRotationVZ,
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
                    index = (int)Index.FixedPosX,
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
                    index = (int)Index.FixedPosY,
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
                    index = (int)Index.FixedPosZ,
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

        public ValueData inversionValue => values[(int)Index.Inversion];
        public ValueData stoprotationValue => values[(int)Index.StopRotation];
        public ValueData scalexValue => values[(int)Index.ScaleX];
        public ValueData scalemagValue => values[(int)Index.ScaleMag];
        public ValueData fixcameraValue => values[(int)Index.FixCamera];
        public ValueData attachValue => values[(int)Index.Attach];
        public ValueData attachrotationValue => values[(int)Index.AttachRotation];
        public ValueData brightnessValue => values[(int)Index.Brightness];
        public ValueData scalezValue => values[(int)Index.ScaleZ];
        public ValueData primitivereferencexValue => values[(int)Index.PrimitiveReferenceX];
        public ValueData squareuvValue => values[(int)Index.SquareUV];
        public ValueData maidValue => values[(int)Index.Maid];
        public ValueData apngspeedValue => values[(int)Index.APngSpeed];
        public ValueData apngisfixedspeedValue => values[(int)Index.APngIsFixedSpeed];
        public ValueData[] stoprotationvValues
        {
            get => new ValueData[] { 
                values[(int)Index.StopRotationVX], 
                values[(int)Index.StopRotationVY], 
                values[(int)Index.StopRotationVZ] 
            };
        }
        public ValueData[] fixedposValues
        {
            get => new ValueData[] { 
                values[(int)Index.FixedPosX], 
                values[(int)Index.FixedPosY], 
                values[(int)Index.FixedPosZ] 
            };
        }

        public CustomValueInfo inversionInfo => CustomValueInfoMap["inversion"];
        public CustomValueInfo stoprotationInfo => CustomValueInfoMap["stoprotation"];
        public CustomValueInfo scalexInfo => CustomValueInfoMap["scalex"];
        public CustomValueInfo scalemagInfo => CustomValueInfoMap["scalemag"];
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