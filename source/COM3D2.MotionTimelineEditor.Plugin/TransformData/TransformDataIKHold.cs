using System.Collections.Generic;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TransformDataIKHold : TransformDataBase
    {
        public override int valueCount
        {
            get
            {
                return 5;
            }
        }

        public override bool hasPosition
        {
            get
            {
                return true;
            }
        }

        public override bool hasTangent
        {
            get
            {
                return true;
            }
        }

        public override ValueData[] positionValues
        {
            get
            {
                return new ValueData[] { values[0], values[1], values[2] };
            }
        }

        public override ValueData[] tangentValues
        {
            get
            {
                return positionValues;
            }
        }

        public override bool isGlobal
        {
            get
            {
                return true;
            }
        }

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

        public ValueData isHoldValue
        {
            get
            {
                return this["isHold"];
            }
        }

        public ValueData isAnimeValue
        {
            get
            {
                return this["isAnime"];
            }
        }

        public bool isHold
        {
            get
            {
                return isHoldValue.boolValue;
            }
            set
            {
                isHoldValue.boolValue = value;
            }
        }

        public bool isAnime
        {
            get
            {
                return isAnimeValue.boolValue;
            }
            set
            {
                isAnimeValue.boolValue = value;
            }
        }
    }
}