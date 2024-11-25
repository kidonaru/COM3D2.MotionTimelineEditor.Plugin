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
    using UndressPlayData = PlayDataBase<UndressMotionData>;

    public class UndressTimeLineRow
    {
        public int frame;
        public DressSlotID slotId;
        public int maidSlotNo;
        public bool isVisible;
    }

    public class UndressMotionData : MotionDataBase
    {
        public DressSlotID slotId;
        public bool isVisible;
    }

    [TimelineLayerDesc("メイド脱衣", 15)]
    public class UndressTimelineLayer : TimelineLayerBase
    {
        public override string className
        {
            get
            {
                return typeof(UndressTimelineLayer).Name;
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
                return DressUtils.DressSlotNames;
            }
        }

        public Dictionary<string, List<UndressTimeLineRow>> timelineRowsMap
        {
            get
            {
                return _timelineRowsMap;
            }
        }

        private Dictionary<string, List<UndressTimeLineRow>> _timelineRowsMap = new Dictionary<string, List<UndressTimeLineRow>>();
        private Dictionary<string, UndressPlayData> _playDataMap = new Dictionary<string, UndressPlayData>();
        private List<UndressTimeLineRow> _dcmOutputRows = new List<UndressTimeLineRow>(128);

        private UndressTimelineLayer(int slotNo) : base(slotNo)
        {
        }

        public static UndressTimelineLayer Create(int slotNo)
        {
            return new UndressTimelineLayer(slotNo);
        }

        protected override void InitMenuItems()
        {
            _allMenuItems.Clear();

            Action<string, string, List<DressSlotID>> addCategory = (setName, setJpName, slotIds) =>
            {
                var setMenuItem = new BoneSetMenuItem(setName, setJpName);
                _allMenuItems.Add(setMenuItem);

                foreach (var slotId in slotIds)
                {
                    var displayName = DressUtils.GetDressSlotJpName(slotId);
                    var menuItem = new BoneMenuItem(slotId.ToString(), displayName);
                    setMenuItem.AddChild(menuItem);
                }
            };

            addCategory("clothing", "衣装", DressUtils.ClothingSlotIds);
            addCategory("headwear", "頭部衣装", DressUtils.HeadwearSlotIds);
            addCategory("accessory", "アクセ", DressUtils.AccessorySlotIds);
            addCategory("mekure", "めくれ", DressUtils.MekureSlotIds);
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

            foreach (var slotName in _playDataMap.Keys)
            {
                var playData = _playDataMap[slotName];

                var updated = playData.Update(playingFrameNoFloat);
                if (updated)
                {
                    var current = playData.current;
                    ApplyMotion(current);
                }
            }

            //PluginUtils.LogDebug("ApplyCamera: lerpFrame={0}, listIndex={1}", playData.lerpFrame, playData.listIndex);
        }

        private void ApplyMotion(UndressMotionData motion)
        {
            var maidCache = this.maidCache;
            if (maidCache == null) return;

            maidCache.SetSlotVisible(motion.slotId, motion.isVisible);
        }

        public override void UpdateFrame(FrameData frame)
        {
            var maidCache = this.maidCache;
            if (maidCache == null) return;

            foreach (var slotName in allBoneNames)
            {
                var slotId = DressUtils.GetDressSlotId(slotName);
                var trans = CreateTransformData(slotName);
                trans["isVisible"].boolValue = maidCache.IsSlotVisible(slotId);

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
            foreach (var slotName in firstFrame.boneNames)
            {
                List<UndressTimeLineRow> rows;
                if (!_timelineRowsMap.TryGetValue(slotName, out rows))
                {
                    rows = new List<UndressTimeLineRow>(16);
                    _timelineRowsMap[slotName] = rows;
                }

                var bone = frame.GetBone(slotName);
                if (bone == null)
                {
                    continue;
                }

                // 最後のフレームは2重に追加しない
                if (isLastFrame && rows.Count > 0 && rows.Last().frame == frame.frameNo)
                {
                    continue;
                }

                var slotId = DressUtils.GetDressSlotId(slotName);

                var row = new UndressTimeLineRow
                {
                    frame = frame.frameNo,
                    slotId = slotId,
                    maidSlotNo = slotNo,
                    isVisible = bone.transform["isVisible"].boolValue,
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

                UndressPlayData playData;
                if (!_playDataMap.TryGetValue(name, out playData))
                {
                    playData = new UndressPlayData
                    {
                        motions = new List<UndressMotionData>(rows.Count)
                    };
                    _playDataMap[name] = playData;
                }

                foreach (var row in rows)
                {
                    var motion = new UndressMotionData
                    {
                        stFrame = row.frame,
                        edFrame = row.frame,
                        slotId = row.slotId,
                        isVisible = row.isVisible,
                    };
                    playData.motions.Add(motion);

                    if (playData.motions.Count >= 2)
                    {
                        int prevIndex = playData.motions.Count() - 2;
                        var prevMotion = playData.motions[prevIndex];
                        prevMotion.edFrame = row.frame;
                    }
                }
            }

            foreach (var pair in _playDataMap)
            {
                var playData = pair.Value;
                playData.Setup(SingleFrameType.None);
            }
        }

        public void SaveUndressTimeLine(
            List<UndressTimeLineRow> rows,
            string filePath)
        {
            var offsetTime = timeline.startOffsetTime;

            var builder = new StringBuilder();
            builder.Append("label,maidSlotNo,time,mask\r\n");

            Action<UndressTimeLineRow, bool> appendRow = (row, isFirst) =>
            {
                var time = row.frame * timeline.frameDuration;

                if (!isFirst)
                {
                    time += offsetTime;
                }

                bool mask = row.isVisible;
                if (!DressUtils.IsShiftSlotId(row.slotId))
                {
                    mask = !mask;
                }

                builder.Append(row.slotId.ToString() + ",");
                builder.Append(row.maidSlotNo + ",");
                builder.Append(time.ToString("0.000") + ",");
                builder.Append(mask ? "1" : "0");
                builder.Append("\r\n");
            };

            for (var i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                appendRow(row, i == 0);
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
                _dcmOutputRows.Clear();

                foreach (var layer in timelineManager.FindLayers<UndressTimelineLayer>(className))
                {
                    foreach (var rows in layer.timelineRowsMap.Values)
                    {
                        _dcmOutputRows.AddRange(rows);
                    }
                }

                var outputFileName = "undress.csv";
                var outputPath = timeline.GetDcmSongFilePath(outputFileName);
                SaveUndressTimeLine(_dcmOutputRows, outputPath);

                songElement.Add(new XElement("changeUndress", outputFileName));
            }
            catch (Exception e)
            {
                PluginUtils.LogException(e);
                PluginUtils.ShowDialog("脱衣モーションの出力に失敗しました");
            }
        }

        public override float CalcEasingValue(float t, int easing)
        {
            return TimelineMotionEasing.MotionEasing(t, (EasingType) easing);
        }

        private enum TabType
        {
            衣装,
            頭部,
            アクセ,
            めくれ,
        }

        private TabType _tabType = TabType.衣装;

        public override void DrawWindow(GUIView view)
        {
            if (maidCache == null)
            {
                return;
            }

            _tabType = view.DrawTabs(_tabType, 50, 20);

            view.DrawHorizontalLine(Color.gray);

            view.SetEnabled(!view.IsComboBoxFocused() && studioHack.isPoseEditing);

            DrawUndress(view);
        }

        private void DrawUndress(GUIView view)
        {
            view.DrawLabel(_tabType.ToString(), 80, 20);

            switch (_tabType)
            {
                case TabType.衣装:
                    DrawUndressByCategory(view, DressUtils.ClothingSlotIds);
                    break;
                case TabType.頭部:
                    DrawUndressByCategory(view, DressUtils.HeadwearSlotIds);
                    break;
                case TabType.アクセ:
                    DrawUndressByCategory(view, DressUtils.AccessorySlotIds);
                    break;
                case TabType.めくれ:
                    DrawUndressByCategory(view, DressUtils.MekureSlotIds);
                    break;
            }
        }

        private void DrawUndressByCategory(GUIView view, List<DressSlotID> slotIds)
        {
            for (var i = 0; i < slotIds.Count; i++)
            {
                if (i % 2 == 0)
                {
                    view.BeginHorizontal();
                }

                var slotId = slotIds[i];
                //var isLoaded = maidCache.IsSlotLoaded(slotId);
                var isVisible = maidCache.IsSlotVisible(slotId);

                DrawUndressToggle(view, slotId, isVisible);

                if (i % 2 == 1)
                {
                    view.EndLayout();
                }
            }
            view.EndLayout();
        }

        private void DrawUndressToggle(GUIView view, DressSlotID slotId, bool isVisible)
        {
            var displayName = DressUtils.GetDressSlotJpName(slotId);

            view.DrawToggle(displayName, isVisible, 100, 20, newIsVisible =>
            {
                maidCache.SetSlotVisible(slotId, newIsVisible);
            });
        }

        public override ITransformData CreateTransformData(string name)
        {
            var transform = new TransformDataUndress();
            transform.Initialize(name);
            return transform;
        }
    }
}