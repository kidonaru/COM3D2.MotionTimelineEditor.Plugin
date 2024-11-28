
using System.Collections.Generic;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TransformDataVoice : TransformDataBase
    {
        public override TransformType type
        {
            get
            {
                return TransformType.Voice;
            }
        }

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

        public ValueData startTimeValue
        {
            get
            {
                return this["startTime"];
            }
        }

        public ValueData lengthValue
        {
            get
            {
                return this["length"];
            }
        }

        public ValueData fadeTimeValue
        {
            get
            {
                return this["fadeTime"];
            }
        }

        public ValueData pitchValue
        {
            get
            {
                return this["pitch"];
            }
        }

        public string voiceName
        {
            get
            {
                return GetStrValue("voiceName");
            }
            set
            {
                SetStrValue("voiceName", value);
            }
        }

        public string loopVoiceName
        {
            get
            {
                return GetStrValue("loopVoiceName");
            }
            set
            {
                SetStrValue("loopVoiceName", value);
            }
        }

        public float startTime
        {
            get
            {
                return startTimeValue.value;
            }
            set
            {
                startTimeValue.value = value;
            }
        }

        public float length
        {
            get
            {
                return lengthValue.value;
            }
            set
            {
                lengthValue.value = value;
            }
        }

        public float fadeTime
        {
            get
            {
                return fadeTimeValue.value;
            }
            set
            {
                fadeTimeValue.value = value;
            }
        }

        public float pitch
        {
            get
            {
                return pitchValue.value;
            }
            set
            {
                pitchValue.value = value;
            }
        }
    }
}