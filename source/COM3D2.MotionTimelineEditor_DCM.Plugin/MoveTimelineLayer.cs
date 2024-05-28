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

    public class MoveMotionData : IMotionData
    {
        public int stFrame { get; set; }
        public int edFrame { get; set; }

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
            transform.position = Vector3.Lerp(motion.myTm.stPos, motion.myTm.edPos, easingTime);
            transform.rotation = Quaternion.Euler(Vector3.Lerp(motion.myTm.stRot, motion.myTm.edRot, easingTime));
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
            trans.position = maid.transform.position;
            trans.eulerAngles = maid.transform.eulerAngles;

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

            bool warpFrameEnabled = forOutput || !studioHack.isPoseEditing;

            var maid = this.maid;
            if (maid == null)
            {
                return;
            }

            _playData.ResetIndex();
            _playData.motions.Clear();

            bool isWarpFrame = false;

            for (var i = 0; i < _timelineRows.Count - 1; i++)
            {
                var start = _timelineRows[i];
                var end = _timelineRows[i + 1];

                var stFrame = start.frame;
                var edFrame = end.frame;

                if (!isWarpFrame && warpFrameEnabled && stFrame + 1 == edFrame)
                {
                    isWarpFrame = true;
                    continue;
                }

                if (isWarpFrame)
                {
                    stFrame--;
                    isWarpFrame = false;
                }

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

            PluginUtils.LogDebug("PlayData: name={0}, count={1}", maid.name, _playData.motions.Count);
        }

        public void SaveModelMotion(
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
                SaveModelMotion(_playData.motions, outputPath);

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

        private FloatFieldValue[] _fieldValues = FloatFieldValue.CreateArray(
            new string[] { "X", "Y", "Z", "RX", "RY", "RZ" }
        );

        public override void DrawWindow(GUIView view)
        {
            var maid = this.maid;
            if (maid == null)
            {
                return;
            }

            var position = maid.transform.position;
            var angle = maid.transform.eulerAngles;
            var updateTransform = false;

            GUI.enabled = studioHack.isPoseEditing;

            updateTransform |= view.DrawValue(_fieldValues[0], 0.01f, 0.1f, 0f,
                position.x,
                x => position.x = x,
                x => position.x += x);

            updateTransform |= view.DrawValue(_fieldValues[1], 0.01f, 0.1f, 0f,
                position.y,
                y => position.y = y,
                y => position.y += y);

            updateTransform |= view.DrawValue(_fieldValues[2], 0.01f, 0.1f, 0f,
                position.z,
                z => position.z = z,
                z => position.z += z);

            updateTransform |= view.DrawValue(_fieldValues[3], 1f, 10f, 0f,
                angle.x,
                x => angle.x = x,
                x => angle.x += x);

            updateTransform |= view.DrawValue(_fieldValues[4], 1f, 10f, 0f,
                angle.y,
                y => angle.y = y,
                y => angle.y += y);

            updateTransform |= view.DrawValue(_fieldValues[5], 1f, 10f, 0f,
                angle.z,
                z => angle.z = z,
                z => angle.z += z);

            GUI.enabled = true;

            if (updateTransform)
            {
                maid.transform.position = position;
                maid.transform.eulerAngles = angle;
            }
        }

        public override ITransformData CreateTransformData(string name)
        {
            var transform = new TransformDataMove();
            transform.Initialize(name);
            return transform;
        }
    }
}