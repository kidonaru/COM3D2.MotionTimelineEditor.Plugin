using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    [TimelineLayerDesc("メイド移動", 11)]
    public partial class MoveTimelineLayer : TimelineLayerBase
    {
        public override Type layerType => typeof(MoveTimelineLayer);
        public override string layerName => nameof(MoveTimelineLayer);

        public override bool hasSlotNo => true;

        public static string MoveBoneName = "move";
        public static string MoveDisplayName = "移動";

        private List<string> _allBoneNames = new List<string> { MoveBoneName };
        public override List<string> allBoneNames => _allBoneNames;

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

            if (!studioHackManager.isPoseEditing)
            {
                ApplyPlayData();
            }
        }

        protected override void ApplyMotion(MotionData motion, float t, bool indexUpdated)
        {
            if (indexUpdated)
            {
                ApplyMotionInit(motion, t);
            }

            if (timeline.isTangentMove)
            {
                ApplyMotionUpdateTangent(motion, t);
            }
            else
            {
                ApplyMotionUpdateEasing(motion, t);
            }
        }

        private void ApplyMotionInit(MotionData motion, float t)
        {
            var transform = maid.transform;
            var start = motion.start;

            transform.localPosition = start.position;
            transform.localRotation = Quaternion.Euler(start.eulerAngles);
            transform.localScale = start.scale;
        }

        private void ApplyMotionUpdateEasing(MotionData motion, float t)
        {
            var transform = maid.transform;

            var start = motion.start;
            var end = motion.end;

            float easingTime = CalcEasingValue(t, motion.easing);

            if (start.position != end.position)
            {
                transform.localPosition = Vector3.Lerp(start.position, end.position, easingTime);
            }

            if (start.rotation != end.rotation)
            {
                transform.localRotation = Quaternion.Lerp(start.rotation, end.rotation, easingTime);
            }

            if (start.scale != end.scale)
            {
                transform.localScale = Vector3.Lerp(start.scale, end.scale, easingTime);
            }
        }

        private void ApplyMotionUpdateTangent(MotionData motion, float t)
        {
            var transform = maid.transform;

            var start = motion.start;
            var end = motion.end;

            var t0 = motion.stFrame * timeline.frameDuration;
            var t1 = motion.edFrame * timeline.frameDuration;

            transform.localPosition = PluginUtils.HermiteVector3(
                t0,
                t1,
                start.positionValues,
                end.positionValues,
                t);

            transform.localRotation = PluginUtils.HermiteQuaternion(
                t0,
                t1,
                start.rotationValues,
                end.rotationValues,
                t);

            transform.localScale = PluginUtils.HermiteVector3(
                t0,
                t1,
                start.scaleValues,
                end.scaleValues,
                t);
        }

        public override void OnPoseEditEnd()
        {
            base.OnPoseEditEnd();
            ApplyPlayData();
        }

        public override void UpdateFrame(FrameData frame, bool initialEdit, bool force)
        {
            var maid = this.maid;
            if (maid == null)
            {
                MTEUtils.LogError("メイドが配置されていません");
                return;
            }

            var trans = CreateTransformData<TransformDataMove>(MoveBoneName);
            trans.position = maid.transform.localPosition;
            trans.rotation = maid.transform.localRotation;
            trans.scale = maid.transform.localScale;
            trans.easing = GetEasing(frame.frameNo, MoveBoneName);

            var bone = frame.CreateBone(trans);
            frame.UpdateBone(bone);
        }

        public void OutputMotions(
            List<MotionData> motions,
            string filePath)
        {
            var offsetTime = timeline.startOffsetTime;

            var builder = new StringBuilder();
            builder.Append("stTime,stPosX,stPosY,stPosZ,stRotX,stRotY,stRotZ," +
                            "edTime,edPosX,edPosY,edPosZ,edRotX,edRotY,edRotZ," +
                            "bezier1,bezier2,bezierType,easing" +
                            "\r\n");

            Action<MotionData, bool> appendMotion = (motion, isFirst) =>
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

                var start = motion.start;
                var end = motion.end;

                builder.Append(stTime.ToString("0.000") + ",");
                builder.Append(start.position.x.ToString("0.000") + ",");
                builder.Append(start.position.y.ToString("0.000") + ",");
                builder.Append(start.position.z.ToString("0.000") + ",");
                builder.Append(start.eulerAngles.x.ToString("0.000") + ",");
                builder.Append(start.eulerAngles.y.ToString("0.000") + ",");
                builder.Append(start.eulerAngles.z.ToString("0.000") + ",");
                builder.Append(edTime.ToString("0.000") + ",");
                builder.Append(end.position.x.ToString("0.000") + ",");
                builder.Append(end.position.y.ToString("0.000") + ",");
                builder.Append(end.position.z.ToString("0.000") + ",");
                builder.Append(end.eulerAngles.x.ToString("0.000") + ",");
                builder.Append(end.eulerAngles.y.ToString("0.000") + ",");
                builder.Append(end.eulerAngles.z.ToString("0.000") + ",");
                builder.Append(0 + ","); // bezier1
                builder.Append(0 + ","); // bezier2
                builder.Append(0 + ","); // bezierType
                builder.Append(start.easing);
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
                OutputMotions(_playDataMap[MoveBoneName].motions, outputPath);

                var maidElement = GetMeidElement(songElement);
                maidElement.Add(new XElement("move", outputFileName));
            }
            catch (Exception e)
            {
                MTEUtils.LogException(e);
                MTEUtils.LogError("メイド移動の出力に失敗しました");
            }
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

            view.SetEnabled(!view.IsComboBoxFocused() && studioHackManager.isPoseEditing);

            DrawTransform(
                view,
                transform,
                TransformEditType.全て,
                DrawMaskAll,
                MoveBoneName,
                initialPosition,
                initialEulerAngles,
                initialScale);
        }

        public override TransformType GetTransformType(string name)
        {
            return TransformType.Move;
        }
    }
}