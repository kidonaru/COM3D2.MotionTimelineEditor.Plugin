using System.Collections.Generic;
using COM3D2.MotionTimelineEditor.Plugin;

namespace COM3D2.MotionTimelineEditor_DCM.Plugin
{
    public class TransformDataEyes : TransformDataBase
    {
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

        private readonly static Dictionary<string, int> CustomValueIndexMap = new Dictionary<string, int>
        {
            { "horizon", 1 },
            { "vertical", 2 },
        };

        public override Dictionary<string, int> GetCustomValueIndexMap()
        {
            return CustomValueIndexMap;
        }
    }
}