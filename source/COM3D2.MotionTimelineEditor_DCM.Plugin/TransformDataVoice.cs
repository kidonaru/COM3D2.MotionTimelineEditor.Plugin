
using System.Collections.Generic;
using COM3D2.MotionTimelineEditor.Plugin;

namespace COM3D2.MotionTimelineEditor_DCM.Plugin
{
    public class TransformDataVoice : TransformDataBase
    {
        public override int strValueCount
        {
            get
            {
                return 2;
            }
        }

        public TransformDataVoice()
        {
        }

        private readonly static Dictionary<string, int> StrValueIndexMap = new Dictionary<string, int>
        {
            { "oneTimeVoiceName", 0 },
            { "loopVoiceName", 1 },
        };

        public override Dictionary<string, int> GetStrValueIndexMap()
        {
            return StrValueIndexMap;
        }
    }
}