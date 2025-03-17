using System.Collections.Generic;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TransformDataGrounding : TransformDataBase
    {
        public enum Index
        {
            IsGroundingFootL = 0,
            FloorHeight = 1,
            FootBaseOffset = 2,
            FootStretchHeight = 3,
            FootStretchAngle = 4,
            FootGroundAngle = 5,
            IsGroundingFootR = 6
        }

        public override TransformType type => TransformType.Grounding;

        public override int valueCount => 7;

        public TransformDataGrounding()
        {
        }

        private readonly static Dictionary<string, CustomValueInfo> CustomValueInfoMap = new Dictionary<string, CustomValueInfo>
        {
            {
                "isGroundingFootL",
                new CustomValueInfo
                {
                    index = (int)Index.IsGroundingFootL,
                    name = "左足の接地",
                    defaultValue = 0,
                }
            },
            {
                "floorHeight",
                new CustomValueInfo
                {
                    index = (int)Index.FloorHeight,
                    name = "床の高さ",
                    defaultValue = 0f,
                }
            },
            {
                "footBaseOffset",
                new CustomValueInfo
                {
                    index = (int)Index.FootBaseOffset,
                    name = "足首の高さ",
                    defaultValue = 0.05f,
                }
            },
            {
                "footStretchHeight",
                new CustomValueInfo
                {
                    index = (int)Index.FootStretchHeight,
                    name = "伸ばす高さ",
                    defaultValue = 0.1f,
                }
            },
            {
                "footStretchAngle",
                new CustomValueInfo
                {
                    index = (int)Index.FootStretchAngle,
                    name = "伸ばす角度",
                    defaultValue = 45f,
                }
            },
            {
                "footGroundAngle",
                new CustomValueInfo
                {
                    index = (int)Index.FootGroundAngle,
                    name = "接地時角度",
                    defaultValue = 90f,
                }
            },
            {
                "isGroundingFootR",
                new CustomValueInfo
                {
                    index = (int)Index.IsGroundingFootR,
                    name = "右足の接地",
                    defaultValue = 0,
                }
            },
        };

        public override Dictionary<string, CustomValueInfo> GetCustomValueInfoMap()
        {
            return CustomValueInfoMap;
        }

        public ValueData isGroundingFootLValue => values[(int)Index.IsGroundingFootL];
        public ValueData floorHeightValue => values[(int)Index.FloorHeight];
        public ValueData footBaseOffsetValue => values[(int)Index.FootBaseOffset];
        public ValueData footStretchHeightValue => values[(int)Index.FootStretchHeight];
        public ValueData footStretchAngleValue => values[(int)Index.FootStretchAngle];
        public ValueData footGroundAngleValue => values[(int)Index.FootGroundAngle];
        public ValueData isGroundingFootRValue => values[(int)Index.IsGroundingFootR];

        public bool isGroundingFootL
        {
            get => isGroundingFootLValue.boolValue;
            set => isGroundingFootLValue.boolValue = value;
        }

        public float floorHeight
        {
            get => floorHeightValue.value;
            set => floorHeightValue.value = value;
        }

        public float footBaseOffset
        {
            get => footBaseOffsetValue.value;
            set => footBaseOffsetValue.value = value;
        }

        public float footStretchHeight
        {
            get => footStretchHeightValue.value;
            set => footStretchHeightValue.value = value;
        }

        public float footStretchAngle
        {
            get => footStretchAngleValue.value;
            set => footStretchAngleValue.value = value;
        }

        public float footGroundAngle
        {
            get => footGroundAngleValue.value;
            set => footGroundAngleValue.value = value;
        }

        public bool isGroundingFootR
        {
            get => isGroundingFootRValue.boolValue;
            set => isGroundingFootRValue.boolValue = value;
        }
    }
}