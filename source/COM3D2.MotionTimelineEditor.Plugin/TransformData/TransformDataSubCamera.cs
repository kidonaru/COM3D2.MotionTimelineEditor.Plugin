using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TransformDataSubCamera : TransformDataBase
    {
        public enum Index
        {
            Visible = 0,
            Easing = 1,
            PositionX = 2,
            PositionY = 3,
            PositionZ = 4,
            RotationX = 5,
            RotationY = 6,
            RotationZ = 7,
            FoV = 8,
            ViewportX = 9,
            ViewportY = 10,
            ViewportW = 11,
            ViewportH = 12,
            MaidSlotNo = 13,
            MaidPointType = 14,
            FollowRotation = 15
        }

        public override TransformType type => TransformType.SubCamera;

        public override int valueCount => 16;

        public override bool hasPosition => true;
        public override bool hasEulerAngles => true;
        public override bool hasVisible => true;
        public override bool hasEasing => !timeline.isTangentCamera;
        public override bool hasTangent => timeline.isTangentCamera;

        // カメラはキーフレームのEnableで明示的に有効化する
        public override bool initialVisible => true;

        public override ValueData[] positionValues
        {
            get => new ValueData[]
            {
                values[(int)Index.PositionX],
                values[(int)Index.PositionY],
                values[(int)Index.PositionZ]
            };
        }

        public override ValueData[] eulerAnglesValues
        {
            get => new ValueData[]
            {
                values[(int)Index.RotationX],
                values[(int)Index.RotationY],
                values[(int)Index.RotationZ]
            };
        }

        public override ValueData easingValue => values[(int)Index.Easing];

        public override ValueData visibleValue => values[(int)Index.Visible];

        public override ValueData[] tangentValues => values;

        private readonly static Dictionary<string, CustomValueInfo> CustomValueInfoMap = new Dictionary<string, CustomValueInfo>
        {
            {
                "fov", new CustomValueInfo
                {
                    index = (int)Index.FoV,
                    name = "FoV",
                    defaultValue = 35f,
                }
            },
            {
                "viewportX", new CustomValueInfo
                {
                    index = (int)Index.ViewportX,
                    name = "VP X",
                    defaultValue = SubCameraManager.DefaultViewport.x,
                }
            },
            {
                "viewportY", new CustomValueInfo
                {
                    index = (int)Index.ViewportY,
                    name = "VP Y",
                    defaultValue = SubCameraManager.DefaultViewport.y,
                }
            },
            {
                "viewportW", new CustomValueInfo
                {
                    index = (int)Index.ViewportW,
                    name = "VP 幅",
                    defaultValue = SubCameraManager.DefaultViewport.width,
                }
            },
            {
                "viewportH", new CustomValueInfo
                {
                    index = (int)Index.ViewportH,
                    name = "VP 高",
                    defaultValue = SubCameraManager.DefaultViewport.height,
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
            {
                "maidPointType", new CustomValueInfo
                {
                    index = (int)Index.MaidPointType,
                    name = "追従点",
                    min = 0f,
                    max = (float)MaidPointType.Bip01,
                    step = 1f,
                    // 既存データ互換のため、旧仕様の追従先(股)をデフォルトとする
                    defaultValue = (float)MaidPointType.Crotch,
                }
            },
            {
                "followRotation", new CustomValueInfo
                {
                    index = (int)Index.FollowRotation,
                    name = "向き反映",
                    min = 0f,
                    max = 1f,
                    step = 1f,
                    defaultValue = 0f,
                }
            },
        };

        public override Dictionary<string, CustomValueInfo> GetCustomValueInfoMap()
        {
            return CustomValueInfoMap;
        }

        // 値アクセサ
        public ValueData fovValue => values[(int)Index.FoV];
        public ValueData maidSlotNoValue => values[(int)Index.MaidSlotNo];
        public ValueData maidPointTypeValue => values[(int)Index.MaidPointType];
        public ValueData followRotationValue => values[(int)Index.FollowRotation];

        public ValueData[] viewportValues
        {
            get => new ValueData[]
            {
                values[(int)Index.ViewportX],
                values[(int)Index.ViewportY],
                values[(int)Index.ViewportW],
                values[(int)Index.ViewportH]
            };
        }

        // プロパティアクセサ
        public float fov
        {
            get => fovValue.value;
            set => fovValue.value = value;
        }

        public int maidSlotNo
        {
            get => maidSlotNoValue.intValue;
            set => maidSlotNoValue.intValue = value;
        }

        public MaidPointType maidPointType
        {
            get => (MaidPointType)maidPointTypeValue.intValue;
            set => maidPointTypeValue.intValue = (int)value;
        }

        public bool followRotation
        {
            get => followRotationValue.boolValue;
            set => followRotationValue.boolValue = value;
        }

        public Rect viewport
        {
            get => viewportValues.ToRect();
            set => viewportValues.FromRect(value);
        }

        public TransformDataSubCamera()
        {
        }

        public override void FromXml(TransformXml xml)
        {
            base.FromXml(xml);

            // 追従点を持たない旧データは従来の追従先(股)として読み込む
            if (xml.values != null && xml.values.Length <= (int)Index.MaidPointType)
            {
                maidPointType = MaidPointType.Crotch;
            }
        }
    }
}
