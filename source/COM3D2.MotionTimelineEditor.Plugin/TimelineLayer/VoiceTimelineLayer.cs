using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    [TimelineLayerDesc("メイドボイス", 14)]
    public partial class VoiceTimelineLayer : TimelineLayerBase
    {
        public override string className => typeof(VoiceTimelineLayer).Name;

        public override bool hasSlotNo => true;

        public static string VoiceBoneName = "Voice";
        public static string VoiceDisplayName = "ボイス";

        private List<string> _allBoneNames = new List<string> { VoiceBoneName };
        public override List<string> allBoneNames => _allBoneNames;

        private VoiceTimelineLayer(int slotNo) : base(slotNo)
        {
        }

        public static VoiceTimelineLayer Create(int slotNo)
        {
            return new VoiceTimelineLayer(slotNo);
        }

        protected override void InitMenuItems()
        {
            allMenuItems.Clear();

            var menuItem = new BoneMenuItem(VoiceBoneName, VoiceDisplayName);
            allMenuItems.Add(menuItem);
        }

        public override bool IsValidData()
        {
            errorMessage = "";
            return true;
        }

        public override void Update()
        {
            base.Update();
        }

        public override void LateUpdate()
        {
            base.LateUpdate();

            if (!studioHack.isPoseEditing)
            {
                ApplyPlayData();
            }
        }

        protected override void ApplyMotion(MotionData motion, float t, bool indexUpdated)
        {
            var start = motion.start as TransformDataVoice;

            maidCache.oneShotVoiceName = start.voiceName;
            maidCache.oneShotVoiceStartTime = start.startTime;
            maidCache.oneShotVoiceLength = start.length;
            maidCache.voiceFadeTime = start.fadeTime;
            maidCache.voicePitch = start.pitch;
            maidCache.loopVoiceName = start.loopVoiceName;
            maidCache.PlayOneShotVoice();
        }

        public override void UpdateFrame(FrameData frame)
        {
            var maid = this.maid;
            if (maid == null)
            {
                PluginUtils.LogError("メイドが配置されていません");
                return;
            }

            var trans = CreateTransformData<TransformDataVoice>(VoiceBoneName);
            trans.voiceName = maidCache.oneShotVoiceName;
            trans.startTime = maidCache.oneShotVoiceStartTime;
            trans.length = maidCache.oneShotVoiceLength;
            trans.fadeTime = maidCache.voiceFadeTime;
            trans.pitch = maidCache.voicePitch;
            trans.loopVoiceName = maidCache.loopVoiceName;

            var bone = frame.CreateBone(trans);
            frame.UpdateBone(bone);
        }

        public List<MotionData> GetVoiceMotionData()
        {
            return _playDataMap[VoiceBoneName].motions;
        }

        public void OutputMotions(
            List<MotionData> motions,
            string filePath)
        {
            var offsetTime = timeline.startOffsetTime;

            var builder = new StringBuilder();
            builder.Append("sTime,eTime,oneTimeVoiceName,loopVoiceName,maidSlotNo" +
                            "\r\n");

            Action<MotionData> appendMotion = motion =>
            {
                var stTime = motion.stFrame * timeline.frameDuration;
                var edTime = motion.edFrame * timeline.frameDuration;

                stTime += offsetTime;
                edTime += offsetTime;

                var start = motion.start as TransformDataVoice;

                builder.Append(stTime.ToString("0.000") + ",");
                builder.Append(edTime.ToString("0.000") + ",");
                builder.Append(start.voiceName + ",");
                builder.Append(start.loopVoiceName + ",");
                builder.Append(slotNo.ToString());
                builder.Append("\r\n");
            };

            foreach (var motion in motions)
            {
                appendMotion(motion);
            }

            using (var streamWriter = new StreamWriter(filePath, false))
            {
                streamWriter.Write(builder.ToString());
            }
        }

        public override void OutputDCM(XElement songElement)
        {
            if (slotNo != 0)
            {
                return;
            }

            try
            {
                var motions = new List<MotionData>(64);
                foreach (var layer in timelineManager.FindLayers<VoiceTimelineLayer>(className))
                {
                    motions.AddRange(layer.GetVoiceMotionData());
                }

                var outputFileName = "maid_voice.csv";
                var outputPath = timeline.GetDcmSongFilePath(outputFileName);
                OutputMotions(motions, outputPath);

                songElement.Add(new XElement("changeMaidVoice", outputFileName));
            }
            catch (Exception e)
            {
                PluginUtils.LogException(e);
                PluginUtils.LogError("メイドボイスの出力に失敗しました");
            }
        }

        public override void DrawWindow(GUIView view)
        {
            if (maidCache == null)
            {
                view.DrawLabel("メイドを配置してください", -1, 20);
                return;
            }

            view.SetEnabled(!view.IsComboBoxFocused() && studioHack.isPoseEditing);

            view.DrawSliderValue(new GUIView.SliderOption
            {
                label = "開始",
                labelWidth = 30,
                min = 0f,
                max = config.voiceMaxLength,
                step = 0.01f,
                defaultValue = 0f,
                value = maidCache.oneShotVoiceStartTime,
                onChanged = value => maidCache.oneShotVoiceStartTime = value,
            });

            view.DrawSliderValue(new GUIView.SliderOption
            {
                label = "長さ",
                labelWidth = 30,
                min = 0f,
                max = config.voiceMaxLength,
                step = 0.01f,
                defaultValue = 0f,
                value = maidCache.oneShotVoiceLength,
                onChanged = value => maidCache.oneShotVoiceLength = value,
            });

            view.DrawSliderValue(new GUIView.SliderOption
            {
                label = "Fade",
                labelWidth = 30,
                min = 0f,
                max = config.voiceMaxLength,
                step = 0.01f,
                defaultValue = 0.1f,
                value = maidCache.voiceFadeTime,
                onChanged = value => maidCache.voiceFadeTime = value,
            });

            view.DrawSliderValue(new GUIView.SliderOption
            {
                label = "音程",
                labelWidth = 30,
                min = 0f,
                max = 2f,
                step = 0.01f,
                defaultValue = 1f,
                value = maidCache.voicePitch,
                onChanged = value => maidCache.voicePitch = value,
            });

            view.DrawTextField(new GUIView.TextFieldOption
            {
                label = "ボイス名",
                labelWidth = 75,
                value = maidCache.oneShotVoiceName,
                onChanged = value => maidCache.oneShotVoiceName = value,
            });

            view.DrawTextField(new GUIView.TextFieldOption
            {
                label = "ループボイス",
                labelWidth = 75,
                value = maidCache.loopVoiceName,
                onChanged = value => maidCache.loopVoiceName = value,
            });

            if (view.DrawButton("再生", 100, 20))
            {
                maidCache.PlayOneShotVoice();
            }

            view.SetEnabled(!view.IsComboBoxFocused());

            view.DrawHorizontalLine(Color.gray);

            view.BeginHorizontal();
            {
                if (view.DrawButton("ボイス一覧出力", 120, 20))
                {
                    PluginUtils.ShowConfirmDialog("ボイス一覧を出力しますか？\n※出力に数時間かかることがあります", () =>
                    {
                        GameMain.Instance.SysDlg.Close();

                        PluginUtils.Log("スクリプト一覧取得中...");
                        string[] array = GetFileListAtExtension(".ks");
                        //string[] array = new string[] { "yotogi\\24220_【カラオケ】ハーレム後背位\\a1\\a1_sya_07620a.ks" };
                        ScriptLoader.OutputVoiceInfo(array);

                        PluginUtils.ShowDialog("ボイス一覧の出力が完了しました");
                    }, null);
                }

                var enabled = Directory.Exists(PluginUtils.PluginConfigDirPath);
                if (view.DrawButton("出力先を開く", 120, 20, enabled))
                {
                    PluginUtils.OpenDirectory(PluginUtils.PluginConfigDirPath);
                }
            }
            view.EndLayout();
        }

        public static string[] GetFileListAtExtension(string extention)
        {
            return GameUty.FileSystem.GetFileListAtExtension(extention).Concat(GameUty.FileSystemOld.GetFileListAtExtension(extention)).ToArray();
        }

        public override SingleFrameType GetSingleFrameType(TransformType transformType)
        {
            return SingleFrameType.None;
        }

        public override TransformType GetTransformType(string name)
        {
            return TransformType.Voice;
        }
    }
}