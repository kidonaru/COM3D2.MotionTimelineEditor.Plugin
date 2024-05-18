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

        private readonly static Dictionary<string, int> CustomValueIndexMap = new Dictionary<string, int>
        {
            { "morphValue", 0 },
        };

        public override Dictionary<string, int> GetCustomValueIndexMap()
        {
            return CustomValueIndexMap;
        }
    }
}