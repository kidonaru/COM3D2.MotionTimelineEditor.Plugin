using System.Collections.Generic;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TransformDataModelShapeKey : TransformDataBase
    {
        public enum Index
        {
            Easing = 0,
            Weight = 1
        }

        public override TransformType type => TransformType.ModelShapeKey;

        public override int valueCount => 2;
        public override bool hasEasing => !timeline.isTangentModelShapeKey;
        public override bool hasTangent => timeline.isTangentModelShapeKey;

        public override ValueData easingValue => values[(int)Index.Easing];
        public override ValueData[] tangentValues => values;

        public TransformDataModelShapeKey()
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
    }
}