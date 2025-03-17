using System.Collections.Generic;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TransformDataIKHold : TransformDataBase
    {
        public enum Index
        {
            PositionX = 0,
            PositionY = 1,
            PositionZ = 2,
            IsHold = 3,
            IsAnime = 4
        }

        public override TransformType type => TransformType.IKHold;

        public override int valueCount => 5;

        public override bool hasPosition => true;
        public override bool hasTangent => true;

        public override ValueData[] positionValues
        {
            get => new ValueData[] { 
                values[(int)Index.PositionX], 
                values[(int)Index.PositionY], 
                values[(int)Index.PositionZ] 
            };
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
                    index = (int)Index.IsHold,
                    name = "IK固定",
                    defaultValue = 0,
                }
            },
            {
                "isAnime",
                new CustomValueInfo
                {
                    index = (int)Index.IsAnime,
                    name = "IKアニメ",
                    defaultValue = 0,
                }
            },
        };

        public override Dictionary<string, CustomValueInfo> GetCustomValueInfoMap()
        {
            return CustomValueInfoMap;
        }

        // 値アクセサ
        public ValueData isHoldValue => values[(int)Index.IsHold];
        public ValueData isAnimeValue => values[(int)Index.IsAnime];

        // プロパティアクセサ
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