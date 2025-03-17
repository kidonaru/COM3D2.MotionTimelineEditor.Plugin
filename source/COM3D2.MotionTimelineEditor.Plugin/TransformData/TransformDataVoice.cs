using System.Collections.Generic;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TransformDataVoice : TransformDataBase
    {
        public enum Index
        {
            StartTime = 0,
            Length = 1,
            FadeTime = 2,
            Pitch = 3
        }

        public enum StrIndex
        {
            VoiceName = 0,
            LoopVoiceName = 1
        }

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
                    index = (int)Index.StartTime,
                    name = "開始",
                    defaultValue = 0f,
                }
            },
            {
                "length",
                new CustomValueInfo
                {
                    index = (int)Index.Length,
                    name = "長さ",
                    defaultValue = 0f,
                }
            },
            {
                "fadeTime",
                new CustomValueInfo
                {
                    index = (int)Index.FadeTime,
                    name = "Fade",
                    defaultValue = 0.1f,
                }
            },
            {
                "pitch",
                new CustomValueInfo
                {
                    index = (int)Index.Pitch,
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
                    index = (int)StrIndex.VoiceName,
                    name = "ボイス名",
                }
            },
            {
                "loopVoiceName",
                new StrValueInfo
                {
                    index = (int)StrIndex.LoopVoiceName,
                    name = "ループボイス",
                }
            },
        };

        public override Dictionary<string, StrValueInfo> GetStrValueInfoMap()
        {
            return StrValueInfoMap;
        }

        public ValueData startTimeValue => values[(int)Index.StartTime];

        public ValueData lengthValue => values[(int)Index.Length];

        public ValueData fadeTimeValue => values[(int)Index.FadeTime];

        public ValueData pitchValue => values[(int)Index.Pitch];

        public string voiceName
        {
            get => strValues[(int)StrIndex.VoiceName];
            set => strValues[(int)StrIndex.VoiceName] = value;
        }

        public string loopVoiceName
        {
            get => strValues[(int)StrIndex.LoopVoiceName];
            set => strValues[(int)StrIndex.LoopVoiceName] = value;
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