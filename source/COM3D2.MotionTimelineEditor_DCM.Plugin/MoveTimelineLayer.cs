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
    using MovePlayData = MotionPlayData<MoveMotionData>;

    public class MoveTimeLineRow
    {
        public int frame;
        public Vector3 position;
        public Vector3 rotation;
        public int easing;
    }

    public class MoveMotionData : MotionDataBase
    {
        public MyTransform myTm;
        public int easing;
    }

    [LayerDisplayName("メイド移動")]
    public partial class MoveTimelineLayer : TimelineLayerBase
    {
        public override int priority
        {
            get
            {
                return 11;
            }
        }

        public override string className
        {
            get
            {
                return typeof(MoveTimelineLayer).Name;
            }
        }

        public override bool hasSlotNo
        {
            get
            {
                return true;
            }
        }

        public static string MoveBoneName = "move";
        public static string MoveDisplayName = "移動";

        private List<string> _allBoneNames = new List<string> { MoveBoneName };
        public override List<string> allBoneNames
        {
            get
            {
                return _allBoneNames;
            }
        }

        private List<MoveTimeLineRow> _timelineRows = new List<MoveTimeLineRow>();
        private MovePlayData _playData = new MovePlayData();

        private MoveTimelineLayer(int slotNo) : base(slotNo)
        {
        }

        public static MoveTimelineLayer Create(int slotNo)
        {
            return new MoveTimelineLayer(slotNo);
        }

        protected override void InitMenuItems()
        {
            allMenuItems.Clear();

            var menuItem = new BoneMenuItem(MoveBoneName, MoveDisplayName);
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

            _playData.Update(playingFrameNoFloat);

            var current = _playData.current;
            if (current != null)
            {
                ApplyMotion(current, maid.transform, _playData.lerpFrame);
            }

            //PluginUtils.LogDebug("ApplyPlayData: name={0} lerpFrame={1}, listIndex={2}",
            //    maid.name, _playData.lerpFrame, _playData.listIndex);
        }

        private void ApplyMotion(MoveMotionData motion, Transform transform, float lerpTime)
        {
            float easingTime = CalcEasingValue(lerpTime, motion.easing);
            transform.localPosition = Vector3.Lerp(motion.myTm.stPos, motion.myTm.edPos, easingTime);
            transform.localRotation = Quaternion.Euler(Vector3.Lerp(motion.myTm.stRot, motion.myTm.edRot, easingTime));
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

            var trans = CreateTransformData(MoveBoneName);
            trans.position = maid.transform.localPosition;
            trans.eulerAngles = maid.transform.localEulerAngles;

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
            var bone = frame.GetBone(MoveBoneName);
            if (bone == null)
            {
                return;
            }

            var trans = bone.transform;

            var row = new MoveTimeLineRow
            {
                frame = frame.frameNo,
                position = trans.position,
                rotation = trans.eulerAngles,
                easing = trans.easing
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

                var motion = new MoveMotionData
                {
                    stFrame = stFrame,
                    edFrame = edFrame,
                    myTm = new MyTransform
                    {
                        stPos = start.position,
                        stRot = start.rotation,
                        edPos = end.position,
                        edRot = end.rotation,
                    },
                    easing = end.easing,
                };

                _playData.motions.Add(motion);
            }

            _playData.Setup(timeline.singleFrameType);

            //PluginUtils.LogDebug("PlayData: name={0}, count={1}", maid.name, _playData.motions.Count);
        }

        public void SaveMotions(
            List<MoveMotionData> motions,
            string filePath)
        {
            var offsetTime = timeline.startOffsetTime;

            var builder = new StringBuilder();
            builder.Append("stTime,stPosX,stPosY,stPosZ,stRotX,stRotY,stRotZ," +
                            "edTime,edPosX,edPosY,edPosZ,edRotX,edRotY,edRotZ," +
                            "bezier1,bezier2,bezierType,easing" +
                            "\r\n");

            Action<MoveMotionData, bool> appendMotion = (motion, isFirst) =>
            {
                var stTime = motion.stFrame * timeline.frameDuration;
                var edTime = motion.edFrame * timeline.frameDuration;

                if (isFirst)
                {
                    stTime = 0;
                    edTime = offsetTime;
                }
                else
                {
                    stTime += offsetTime;
                    edTime += offsetTime;
                }

                builder.Append(stTime.ToString("0.000") + ",");
                builder.Append(motion.myTm.stPos.x.ToString("0.000") + ",");
                builder.Append(motion.myTm.stPos.y.ToString("0.000") + ",");
                builder.Append(motion.myTm.stPos.z.ToString("0.000") + ",");
                builder.Append(motion.myTm.stRot.x.ToString("0.000") + ",");
                builder.Append(motion.myTm.stRot.y.ToString("0.000") + ",");
                builder.Append(motion.myTm.stRot.z.ToString("0.000") + ",");
                builder.Append(edTime.ToString("0.000") + ",");
                builder.Append(motion.myTm.edPos.x.ToString("0.000") + ",");
                builder.Append(motion.myTm.edPos.y.ToString("0.000") + ",");
                builder.Append(motion.myTm.edPos.z.ToString("0.000") + ",");
                builder.Append(motion.myTm.edRot.x.ToString("0.000") + ",");
                builder.Append(motion.myTm.edRot.y.ToString("0.000") + ",");
                builder.Append(motion.myTm.edRot.z.ToString("0.000") + ",");
                builder.Append(0 + ","); // bezier1
                builder.Append(0 + ","); // bezier2
                builder.Append(0 + ","); // bezierType
                builder.Append(motion.easing);
                builder.Append("\r\n");
            };

            if (motions.Count > 0 && offsetTime > 0f)
            {
                appendMotion(motions.First(), true);
            }

            foreach (var motion in motions)
            {
                appendMotion(motion, false);
            }

            using (var streamWriter = new StreamWriter(filePath, false))
            {
                streamWriter.Write(builder.ToString());
            }
        }

        public override void OutputDCM(XElement songElement)
        {
            try
            {
                var outputFileName = string.Format("move_{0}.csv", slotNo);
                var outputPath = timeline.GetDcmSongFilePath(outputFileName);
                SaveMotions(_playData.motions, outputPath);

                var maidElement = GetMeidElement(songElement);
                maidElement.Add(new XElement("move", outputFileName));
            }
            catch (Exception e)
            {
                PluginUtils.LogException(e);
                PluginUtils.ShowDialog("メイド移動の出力に失敗しました");
            }
        }

        public override float CalcEasingValue(float t, int easing)
        {
            return TimelineMotionEasing.MotionEasing(t, (EasingType) easing);
        }

        public override void DrawWindow(GUIView view)
        {
            var maid = this.maid;
            if (maid == null)
            {
                return;
            }

            var transform = maid.transform;
            var initialPosition = Vector3.zero;
            var initialEulerAngles = Vector3.zero;
            var initialScale = Vector3.one;

            view.SetEnabled(!view.IsComboBoxFocused() && studioHack.isPoseEditing);

            DrawTransform(
                view,
                transform,
                TransformEditType.全て,
                DrawMaskPositonAndRotation,
                MoveBoneName,
                initialPosition,
                initialEulerAngles,
                initialScale);
        }

        public override ITransformData CreateTransformData(string name)
        {
            var transform = new TransformDataMove();
            transform.Initialize(name);
            return transform;
        }
    }
}