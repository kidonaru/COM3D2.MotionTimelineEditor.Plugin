using System.Collections.Generic;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TransformDataEyes : TransformDataBase
    {
        public override TransformType type => TransformType.Eyes;

        public override int valueCount => 3;

        public override bool hasEasing => true;

        public override ValueData easingValue => values[0];

        public TransformDataEyes()
        {
        }

        private readonly static Dictionary<string, CustomValueInfo> CustomValueInfoMap = new Dictionary<string, CustomValueInfo>
        {
            {
                "horizon",
                new CustomValueInfo
                {
                    index = 1,
                    name = "水平",
                    defaultValue = 0f,
                }
            },
            {
                "vertical",
                new CustomValueInfo
                {
                    index = 2,
                    name = "垂直",
                    defaultValue = 0f,
                }
            },
        };

        public override Dictionary<string, CustomValueInfo> GetCustomValueInfoMap()
        {
            return CustomValueInfoMap;
        }

        public ValueData horizonValue => this["horizon"];

        public ValueData verticalValue => this["vertical"];

        public float horizon
        {
            get => horizonValue.value;
            set => horizonValue.value = value;
        }

        public float vertical
        {
            get => verticalValue.value;
            set => verticalValue.value = value;
        }
    }
}