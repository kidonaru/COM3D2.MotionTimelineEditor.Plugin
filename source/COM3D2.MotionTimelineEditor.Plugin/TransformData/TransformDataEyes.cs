using System.Collections.Generic;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TransformDataEyes : TransformDataBase
    {
        public override TransformType type
        {
            get
            {
                return TransformType.Eyes;
            }
        }

        public override int valueCount
        {
            get
            {
                return 3;
            }
        }

        public override bool hasEasing
        {
            get
            {
                return true;
            }
        }

        public override ValueData easingValue
        {
            get
            {
                return values[0];
            }
        }

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

        public ValueData horizonValue
        {
            get
            {
                return this["horizon"];
            }
        }

        public ValueData verticalValue
        {
            get
            {
                return this["vertical"];
            }
        }

        public float horizon
        {
            get
            {
                return horizonValue.value;
            }
            set
            {
                horizonValue.value = value;
            }
        }

        public float vertical
        {
            get
            {
                return verticalValue.value;
            }
            set
            {
                verticalValue.value = value;
            }
        }
    }
}