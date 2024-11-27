using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    [TimelineLayerDesc("背景色", 32)]
    public partial class BGColorTimelineLayer : TimelineLayerBase
    {
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

        private List<BoneData> _timelineRows = new List<BoneData>();
        private MotionPlayData _playData = new MotionPlayData(32);

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
            if (indexUpdated)
            {
                ApplyMotion(_playData.current);
            }
        }

        private void ApplyMotion(MotionData motion)
        {
            if (motion == null)
            {
                return;
            }

            try
            {
                var start = motion.start;
                camera.backgroundColor = start.color;
            }
            catch (Exception e)
            {
                PluginUtils.LogException(e);
            }
        }

        public override void UpdateFrame(FrameData frame)
        {
            var trans = CreateTransformData<TransformDataBGColor>(BoneName);
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

            AppendTimelineRow(_dummyLastFrame);

            BuildPlayData(forOutput);
            return null;
        }

        private void AppendTimelineRow(FrameData frame)
        {
            var isLastFrame = frame.frameNo == maxFrameNo;
            var bone = frame.GetBone(BoneName);
            _timelineRows.AppendBone(bone, isLastFrame);
        }

        private void BuildPlayData(bool forOutput)
        {
            BuildPlayDataFromBones(
                _timelineRows,
                _playData,
                SingleFrameType.None);
        }

        public void SaveBGTimeLine(
            List<BoneData> rows,
            string filePath)
        {
            var offsetTime = timeline.startOffsetTime;

            var builder = new StringBuilder();
            builder.Append("bgName,colorR,colorG,colorB\r\n");

            Action<BoneData, bool> appendRow = (row, isFirst) =>
            {
                var time = row.frameNo * timeline.frameDuration;

                if (isFirst)
                {
                    time = 0;
                }
                else
                {
                    time += offsetTime;
                }

                var transform = row.transform;

                builder.Append(time.ToString("0.000") + ",");
                builder.Append(transform.color.IntR() + ",");
                builder.Append(transform.color.IntG() + ",");
                builder.Append(transform.color.IntB());
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

        private ColorFieldCache _colorFieldValue = new ColorFieldCache("Color", false);

        public override void DrawWindow(GUIView view)
        {
            if (camera == null)
            {
                return;
            }
            var color = camera.backgroundColor;
            var updateTransform = false;

            view.SetEnabled(!view.IsComboBoxFocused() && studioHack.isPoseEditing);

            updateTransform |= view.DrawColor(
                _colorFieldValue,
                color,
                Color.black,
                c => color = c);

            view.DrawHorizontalLine(Color.gray);

            if (updateTransform)
            {
                camera.backgroundColor = color;
            }
        }

        public override TransformType GetTransformType(string name)
        {
            return TransformType.BGColor;
        }
    }
}