using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TransformDataSubCamera : TransformDataBase
    {
        public enum Index
        {
            PositionX = 0,
            PositionY = 1,
            PositionZ = 2,
            RotationX = 3,
            RotationY = 4,
            RotationZ = 5,
            Easing = 6,
            FoV = 7,
            ViewportX = 8,
            ViewportY = 9,
            ViewportW = 10,
            ViewportH = 11,
            MaidSlotNo = 12,
            Visible = 13
        }

        public override TransformType type => TransformType.SubCamera;

        public override int valueCount => 14;

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
                    defaultValue = 0.75f,
                }
            },
            {
                "viewportY", new CustomValueInfo
                {
                    index = (int)Index.ViewportY,
                    name = "VP Y",
                    defaultValue = 0f,
                }
            },
            {
                "viewportW", new CustomValueInfo
                {
                    index = (int)Index.ViewportW,
                    name = "VP 幅",
                    defaultValue = 0.25f,
                }
            },
            {
                "viewportH", new CustomValueInfo
                {
                    index = (int)Index.ViewportH,
                    name = "VP 高",
                    defaultValue = 0.25f,
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

        // 値アクセサ
        public ValueData fovValue => values[(int)Index.FoV];
        public ValueData maidSlotNoValue => values[(int)Index.MaidSlotNo];

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

        public Rect viewport
        {
            get => viewportValues.ToRect();
            set => viewportValues.FromRect(value);
        }

        public TransformDataSubCamera()
        {
        }
    }
}
