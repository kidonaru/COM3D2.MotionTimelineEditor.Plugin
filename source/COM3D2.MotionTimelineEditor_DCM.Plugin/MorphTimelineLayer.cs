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
    using MorphPlayData = MotionPlayData<MorphMotionData>;

    public class MorphTimeLineRow
    {
        public int frame;
        public string name;
        public float morphValue;
    }

    public class MorphMotionData : IMotionData
    {
        public int stFrame { get; set; }
        public int edFrame { get; set; }

        public float startValue;
        public float endValue;
    }

    [LayerDisplayName("メイド表情")]
    public class MorphTimelineLayer : TimelineLayerBase
    {
        public override int priority
        {
            get
            {
                return 10;
            }
        }

        public override string className
        {
            get
            {
                return typeof(MorphTimelineLayer).Name;
            }
        }

        public override bool hasSlotNo
        {
            get
            {
                return true;
            }
        }

        public override List<string> allBoneNames
        {
            get
            {
                return MorphUtils.saveMorphNames;
            }
        }

        private MaidFaceManager _faceManager = new MaidFaceManager();
        private Dictionary<string, float> _applyMorphMap = new Dictionary<string, float>();

        private Dictionary<string, List<MorphTimeLineRow>> _timelineRowsMap = new Dictionary<string, List<MorphTimeLineRow>>();
        private Dictionary<string, MorphPlayData> _playDataMap = new Dictionary<string, MorphPlayData>();
        private List<MorphTimeLineRow> _dcmOutputRows = new List<MorphTimeLineRow>(128);

        private MorphTimelineLayer(int slotNo) : base(slotNo)
        {
        }

        public static MorphTimelineLayer Create(int slotNo)
        {
            return new MorphTimelineLayer(slotNo);
        }

        protected override void InitMenuItems()
        {
            var setMenuItemMap = new Dictionary<string, BoneSetMenuItem>(10);

            foreach (var pair in MorphUtils.MorphNameToSetNameMap)
            {
                var morphName = pair.Key;
                var morphSetName = pair.Value;

                BoneSetMenuItem setMenuItem;
                if (!setMenuItemMap.TryGetValue(morphSetName, out setMenuItem))
                {
                    var displaySetName = MorphUtils.GetMorphSetJpName(morphSetName);
                    setMenuItem = new BoneSetMenuItem(morphSetName, displaySetName);
                    setMenuItemMap[morphSetName] = setMenuItem;
                }

                var displayName = MorphUtils.GetMorphJpName(morphName);

                var menuItem = new BoneMenuItem(morphName, displayName);
                setMenuItem.AddChild(menuItem);
            }

            _allMenuItems = new List<IBoneMenuItem>(
                setMenuItemMap.Values.Cast<IBoneMenuItem>());
        }

        public override bool IsValidData()
        {
            errorMessage = "";

            var firstFrame = this.firstFrame;
            if (firstFrame == null || firstFrame.frameNo != 0)
            {
                errorMessage = "0フレーム目にキーフレームが必要です";
                return false;
            }

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
            var maid = this.maid;
            if (maid == null || maid.body0 == null || !maid.body0.isLoadedBody)
            {
                return;
            }

            var playingFrameNoFloat = this.playingFrameNoFloat;

            _applyMorphMap.Clear();

            foreach (var morphName in _playDataMap.Keys)
            {
                var playData = _playDataMap[morphName];

                playData.Update(playingFrameNoFloat);

                var current = playData.current;
                if (current != null)
                {
                    _applyMorphMap[morphName] = this.Lerp(
                        current.startValue,
                        current.endValue,
                        playData.lerpFrame,
                        morphName);
                }
            }

            _faceManager.SetMabatakiOff(maid);
            _faceManager.SetMorphValue(maid, _applyMorphMap);

            //PluginUtils.LogDebug("ApplyCamera: lerpFrame={0}, listIndex={1}", playData.lerpFrame, playData.listIndex);
        }

        private float Lerp(float startValue, float endValue, float lerpFrame, string morphName)
        {
            if (MyConst.FACE_OPTION_MORPH.ContainsKey(morphName) && lerpFrame < 0.99f)
            {
                lerpFrame = 0f;
            }
            return Mathf.Lerp(startValue, endValue, lerpFrame);
        }

        public override void UpdateFrameWithCurrentStat(FrameData frame)
        {
            var maid = this.maid;
            foreach (var name in allBoneNames)
            {
                var morphValue = _faceManager.GetMorphValue(maid, name);

                var trans = CreateTransformData(name);
                trans.values[0].value = morphValue;

                var bone = frame.CreateBone(trans);
                frame.UpdateBone(bone);
            }
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
            _timelineRowsMap.Clear();

            foreach (var keyFrame in keyFrames)
            {
                AppendTimeLineRow(keyFrame);
            }

            AppendTimeLineRow(_dummyLastFrame);

            BuildPlayData();
            return null;
        }

        private void AppendTimeLineRow(FrameData frame)
        {
            var isLastFrame = frame.frameNo == maxFrameNo;
            foreach (var name in firstFrame.boneNames)
            {
                List<MorphTimeLineRow> rows;
                if (!_timelineRowsMap.TryGetValue(name, out rows))
                {
                    rows = new List<MorphTimeLineRow>(16);
                    _timelineRowsMap[name] = rows;
                }

                var bone = frame.GetBone(name);
                if (bone == null)
                {
                    continue;
                }

                // 最後のフレームは2重に追加しない
                if (isLastFrame && rows.Count > 0 && rows.Last().frame == frame.frameNo)
                {
                    continue;
                }

                var row = new MorphTimeLineRow
                {
                    frame = frame.frameNo,
                    name = name,
                    morphValue = bone.transform["morphValue"].value
                };

                rows.Add(row);
            }
        }

        private void BuildPlayData()
        {
            _playDataMap.Clear();

            foreach (var pair in _timelineRowsMap)
            {
                var name = pair.Key;
                var rows = pair.Value;

                MorphPlayData playData;
                if (!_playDataMap.TryGetValue(name, out playData))
                {
                    playData = new MorphPlayData
                    {
                        motions = new List<MorphMotionData>(rows.Count)
                    };
                    _playDataMap[name] = playData;
                }

                foreach (var row in rows)
                {
                    var motion = new MorphMotionData
                    {
                        stFrame = row.frame,
                        edFrame = row.frame,
                        startValue = row.morphValue,
                        endValue = row.morphValue
                    };
                    playData.motions.Add(motion);

                    if (playData.motions.Count >= 2)
                    {
                        int prevIndex = playData.motions.Count() - 2;
                        var prevMotion = playData.motions[prevIndex];
                        prevMotion.edFrame = row.frame;
                        prevMotion.endValue = row.morphValue;
                    }
                }
            }
        }

        public void SaveMorphTimeLine(
            List<MorphTimeLineRow> rows,
            string filePath)
        {
            var offsetTime = timeline.startOffsetTime;
            var offsetFrame = (int) Mathf.Round(offsetTime * 30f);
            var frameFactor = 30f / timeline.frameRate;

            var builder = new StringBuilder();
            builder.Append("frame,morphName,morphValue\r\n");

            Action<MorphTimeLineRow, bool> appendRow = (row, isFirst) =>
            {
                int frame = (int) Mathf.Round(row.frame * frameFactor);
                if (!isFirst)
                {
                    frame += offsetFrame;
                }

                builder.Append(frame + ",");
                builder.Append(row.name + ",");
                builder.Append(row.morphValue.ToString("0.000"));
                builder.Append("\r\n");
            };

            if (offsetFrame > 0)
            {
                foreach (var name in firstFrame.boneNames)
                {
                    var row = new MorphTimeLineRow
                    {
                        frame = 0,
                        name = name,
                        morphValue = 0f
                    };
                    appendRow(row, true);
                }
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
                _dcmOutputRows.Clear();

                foreach (var rows in _timelineRowsMap.Values)
                {
                    _dcmOutputRows.AddRange(rows);
                }

                var outputFileName = string.Format("morph_{0}.csv", slotNo);
                var outputPath = timeline.GetDcmSongFilePath(outputFileName);
                SaveMorphTimeLine(_dcmOutputRows, outputPath);

                var maidElement = GetMeidElement(songElement);
                maidElement.Add(new XElement("morph", outputFileName));
            }
            catch (Exception e)
            {
                PluginUtils.LogException(e);
                PluginUtils.ShowDialog("表情モーションの出力に失敗しました");
            }
        }

        public override float CalcEasingValue(float t, int easing)
        {
            return TimelineMotionEasing.MotionEasing(t, (EasingType) easing);
        }

        private Rect _contentRect = new Rect(0, 0, SubWindow.WINDOW_WIDTH, SubWindow.WINDOW_HEIGHT);
        private Vector2 _scrollPosition = Vector2.zero;

        public override void DrawWindow(GUIView view)
        {
            var maid = this.maid;
            if (maid == null)
            {
                return;
            }

            _contentRect.width = view.viewRect.width - 20;

            _scrollPosition = view.BeginScrollView(
                view.viewRect.width,
                view.viewRect.height,
                _contentRect,
                _scrollPosition,
                false,
                true);

            view.SetEnabled(studioHack.isPoseEditing);

            Action<string> drawMorphSlider = morphName =>
            {
                var displayName = MorphUtils.GetMorphJpName(morphName);
                var morphValue = _faceManager.GetMorphValue(maid, morphName);
                var newMorphValue = morphValue;

                view.BeginLayout(GUIView.LayoutDirection.Horizontal);
                view.DrawLabel(displayName, 80, 20);

                newMorphValue = view.DrawFloatField(newMorphValue, 50, 20);

                newMorphValue = view.DrawSlider(newMorphValue, 0f, 1f, 80, 20);

                if (view.DrawButton("R", 20, 20))
                {
                    newMorphValue = 0;
                }

                if (!Mathf.Approximately(newMorphValue, morphValue))
                {
                    _applyMorphMap.Clear();
                    _applyMorphMap[morphName] = newMorphValue;
                    _faceManager.SetMorphValue(maid, _applyMorphMap);
                }

                view.EndLayout();
            };

            Action<string> drawMorphToggle = morphName =>
            {
                var displayName = MorphUtils.GetMorphJpName(morphName);
                var morphValue = _faceManager.GetMorphValue(maid, morphName);
                var isOn = morphValue >= 1f;

                var newIsOn = view.DrawToggle(displayName, isOn, 150, 20);

                if (newIsOn != isOn)
                {
                    _applyMorphMap.Clear();
                    _applyMorphMap[morphName] = newIsOn ? 1f : 0f;
                    _faceManager.SetMorphValue(maid, _applyMorphMap);
                }
            };

            view.DrawLabel("目", 80, 20);
            foreach (var morphName in MyConst.EYE_MORPH.Keys)
            {
                drawMorphSlider(morphName);
            }
            view.DrawHorizontalLine(Color.gray);

            view.DrawLabel("眉", 80, 20);
            foreach (var morphName in MyConst.MAYU_MORPH.Keys)
            {
                drawMorphSlider(morphName);
            }
            view.DrawHorizontalLine(Color.gray);

            view.DrawLabel("口", 80, 20);
            foreach (var morphName in MyConst.MOUTH_MORPH.Keys)
            {
                drawMorphSlider(morphName);
            }
            view.DrawHorizontalLine(Color.gray);

            view.DrawLabel("オプション", 80, 20);
            foreach (var morphName in MyConst.FACE_OPTION_MORPH.Keys)
            {
                drawMorphToggle(morphName);
            }

            _contentRect.height = view.currentPos.y + 20;

            view.EndScrollView();

            view.SetEnabled(true);
        }

        public override ITransformData CreateTransformData(string name)
        {
            var transform = new TransformDataMorph();
            transform.Initialize(name);
            return transform;
        }
    }
}