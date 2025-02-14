using System.Collections.Generic;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TransformDataModelShapeKey : TransformDataBase
    {
        public override TransformType type => TransformType.ModelShapeKey;

        public override int valueCount => 2;
        public override bool hasEasing => !timeline.isTangentModelShapeKey;
        public override bool hasTangent => timeline.isTangentModelShapeKey;

        public override ValueData easingValue => values[0];
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
                    index = 1,
                    name = "値",
                    defaultValue = 0f,
                }
            },
        };

        public override Dictionary<string, CustomValueInfo> GetCustomValueInfoMap()
        {
            return CustomValueInfoMap;
        }

        public ValueData weightValue => this["weight"];

        public float weight
        {
            get => weightValue.value;
            set => weightValue.value = value;
        }
    }
}