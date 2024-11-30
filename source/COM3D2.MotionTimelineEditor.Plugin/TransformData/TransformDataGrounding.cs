using System.Collections.Generic;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TransformDataGrounding : TransformDataBase
    {
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
                    index = 0,
                    name = "左足の接地",
                    defaultValue = 0,
                }
            },
            {
                "floorHeight",
                new CustomValueInfo
                {
                    index = 1,
                    name = "床の高さ",
                    defaultValue = 0f,
                }
            },
            {
                "footBaseOffset",
                new CustomValueInfo
                {
                    index = 2,
                    name = "足首の高さ",
                    defaultValue = 0.05f,
                }
            },
            {
                "footStretchHeight",
                new CustomValueInfo
                {
                    index = 3,
                    name = "伸ばす高さ",
                    defaultValue = 0.1f,
                }
            },
            {
                "footStretchAngle",
                new CustomValueInfo
                {
                    index = 4,
                    name = "伸ばす角度",
                    defaultValue = 45f,
                }
            },
            {
                "footGroundAngle",
                new CustomValueInfo
                {
                    index = 5,
                    name = "接地時角度",
                    defaultValue = 90f,
                }
            },
            {
                "isGroundingFootR",
                new CustomValueInfo
                {
                    index = 6,
                    name = "右足の接地",
                    defaultValue = 0,
                }
            },
        };

        public override Dictionary<string, CustomValueInfo> GetCustomValueInfoMap()
        {
            return CustomValueInfoMap;
        }

        public ValueData isGroundingFootLValue => this["isGroundingFootL"];

        public ValueData floorHeightValue => this["floorHeight"];

        public ValueData footBaseOffsetValue => this["footBaseOffset"];

        public ValueData footStretchHeightValue => this["footStretchHeight"];

        public ValueData footStretchAngleValue => this["footStretchAngle"];

        public ValueData footGroundAngleValue => this["footGroundAngle"];

        public ValueData isGroundingFootRValue => this["isGroundingFootR"];

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