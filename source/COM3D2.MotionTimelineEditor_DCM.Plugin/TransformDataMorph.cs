using System.Collections.Generic;
using COM3D2.MotionTimelineEditor.Plugin;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor_DCM.Plugin
{
    public class TransformDataMorph : TransformDataBase
    {
        public override int valueCount
        {
            get
            {
                return 1;
            }
        }

        public TransformDataMorph()
        {
        }

        private readonly static Dictionary<string, CustomValueInfo> CustomValueInfoMap = new Dictionary<string, CustomValueInfo>
        {
            {
                "morphValue",
                new CustomValueInfo
                {
                    index = 0,
                    name = "値",
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