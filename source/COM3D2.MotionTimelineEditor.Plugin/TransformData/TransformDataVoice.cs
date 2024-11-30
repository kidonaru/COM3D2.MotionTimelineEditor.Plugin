
using System.Collections.Generic;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TransformDataVoice : TransformDataBase
    {
        public override TransformType type => TransformType.Voice;

        public override int valueCount => 4;

        public override int strValueCount => 2;

        public TransformDataVoice()
        {
        }

        private readonly static Dictionary<string, CustomValueInfo> CustomValueInfoMap = new Dictionary<string, CustomValueInfo>
        {
            {
                "startTime",
                new CustomValueInfo
                {
                    index = 0,
                    name = "開始",
                    defaultValue = 0f,
                }
            },
            {
                "length",
                new CustomValueInfo
                {
                    index = 1,
                    name = "長さ",
                    defaultValue = 0f,
                }
            },
            {
                "fadeTime",
                new CustomValueInfo
                {
                    index = 2,
                    name = "Fade",
                    defaultValue = 0.1f,
                }
            },
            {
                "pitch",
                new CustomValueInfo
                {
                    index = 3,
                    name = "音程",
                    defaultValue = 1f,
                }
            },
        };

        public override Dictionary<string, CustomValueInfo> GetCustomValueInfoMap()
        {
            return CustomValueInfoMap;
        }

        private readonly static Dictionary<string, StrValueInfo> StrValueInfoMap = new Dictionary<string, StrValueInfo>
        {
            {
                "voiceName",
                new StrValueInfo
                {
                    index = 0,
                    name = "ボイス名",
                }
            },
            {
                "loopVoiceName",
                new StrValueInfo
                {
                    index = 1,
                    name = "ループボイス",
                }
            },
        };

        public override Dictionary<string, StrValueInfo> GetStrValueInfoMap()
        {
            return StrValueInfoMap;
        }

        public ValueData startTimeValue => this["startTime"];

        public ValueData lengthValue => this["length"];

        public ValueData fadeTimeValue => this["fadeTime"];

        public ValueData pitchValue => this["pitch"];

        public string voiceName
        {
            get => GetStrValue("voiceName");
            set => SetStrValue("voiceName", value);
        }

        public string loopVoiceName
        {
            get => GetStrValue("loopVoiceName");
            set => SetStrValue("loopVoiceName", value);
        }

        public float startTime
        {
            get => startTimeValue.value;
            set => startTimeValue.value = value;
        }

        public float length
        {
            get => lengthValue.value;
            set => lengthValue.value = value;
        }

        public float fadeTime
        {
            get => fadeTimeValue.value;
            set => fadeTimeValue.value = value;
        }

        public float pitch
        {
            get => pitchValue.value;
            set => pitchValue.value = value;
        }
    }
}