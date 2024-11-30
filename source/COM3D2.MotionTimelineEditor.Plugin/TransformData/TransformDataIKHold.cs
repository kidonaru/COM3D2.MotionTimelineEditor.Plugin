using System.Collections.Generic;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TransformDataIKHold : TransformDataBase
    {
        public override TransformType type => TransformType.IKHold;

        public override int valueCount => 5;

        public override bool hasPosition => true;

        public override bool hasTangent => true;

        public override ValueData[] positionValues
        {
            get => new ValueData[] { values[0], values[1], values[2] };
        }

        public override ValueData[] tangentValues => positionValues;

        public override bool isGlobal => true;

        public TransformDataIKHold()
        {
        }

        private readonly static Dictionary<string, CustomValueInfo> CustomValueInfoMap = new Dictionary<string, CustomValueInfo>
        {
            {
                "isHold",
                new CustomValueInfo
                {
                    index = 3,
                    name = "IK固定",
                    defaultValue = 0,
                }
            },
            {
                "isAnime",
                new CustomValueInfo
                {
                    index = 4,
                    name = "IKアニメ",
                    defaultValue = 0,
                }
            },
        };

        public override Dictionary<string, CustomValueInfo> GetCustomValueInfoMap()
        {
            return CustomValueInfoMap;
        }

        public ValueData isHoldValue => this["isHold"];

        public ValueData isAnimeValue => this["isAnime"];

        public bool isHold
        {
            get => isHoldValue.boolValue;
            set => isHoldValue.boolValue = value;
        }

        public bool isAnime
        {
            get => isAnimeValue.boolValue;
            set => isAnimeValue.boolValue = value;
        }
    }
}