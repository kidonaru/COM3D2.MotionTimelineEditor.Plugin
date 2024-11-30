using System.Collections.Generic;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TransformDataShapeKey : TransformDataBase
    {
        public override TransformType type => TransformType.ShapeKey;

        public override int valueCount => 2;

        public override bool hasEasing => true;

        public override ValueData easingValue => values[0];

        public TransformDataShapeKey()
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

        public string slotName;
        public int maidSlotNo;
    }
}