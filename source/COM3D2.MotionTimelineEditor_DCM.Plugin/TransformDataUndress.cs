using System.Collections.Generic;
using COM3D2.MotionTimelineEditor.Plugin;

namespace COM3D2.MotionTimelineEditor_DCM.Plugin
{
    public class TransformDataUndress : TransformDataBase
    {
        public override int valueCount
        {
            get
            {
                return 1;
            }
        }

        public TransformDataUndress()
        {
        }

        private readonly static Dictionary<string, CustomValueInfo> CustomValueInfoMap = new Dictionary<string, CustomValueInfo>
        {
            {
                "isVisible",
                new CustomValueInfo
                {
                    index = 0,
                    name = "表示",
                    defaultValue = 0f,
                }
            },
        };

        public override Dictionary<string, CustomValueInfo> GetCustomValueInfoMap()
        {
            return CustomValueInfoMap;
        }
    }
}