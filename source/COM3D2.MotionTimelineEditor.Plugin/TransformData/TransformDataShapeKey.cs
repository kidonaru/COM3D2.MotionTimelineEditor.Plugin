using System.Collections.Generic;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TransformDataShapeKey : TransformDataBase
    {
        public enum Index
        {
            Easing = 0,
            Weight = 1
        }

        public override TransformType type => TransformType.ShapeKey;

        public override int valueCount => 2;

        public override bool hasEasing => true;

        public override ValueData easingValue => values[(int)Index.Easing];

        public TransformDataShapeKey()
        {
        }

        private readonly static Dictionary<string, CustomValueInfo> CustomValueInfoMap = new Dictionary<string, CustomValueInfo>
        {
            {
                "weight",
                new CustomValueInfo
                {
                    index = (int)Index.Weight,
                    name = "値",
                    defaultValue = 0f,
                }
            },
        };

        public override Dictionary<string, CustomValueInfo> GetCustomValueInfoMap()
        {
            return CustomValueInfoMap;
        }

        // 値アクセサ
        public ValueData weightValue => values[(int)Index.Weight];

        // プロパティアクセサ
        public float weight
        {
            get => weightValue.value;
            set => weightValue.value = value;
        }

        public string slotName;
        public int maidSlotNo;
    }
}