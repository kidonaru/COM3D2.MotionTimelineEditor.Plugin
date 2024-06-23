
using System.Collections.Generic;
using COM3D2.MotionTimelineEditor.Plugin;

namespace COM3D2.MotionTimelineEditor_DCM.Plugin
{
    public class TransformDataVoice : TransformDataBase
    {
        public override int valueCount
        {
            get
            {
                return 4;
            }
        }

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

        private readonly static Dictionary<string, int> CustomValueIndexMap = new Dictionary<string, int>
        {
            { "startTime", 0 },
            { "length", 1 },
            { "fadeTime", 2 },
            { "pitch", 3 },
        };

        public override Dictionary<string, int> GetCustomValueIndexMap()
        {
            return CustomValueIndexMap;
        }

        public override float GetInitialCustomValue(string customName)
        {
            if (customName == "fadeTime")
            {
                return 0.1f;
            }
            if (customName == "pitch")
            {
                return 1f;
            }
            return 0f;
        }

        private readonly static Dictionary<string, int> StrValueIndexMap = new Dictionary<string, int>
        {
            { "voiceName", 0 },
            { "loopVoiceName", 1 },
        };

        public override Dictionary<string, int> GetStrValueIndexMap()
        {
            return StrValueIndexMap;
        }
    }
}