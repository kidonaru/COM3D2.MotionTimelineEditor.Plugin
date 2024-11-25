using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using COM3D2.DanceCameraMotion.Plugin;
using COM3D2.MotionTimelineEditor.Plugin;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor_DCM.Plugin
{
    using VoicePlayData = PlayDataBase<VoiceMotionData>;

    public class VoiceTimeLineRow
    {
        public int frame;
        public string voiceName;
        public float startTime;
        public float length;
        public float fadeTime;
        public float pitch;
        public string loopVoiceName;
    }

    public class VoiceMotionData : MotionDataBase
    {
        public string voiceName;
        public float startTime;
        public float length;
        public float fadeTime;
        public float pitch;
        public string loopVoiceName;
    }

    [TimelineLayerDesc("メイドボイス", 14)]
    public partial class VoiceTimelineLayer : TimelineLayerBase
    {
        public override string className
        {
            get
            {
                return typeof(VoiceTimelineLayer).Name;
            }
        }

        public override bool hasSlotNo
        {
            get
            {
                return true;
            }
        }

        public static string VoiceBoneName = "Voice";
        public static string VoiceDisplayName = "ボイス";

        private List<string> _allBoneNames = new List<string> { VoiceBoneName };
        public override List<string> allBoneNames
        {
            get
            {
                return _allBoneNames;
            }
        }

        private List<VoiceTimeLineRow> _timelineRows = new List<VoiceTimeLineRow>();
        private VoicePlayData _playData = new VoicePlayData();

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

        private void ApplyPlayData()
        {
            var playingFrameNoFloat = this.playingFrameNoFloat;

            var maid = this.maid;
            if (maid == null)
            {
                return;
            }

            var updated = _playData.Update(playingFrameNoFloat);

            if (updated)
            {
                ApplyMotion(_playData.current);
            }
        }

        private void ApplyMotion(VoiceMotionData motion)
        {
            maidCache.oneShotVoiceName = motion.voiceName;
            maidCache.oneShotVoiceStartTime = motion.startTime;
            maidCache.oneShotVoiceLength = motion.length;
            maidCache.voiceFadeTime = motion.fadeTime;
            maidCache.voicePitch = motion.pitch;
            maidCache.loopVoiceName = motion.loopVoiceName;
            maidCache.PlayOneShotVoice();
        }

        public override void OnEndPoseEdit()
        {
            base.OnEndPoseEdit();
            ApplyPlayData();
        }

        public override void UpdateFrame(FrameData frame)
        {
            var maid = this.maid;
            if (maid == null)
            {
                PluginUtils.LogError("メイドが配置されていません");
                return;
            }

            var trans = CreateTransformData(VoiceBoneName);
            trans.SetStrValue("voiceName", maidCache.oneShotVoiceName);
            trans["startTime"].value = maidCache.oneShotVoiceStartTime;
            trans["length"].value = maidCache.oneShotVoiceLength;
            trans["fadeTime"].value = maidCache.voiceFadeTime;
            trans["pitch"].value = maidCache.voicePitch;
            trans.SetStrValue("loopVoiceName", maidCache.loopVoiceName);

            var bone = frame.CreateBone(trans);
            frame.UpdateBone(bone);
        }

        public override void ApplyAnm(long id, byte[] anmData)
        {
            ApplyPlayData();
        }

        public override void ApplyCurrentFrame(bool motionUpdate)
        {
            if (anmId != TimelineAnmId || motionUpdate)
            {
                CreateAndApplyAnm();
            }
            else
            {
                ApplyPlayData();
            }
        }

        public override void OutputAnm()
        {
            // do nothing
        }

        protected override byte[] GetAnmBinaryInternal(bool forOutput, int startFrameNo, int endFrameNo)
        {
            _timelineRows.Clear();

            foreach (var keyFrame in keyFrames)
            {
                AppendTimeLineRow(keyFrame);
            }

            AppendTimeLineRow(_dummyLastFrame);

            BuildPlayData(forOutput);

            return null;
        }

        private void AppendTimeLineRow(FrameData frame)
        {
            var bone = frame.GetBone(VoiceBoneName);
            if (bone == null)
            {
                return;
            }

            var trans = bone.transform;

            var row = new VoiceTimeLineRow
            {
                frame = frame.frameNo,
                voiceName = trans.GetStrValue("voiceName"),
                startTime = trans["startTime"].value,
                length = trans["length"].value,
                fadeTime = trans["fadeTime"].value,
                pitch = trans["pitch"].value,
                loopVoiceName = trans.GetStrValue("loopVoiceName"),
            };

            _timelineRows.Add(row);
        }

        private void BuildPlayData(bool forOutput)
        {
            PluginUtils.LogDebug("BuildPlayData");

            var maid = this.maid;
            if (maid == null)
            {
                return;
            }

            _playData.ResetIndex();
            _playData.motions.Clear();

            for (var i = 0; i < _timelineRows.Count - 1; i++)
            {
                var start = _timelineRows[i];
                var end = _timelineRows[i + 1];

                var stFrame = start.frame;
                var edFrame = end.frame;

                var motion = new VoiceMotionData
                {
                    stFrame = stFrame,
                    edFrame = edFrame,
                    voiceName = start.voiceName,
                    startTime = start.startTime,
                    length = start.length,
                    fadeTime = start.fadeTime,
                    pitch = start.pitch,
                    loopVoiceName = start.loopVoiceName,
                };

                _playData.motions.Add(motion);
            }

            _playData.Setup(SingleFrameType.None);
        }

        public List<VoiceMotionData> GetVoiceMotionData()
        {
            return _playData.motions;
        }

        public void SaveMotions(
            List<VoiceMotionData> motions,
            string filePath)
        {
            var offsetTime = timeline.startOffsetTime;

            var builder = new StringBuilder();
            builder.Append("sTime,eTime,oneTimeVoiceName,loopVoiceName,maidSlotNo" +
                            "\r\n");

            Action<VoiceMotionData> appendMotion = motion =>
            {
                var stTime = motion.stFrame * timeline.frameDuration;
                var edTime = motion.edFrame * timeline.frameDuration;

                stTime += offsetTime;
                edTime += offsetTime;

                builder.Append(stTime.ToString("0.000") + ",");
                builder.Append(edTime.ToString("0.000") + ",");
                builder.Append(motion.voiceName + ",");
                builder.Append(motion.loopVoiceName + ",");
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
                var motions = new List<VoiceMotionData>();
                foreach (var layer in timelineManager.FindLayers<VoiceTimelineLayer>(className))
                {
                    motions.AddRange(layer.GetVoiceMotionData());
                }

                var outputFileName = "maid_voice.csv";
                var outputPath = timeline.GetDcmSongFilePath(outputFileName);
                SaveMotions(motions, outputPath);

                songElement.Add(new XElement("changeMaidVoice", outputFileName));
            }
            catch (Exception e)
            {
                PluginUtils.LogException(e);
                PluginUtils.ShowDialog("メイドボイスの出力に失敗しました");
            }
        }

        public override float CalcEasingValue(float t, int easing)
        {
            return TimelineMotionEasing.MotionEasing(t, (EasingType) easing);
        }

        public override void DrawWindow(GUIView view)
        {
            if (maidCache == null)
            {
                view.DrawLabel("メイドを配置してください", -1, 20);
                return;
            }

            view.SetEnabled(!view.IsComboBoxFocused() && studioHack.isPoseEditing);

            view.DrawSliderValue(
                new GUIView.SliderOption
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

            view.DrawSliderValue(
                new GUIView.SliderOption
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

            view.DrawSliderValue(
                new GUIView.SliderOption
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

            view.DrawSliderValue(
                new GUIView.SliderOption
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

            view.DrawTextField(
                new GUIView.TextFieldOption
                {
                    label = "ボイス名",
                    labelWidth = 75,
                    value = maidCache.oneShotVoiceName,
                    onChanged = value => maidCache.oneShotVoiceName = value,
                }
            );

            view.DrawTextField(
                new GUIView.TextFieldOption
                {
                    label = "ループボイス",
                    labelWidth = 75,
                    value = maidCache.loopVoiceName,
                    onChanged = value => maidCache.loopVoiceName = value,
                }
            );

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
                        string[] array = MyHelper.GetFileListAtExtension(".ks");
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

        public override ITransformData CreateTransformData(string name)
        {
            var transform = new TransformDataVoice();
            transform.Initialize(name);
            return transform;
        }
    }
}