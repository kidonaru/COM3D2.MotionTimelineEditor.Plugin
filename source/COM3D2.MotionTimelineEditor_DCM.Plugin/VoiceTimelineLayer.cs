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
    using VoicePlayData = MotionPlayData<VoiceMotionData>;

    public class VoiceTimeLineRow
    {
        public int frame;
        public string oneTimeVoiceName;
        public string loopVoiceName;
    }

    public class VoiceMotionData : IMotionData
    {
        public int stFrame { get; set; }
        public int edFrame { get; set; }

        public string oneTimeVoiceName;
        public string loopVoiceName;
    }

    [LayerDisplayName("メイドボイス")]
    public partial class VoiceTimelineLayer : TimelineLayerBase
    {
        public override int priority
        {
            get
            {
                return 14;
            }
        }

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
            maidCache.oneTimeVoiceName = motion.oneTimeVoiceName;
            maidCache.loopVoiceName = motion.loopVoiceName;
            maidCache.PlayOneTimeVoice();
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
            trans.SetStrValue("oneTimeVoiceName", maidCache.oneTimeVoiceName);
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
                oneTimeVoiceName = trans.GetStrValue("oneTimeVoiceName"),
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
                    oneTimeVoiceName = start.oneTimeVoiceName,
                    loopVoiceName = start.loopVoiceName,
                };

                _playData.motions.Add(motion);
            }
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
                builder.Append(motion.oneTimeVoiceName + ",");
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

            {
                var oneTimeVoiceName = maidCache.oneTimeVoiceName;
                var newOneTimeVoiceName = view.DrawTextField("ワンタイムボイス", oneTimeVoiceName, -1, 20);
                if (oneTimeVoiceName != newOneTimeVoiceName)
                {
                    maidCache.oneTimeVoiceName = newOneTimeVoiceName;
                }
            }

            {
                var loopVoiceName = maidCache.loopVoiceName;
                var newLoopVoiceName = view.DrawTextField("ループボイス", loopVoiceName, -1, 20);
                if (loopVoiceName != newLoopVoiceName)
                {
                    maidCache.loopVoiceName = newLoopVoiceName;
                }
            }

            if (view.DrawButton("再生", 100, 20))
            {
                maidCache.PlayOneTimeVoice();
            }

            if (view.DrawButton("ボイス一覧出力", 100, 20))
            {
                PluginUtils.ShowConfirmDialog("ボイス一覧を出力しますか？\n※出力に数時間かかることがあります", () =>
                {
                    GameMain.Instance.SysDlg.Close();

                    string[] array = MyHelper.GetFileListAtExtension(".ks");
                    //string[] array = new string[] { "yotogi\\24220_【カラオケ】ハーレム後背位\\a1\\a1_sya_07620a.ks" };
                    ScriptLoader.OutputVoiceInfo(array);

                    PluginUtils.ShowDialog("ボイス一覧の出力が完了しました");
                }, null);
            }
        }

        public override ITransformData CreateTransformData(string name)
        {
            var transform = new TransformDataVoice();
            transform.Initialize(name);
            return transform;
        }
    }
}