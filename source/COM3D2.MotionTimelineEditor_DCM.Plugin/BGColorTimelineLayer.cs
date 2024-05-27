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
    using BGColorPlayData = MotionPlayData<BGColorMotionData>;

    public class BGColorTimeLineRow
    {
        public int frame;
        public Color color;
    }

    public class BGColorMotionData : IMotionData
    {
        public int stFrame { get; set; }
        public int edFrame { get; set; }

        public Color color;
    }

    [LayerDisplayName("背景色")]
    public partial class BGColorTimelineLayer : TimelineLayerBase
    {
        public override int priority
        {
            get
            {
                return 32;
            }
        }

        public override string className
        {
            get
            {
                return typeof(BGColorTimelineLayer).Name;
            }
        }

        public static string BoneName = "BGColor";
        public static string BoneDisplayName = "背景色";

        private List<string> _allBoneNames = new List<string> { BoneName };
        public override List<string> allBoneNames
        {
            get
            {
                return _allBoneNames;
            }
        }

        private List<BGColorTimeLineRow> _timelineRows = new List<BGColorTimeLineRow>();
        private BGColorPlayData _playData = new BGColorPlayData();

        private static Camera camera
        {
            get
            {
                return GameMain.Instance.MainCamera.camera;
            }
        }

        private BGColorTimelineLayer(int slotNo) : base(slotNo)
        {
        }

        public static BGColorTimelineLayer Create(int slotNo)
        {
            return new BGColorTimelineLayer(0);
        }

        protected override void InitMenuItems()
        {
            allMenuItems.Clear();

            var menuItem = new BoneMenuItem(BoneName, BoneDisplayName);
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

            if (!studioHack.isPoseEditing)
            {
                ApplyPlayData();
            }
        }

        public override void LateUpdate()
        {
            base.LateUpdate();
        }

        private void ApplyPlayData()
        {
            var playingFrameNoFloat = this.playingFrameNoFloat;

            var indexUpdated = _playData.Update(playingFrameNoFloat);

            var current = _playData.current;
            if (current != null && indexUpdated)
            {
                ApplyMotion(current);
            }
        }

        private void ApplyMotion(BGColorMotionData motion)
        {
            if (motion == null)
            {
                return;
            }

            //PluginUtils.LogDebug("ApplyMotion: stFrame={0}, color={1}",
            //    motion.stFrame, motion.color);

            try
            {
                camera.backgroundColor = motion.color;
            }
            catch (Exception e)
            {
                PluginUtils.LogException(e);
            }
        }

        public override void UpdateFrameWithCurrentStat(FrameData frame)
        {
            var trans = CreateTransformData(BoneName);
            trans.color = camera.backgroundColor;

            var bone = frame.CreateBone(trans);
            frame.SetBone(bone);
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
                AppendTimelineRow(keyFrame);
            }

            BuildPlayData(forOutput);
            return null;
        }

        private void AppendTimelineRow(FrameData frame)
        {
            var bone = frame.GetBone(BoneName);
            if (bone == null)
            {
                return;
            }

            var trans = bone.transform;

            var row = new BGColorTimeLineRow
            {
                frame = frame.frameNo,
                color = trans.color,
            };

            _timelineRows.Add(row);
        }

        private void BuildPlayData(bool forOutput)
        {
            PluginUtils.LogDebug("BuildPlayData");

            _playData.ResetIndex();
            _playData.motions.Clear();

            for (var i = 0; i < _timelineRows.Count; i++)
            {
                var start = _timelineRows[i];
                var stFrame = start.frame;

                var motion = new BGColorMotionData
                {
                    stFrame = stFrame,
                    edFrame = stFrame,
                    color = start.color,
                };

                _playData.motions.Add(motion);
            }

            PluginUtils.LogDebug("PlayData: name={0}, count={1}", BoneName, _playData.motions.Count);
        }

        public void SaveBGTimeLine(
            List<BGColorTimeLineRow> rows,
            string filePath)
        {
            var offsetTime = timeline.startOffsetTime;

            var builder = new StringBuilder();
            builder.Append("bgName,colorR,colorG,colorB\r\n");

            Action<BGColorTimeLineRow, bool> appendRow = (row, isFirst) =>
            {
                var time = row.frame * timeline.frameDuration;

                if (isFirst)
                {
                    time = 0;
                }
                else
                {
                    time += offsetTime;
                }

                builder.Append(time.ToString("0.000") + ",");
                builder.Append(row.color.IntR() + ",");
                builder.Append(row.color.IntG() + ",");
                builder.Append(row.color.IntB());
                builder.Append("\r\n");
            };

            if (rows.Count > 0 && offsetTime > 0f)
            {
                appendRow(rows.First(), true);
            }
            
            foreach (var row in rows)
            {
                appendRow(row, false);
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
                var outputFileName = "bg_color.csv";
                var outputPath = timeline.GetDcmSongFilePath(outputFileName);
                SaveBGTimeLine(_timelineRows, outputPath);

                songElement.Add(new XElement("changeBgColor", outputFileName));
            }
            catch (Exception e)
            {
                PluginUtils.LogException(e);
                PluginUtils.ShowDialog("背景色チェンジの出力に失敗しました");
            }
        }

        public override float CalcEasingValue(float t, int easing)
        {
            return TimelineMotionEasing.MotionEasing(t, (EasingType) easing);
        }

        private FloatFieldValue[] _fieldValues = FloatFieldValue.CreateArray(
            new string[] { "R", "G", "B" }
        );

        private ColorFieldValue _colorFieldValue = new ColorFieldValue("Color", false);

        public override void DrawWindow(GUIView view)
        {
            if (camera == null)
            {
                return;
            }
            var color = camera.backgroundColor;
            var updateTransform = false;

            GUI.enabled = studioHack.isPoseEditing;

            updateTransform |= view.DrawSliderValue(_fieldValues[0], 0f, 1f, 0.01f, 0f,
                color.r,
                x => color.r = x);

            updateTransform |= view.DrawSliderValue(_fieldValues[1], 0f, 1f, 0.01f, 0f,
                color.g,
                y => color.g = y);

            updateTransform |= view.DrawSliderValue(_fieldValues[2], 0f, 1f, 0.01f, 0f,
                color.b,
                z => color.b = z);

            updateTransform |= view.DrawColor(
                _colorFieldValue,
                color,
                Color.black,
                c => color = c);

            view.DrawHorizontalLine(Color.gray);

            GUI.enabled = true;

            if (updateTransform)
            {
                camera.backgroundColor = color;
            }
        }

        public override ITransformData CreateTransformData(string name)
        {
            var transform = new TransformDataBGColor();
            transform.Initialize(name);
            return transform;
        }
    }
}