using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public enum Personality
    {
        なし,
        ツンデレ,
        クーデレ,
        純真,
        ヤンデレ,
        お姉ちゃん,
        ボクっ娘,
        ドＳ,
        無垢,
        真面目,
        凜デレ,
        文学少女,
        小悪魔,
        おしとやか,
        メイド秘書,
        妹,
        無愛想,
        お嬢様,
        幼馴染,
        ドＭ,
        腹黒,
        淑女,
        気さく,
    }

    public static class ScriptLoader
    {
        public static string ReadScript(string filename)
        {
            if (!filename.EndsWith(".ks", StringComparison.Ordinal))
            {
                filename += ".ks";
            }

            byte[] buffer = BinaryLoader.ReadAFileBase(filename);
            if (buffer == null)
            {
                return null;
            }

            return NUty.SjisToUnicode(buffer);
        }

        private class VoiceEntity
        {
            public string scriptFileName = "";
            public int lineNumber;
            public string voiceText = "";
            public string label = "";
            public string subLabel = "";
            public string motionFile = "";
            public string motionLabel = "";
            public string face = "";
            public string faceBlend = "";
            public bool repeat;
            public string arcName = "";

            private int _voiceHash = 0;
            private string _voiceName = "";
            public string voiceName
            {
                get
                {
                    return _voiceName;
                }
                set
                {
                    _voiceName = value;
                    _voiceHash = value.GetHashCode();
                }
            }

            private static Dictionary<string, Personality> PrefixToPersonalTable = new Dictionary<string, Personality>
            {
                { "S0_", Personality.ツンデレ },
                { "S1_", Personality.クーデレ },
                { "S2_", Personality.純真 },
                { "S3_", Personality.ヤンデレ },
                { "S4_", Personality.お姉ちゃん },
                { "S5_", Personality.ボクっ娘 },
                { "S6_", Personality.ドＳ },
                { "H0_", Personality.無垢 },
                { "H1_", Personality.真面目 },
                { "H2_", Personality.凜デレ },
                { "H3_", Personality.文学少女 },
                { "H4_", Personality.小悪魔 },
                { "H5_", Personality.おしとやか },
                { "H6_", Personality.メイド秘書 },
                { "H7_", Personality.妹 },
                { "H8_", Personality.無愛想 },
                { "H9_", Personality.お嬢様 },
                { "H10", Personality.幼馴染 },
                { "H11", Personality.ドＭ },
                { "H12", Personality.腹黒 },
                { "V0_", Personality.淑女 },
                { "V1_", Personality.気さく },
            };

            public Personality personality
            {
                get
                {
                    string key = this.voiceName.Substring(0, 3);
                    Personality value;
                    if (PrefixToPersonalTable.TryGetValue(key, out value))
                    {
                        return value;
                    }
                    return Personality.なし;
                }
            }

            public static string GetCsvHeader()
            {
                var stringBuilder = new StringBuilder();
                stringBuilder.Append("\"Name\"");
                stringBuilder.Append(",\"Text\"");
                stringBuilder.Append(",\"Script\"");
                stringBuilder.Append(",\"Line Number\"");
                stringBuilder.Append(",\"Label\"");
                stringBuilder.Append(",\"SubLabel\"");
                stringBuilder.Append(",\"Motion File\"");
                stringBuilder.Append(",\"Motion Label\"");
                stringBuilder.Append(",\"Face\"");
                stringBuilder.Append(",\"FaceBlend\"");
                stringBuilder.Append(",\"Repeat\"");
                stringBuilder.Append(",\"Personality\"");
                stringBuilder.Append(",\"Arc\"");
                return stringBuilder.ToString();
            }

            public string ToCsvLine()
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append(string.Format("\"{0}\"", this.voiceName));
                stringBuilder.Append(string.Format(",\"{0}\"", this.voiceText));
                stringBuilder.Append(string.Format(",\"{0}\"", this.scriptFileName));
                stringBuilder.Append(string.Format(",\"{0}\"", this.lineNumber));
                stringBuilder.Append(string.Format(",\"{0}\"", this.label));
                stringBuilder.Append(string.Format(",\"{0}\"", this.subLabel));
                stringBuilder.Append(string.Format(",\"{0}\"", this.motionFile));
                stringBuilder.Append(string.Format(",\"{0}\"", this.motionLabel));
                stringBuilder.Append(string.Format(",\"{0}\"", this.face));
                stringBuilder.Append(string.Format(",\"{0}\"", this.faceBlend));
                stringBuilder.Append(string.Format(",\"{0}\"", this.repeat));
                stringBuilder.Append(string.Format(",\"{0}\"", this.personality));
                stringBuilder.Append(string.Format(",\"{0}\"", this.arcName));
                return stringBuilder.ToString();
            }

            public override bool Equals(object obj)
            {
                var other = obj as VoiceEntity;
                if (other == null)
                {
                    return false;
                }

                return voiceName == other.voiceName;
            }

            public override int GetHashCode()
            {
                return _voiceHash;
            }
        }

        public class ScriptCommand
        {
            public CommandType type;
            public string label = "";
            public string name = "";
            public string file = "";
            public string voice = "";
            public string text = "";
            public int lineNumber;
        }

        public enum CommandType
        {
            None,
            Talk,
            TalkRepeat,
            Motion,
            Face,
            FaceBlend,
            Label,
            SubLabel,
            Return,
            Text,
        }

        public static readonly Dictionary<string, CommandType> CommandMap = new Dictionary<string, CommandType>
        {
            { "@talk", CommandType.Talk },
            { "@talkrepeat", CommandType.TalkRepeat },
            { "@motionscript", CommandType.Motion },
            { "@face", CommandType.Face },
            { "@faceblend", CommandType.FaceBlend },
            { "@r_return", CommandType.Return },
            { "@return", CommandType.Return },
        };

        public enum ValueType
        {
            None,
            Name,
            Voice,
            File,
            Label,
        }

        public static readonly Dictionary<string, ValueType> ValueMap = new Dictionary<string, ValueType>
        {
            { "name", ValueType.Name },
            { "voice", ValueType.Voice },
            { "file", ValueType.File },
            { "label", ValueType.Label},
        };

        public static List<ScriptCommand> ParseScriptCommands(string[] lines)
        {
            List<ScriptCommand> commands = new List<ScriptCommand>();

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                try
                {
                    ScriptCommand command = ParseScriptCommand(line);
                    if (command != null)
                    {
                        command.lineNumber = i + 1;
                        commands.Add(command);
                    }
                }
                catch (Exception e)
                {
                    MTEUtils.LogError("Failed to parse. {0}: {1}", i, line);
                    throw e;
                }
            }

            return commands;
        }

        public static ScriptCommand ParseScriptCommand(string line)
        {
            line = line.Trim();
            if (line.Length == 0)
            {
                return null;
            }

            var first = line[0];
            if (first == ';')
            {
                return null;
            }

            ScriptCommand command = new ScriptCommand();

            if (first == '@')
            {
                var splited = line.Split(' ');

                CommandType type;
                if (!CommandMap.TryGetValue(splited[0].ToLower(), out type))
                {
                    return null;
                }

                command.type = type;

                for (var i = 1; i < splited.Length; ++i)
                {
                    var item = splited[i];
                    var values = item.Split('=');
                    if (values.Length != 2)
                    {
                        continue;
                    }

                    ValueType valueType;
                    if (!ValueMap.TryGetValue(values[0], out valueType))
                    {
                        continue;
                    }

                    switch (valueType)
                    {
                        case ValueType.Name:
                            command.name = values[1].Trim('"');
                            break;
                        case ValueType.Voice:
                            command.voice = values[1].Trim('"');
                            break;
                        case ValueType.File:
                            command.file = values[1].Trim('"');
                            break;
                        case ValueType.Label:
                            command.label = values[1].Trim('"');
                            break;
                    }
                }

                return command;
            }

            if (first == '*')
            {
                if (line.Length >= 2 && line[1] == 'L')
                {
                    command.type = CommandType.SubLabel;
                    command.label = line.Substring(1).Trim('|');
                }
                else
                {
                    command.type = CommandType.Label;
                    command.label = line.Substring(1);
                }

                return command;
            }

            command.type = CommandType.Text;
            command.text = line;

            return command;
        }

        private static List<VoiceEntity> ProcessScript(string scriptName)
        {
            var ret = new List<VoiceEntity>(64);

            var script = ReadScript(scriptName);
            if (script == null)
            {
                return ret;
            }

            string[] lines = script.Split(new string[]
            {
                "\r\n",
                "\r",
                "\n"
            }, StringSplitOptions.None);

            var entity = new VoiceEntity();
            Action reset = delegate ()
            {
                entity.label = "";
                entity.subLabel = "";
                entity.motionFile = "";
                entity.motionLabel = "";
                entity.face = "";
                entity.faceBlend = "";
                entity.repeat = false;
            };
            reset.Invoke();

            var commands = ParseScriptCommands(lines);

            foreach (var command in commands)
            {
                switch (command.type)
                {
                    case CommandType.Talk:
                        entity.voiceName = command.voice;
                        entity.voiceText = command.text;
                        break;
                    case CommandType.TalkRepeat:
                        entity.voiceName = command.voice;
                        entity.voiceText = command.text;
                        entity.repeat = true;
                        break;
                    case CommandType.Motion:
                        entity.motionFile = command.file;
                        entity.motionLabel = command.label;
                        break;
                    case CommandType.Face:
                        entity.face = command.name;
                        break;
                    case CommandType.FaceBlend:
                        entity.faceBlend = command.name;
                        break;
                    case CommandType.Label:
                        entity.label = command.label;
                        break;
                    case CommandType.SubLabel:
                        entity.subLabel = command.label;
                        break;
                    case CommandType.Return:
                        reset.Invoke();
                        break;
                    case CommandType.Text:
                        entity.voiceText = command.text;
                        break;
                }

                if (command.type == CommandType.Text &&
                    !string.IsNullOrEmpty(entity.voiceName))
                {
                    entity.scriptFileName = scriptName;
                    entity.lineNumber = command.lineNumber;
                    entity.arcName = "";
                    ret.Add(entity);

                    var newEntity = new VoiceEntity
                    {
                        label = entity.label,
                        subLabel = entity.subLabel,
                        motionFile = entity.motionFile,
                        motionLabel = entity.motionLabel,
                        face = entity.face,
                        faceBlend = entity.faceBlend,
                    };
                    entity = newEntity;
                }
            }

            return ret;
        }

        public static void OutputVoiceInfo(string[] scriptNames)
        {
            var voiceEntitiesList = new List<HashSet<VoiceEntity>>();

            foreach (Personality personality in Enum.GetValues(typeof(Personality)))
            {
                voiceEntitiesList.Add(new HashSet<VoiceEntity>());
            }

            var length = scriptNames.Length;
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            var totalElapsed = TimeSpan.Zero;

            for (var i = 0; i < length; ++i)
            {
                var scriptName = scriptNames[i];
                try
                {
                    if (i % 10 == 0 && i > 0)
                    {
                        totalElapsed += stopwatch.Elapsed;
                        var averageTime = totalElapsed.TotalSeconds / i;
                        var remainingTime = TimeSpan.FromSeconds(averageTime * (length - i));

                        MTEUtils.Log("スクリプト解析中... {0}/{1} - 経過時間: {2:hh\\:mm\\:ss} - 残り時間: {3:hh\\:mm\\:ss}",
                            i, length, totalElapsed, remainingTime);

                        stopwatch.Reset();
                        stopwatch.Start();
                    }

                    var entities = ProcessScript(scriptName);

                    foreach (var entity in entities)
                    {
                        var voiceEntities = voiceEntitiesList[(int) entity.personality];
                        voiceEntities.Add(entity);
                    }
                }
                catch (Exception e)
                {
                    MTEUtils.LogException(e);
                    MTEUtils.LogError("Failed to process script: {0}", scriptName);
                }
            }

            for (var i = 0; i < voiceEntitiesList.Count; ++i)
            {
                var personality = (Personality) i;
                var voiceEntities = voiceEntitiesList[i];
                var outputFilePath = PluginUtils.GetVoiceInfoCsvPath(personality);

                using (StreamWriter streamWriter = new StreamWriter(
                    File.Open(outputFilePath, FileMode.Create),
                    Encoding.Unicode,
                    1024 * 1024))
                {
                    streamWriter.WriteLine(VoiceEntity.GetCsvHeader());

                    foreach (var entity in voiceEntities)
                    {
                        streamWriter.WriteLine(entity.ToCsvLine());
                    }
                }
            }
        }
    }
}