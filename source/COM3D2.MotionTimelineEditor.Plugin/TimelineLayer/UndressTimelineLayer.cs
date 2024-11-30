using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    [TimelineLayerDesc("メイド脱衣", 15)]
    public class UndressTimelineLayer : TimelineLayerBase
    {
        public override string className => typeof(UndressTimelineLayer).Name;

        public override bool hasSlotNo => true;

        public override List<string> allBoneNames => DressUtils.DressSlotNames;

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

        protected override void ApplyMotion(MotionData motion, float t, bool indexUpdated)
        {
            var start = motion.start as TransformDataUndress;

            maidCache.SetSlotVisible(start.slotId, start.isVisible);
        }

        public override void UpdateFrame(FrameData frame)
        {
            var maidCache = this.maidCache;
            if (maidCache == null) return;

            foreach (var slotName in allBoneNames)
            {
                var slotId = DressUtils.GetDressSlotId(slotName);
                var trans = CreateTransformData<TransformDataUndress>(slotName);
                trans.isVisible = maidCache.IsSlotVisible(slotId);

                var bone = frame.CreateBone(trans);
                frame.UpdateBone(bone);
            }
        }

        public void OutputBones(
            List<BoneData> rows,
            string filePath)
        {
            var offsetTime = timeline.startOffsetTime;

            var builder = new StringBuilder();
            builder.Append("label,maidSlotNo,time,mask\r\n");

            Action<BoneData, bool> appendRow = (row, isFirst) =>
            {
                var time = row.frameNo * timeline.frameDuration;

                if (!isFirst)
                {
                    time += offsetTime;
                }

                var trans = row.transform as TransformDataUndress;

                bool mask = trans.isVisible;
                if (!DressUtils.IsShiftSlotId(trans.slotId))
                {
                    mask = !mask;
                }

                builder.Append(trans.slotId.ToString() + ",");
                builder.Append(trans.maidSlotNo + ",");
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
                var outputRows = new List<BoneData>(64);

                foreach (var layer in timelineManager.FindLayers<UndressTimelineLayer>(className))
                {
                    foreach (var rows in layer.timelineRowsMap.Values)
                    {
                        foreach (var row in rows)
                        {
                            var trans = row.transform as TransformDataUndress;
                            trans.maidSlotNo = layer.slotNo;
                            outputRows.Add(row);
                        }
                    }
                }

                var outputFileName = "undress.csv";
                var outputPath = timeline.GetDcmSongFilePath(outputFileName);
                OutputBones(outputRows, outputPath);

                songElement.Add(new XElement("changeUndress", outputFileName));
            }
            catch (Exception e)
            {
                PluginUtils.LogException(e);
                PluginUtils.LogError("脱衣モーションの出力に失敗しました");
            }
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

        public override SingleFrameType GetSingleFrameType(TransformType transformType)
        {
            return SingleFrameType.None;
        }

        public override TransformType GetTransformType(string name)
        {
            return TransformType.Undress;
        }
    }
}