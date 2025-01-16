using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    [TimelineLayerDesc("メイドシェイプ", 13)]
    public partial class ShapeKeyTimelineLayer : TimelineLayerBase
    {
        public override Type layerType => typeof(ShapeKeyTimelineLayer);
        public override string layerName => nameof(ShapeKeyTimelineLayer);

        public override bool hasSlotNo => true;

        private List<string> _allBoneNames = null;
        public override List<string> allBoneNames
        {
            get
            {
                if (_allBoneNames == null)
                {
                    var shapeKeys = timeline.GetMaidShapeKeys(slotNo);
                    _allBoneNames = new List<string>(shapeKeys);
                }

                return _allBoneNames;
            }
        }

        private ShapeKeyTimelineLayer(int slotNo) : base(slotNo)
        {
        }

        public static ShapeKeyTimelineLayer Create(int slotNo)
        {
            return new ShapeKeyTimelineLayer(slotNo);
        }

        protected override void InitMenuItems()
        {
            allMenuItems.Clear();

            _allBoneNames = null;
            foreach (var boneName in allBoneNames)
            {
                var menuItem = new BoneMenuItem(boneName, boneName);
                allMenuItems.Add(menuItem);
            }
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

        protected override void ApplyPlayData()
        {
            if (maidCache == null)
            {
                return;
            }

            base.ApplyPlayData();

            maidCache.FixBlendValues(_playDataMap.Keys);
        }

        protected override void ApplyMotion(MotionData motion, float t, bool indexUpdated)
        {
            var start = motion.start as TransformDataShapeKey;
            var end = motion.end as TransformDataShapeKey;

            float easingTime = CalcEasingValue(t, start.easing);
            var weight = Mathf.Lerp(start.weight, end.weight, easingTime);
            maidCache.SetBlendShapeValue(motion.name, weight);
        }

        public override void OnShapeKeyAdded(string shapeKey)
        {
            InitMenuItems();

            var boneNames = new List<string> { shapeKey };
            AddFirstBones(boneNames);
            ApplyCurrentFrame(true);
        }

        public override void OnShapeKeyRemoved(string shapeKey)
        {
            InitMenuItems();

            var boneNames = new List<string> { shapeKey };
            RemoveAllBones(boneNames);
            ApplyCurrentFrame(true);
        }

        public override void UpdateFrame(FrameData frame)
        {
            foreach (var boneName in allBoneNames)
            {
                var trans = CreateTransformData<TransformDataShapeKey>(boneName);
                trans.easing = GetEasing(frame.frameNo, boneName);
                trans.weight = maidCache.GetBlendShapeValue(boneName);

                var bone = frame.CreateBone(trans);
                frame.UpdateBone(bone);
            }
        }

        public void OutputMotions(
            List<MotionData> motions,
            string filePath)
        {
            var offsetTime = timeline.startOffsetTime;

            var builder = new StringBuilder();
            builder.Append("stTime,edTime,maidSlotNo,tag,slot,minSize,maxSize,outInterval,inInterval,outKeepTime,inKeepTime,outEasing,inEasing,isOneWay,isReverse" +
                            "\r\n");

            Action<MotionData, bool> appendMotion = (motion, isFirst) =>
            {
                var stTime = motion.stFrame * timeline.frameDuration;
                var edTime = motion.edFrame * timeline.frameDuration;
                var dt = edTime - stTime;

                stTime += offsetTime;
                edTime += offsetTime;

                var start = motion.start as TransformDataShapeKey;
                var end = motion.end as TransformDataShapeKey;

                var isReverse = false;
                var minSize = (int) (start.weight * 100);
                var maxSize = (int) (end.weight * 100);

                if (minSize > maxSize)
                {
                    var tmp = minSize;
                    minSize = maxSize;
                    maxSize = tmp;
                    isReverse = true;
                }

                builder.Append(stTime.ToString("0.000") + ",");
                builder.Append(edTime.ToString("0.000") + ",");
                builder.Append(start.maidSlotNo.ToString() + ",");
                builder.Append(start.name + ",");
                builder.Append(start.slotName + ",");
                builder.Append(minSize.ToString() + ",");
                builder.Append(maxSize.ToString() + ",");
                builder.Append(dt.ToString("0.000") + ","); // outInterval
                builder.Append(dt.ToString("0.000") + ","); // inInterval
                builder.Append("0.000"+ ","); // outKeepTime
                builder.Append("0.000"+ ","); // inKeepTime
                builder.Append(start.easing + ",");
                builder.Append(start.easing + ",");
                builder.Append("1" + ","); // isOneWay
                builder.Append(isReverse ? "1" : "0");
                builder.Append("\r\n");
            };

            foreach (var motion in motions)
            {
                appendMotion(motion, false);
            }

            using (var streamWriter = new StreamWriter(filePath, false))
            {
                streamWriter.Write(builder.ToString());
            }
        }

        public List<MotionData> BuildDcmMotions()
        {
            var outputMotions = new List<MotionData>(64);

            foreach (var pair in _playDataMap)
            {
                var shapeKey = pair.Key;
                var playData = pair.Value;

                var blendShape = maidCache.GetBlendShape(shapeKey);
                if (blendShape == null)
                {
                    continue;
                }

                foreach (var motion in playData.motions)
                {
                    foreach (var entity in blendShape.entities)
                    {
                        var newMotion = motion.Clone();
                        var start = newMotion.start as TransformDataShapeKey;
                        start.slotName = entity.morph.bodyskin.Category;
                        start.maidSlotNo = slotNo;
                        outputMotions.Add(newMotion);
                    }
                }
            }

            return outputMotions;
        }

        public override void OutputDCM(XElement songElement)
        {
            if (slotNo != 0)
            {
                return;
            }

            try
            {
                var motions = new List<MotionData>(64);
                foreach (ShapeKeyTimelineLayer layer in timelineManager.FindLayers(typeof(ShapeKeyTimelineLayer)))
                {
                    motions.AddRange(layer.BuildDcmMotions());
                }

                var outputFileName = string.Format("shape_key.csv", slotNo);
                var outputPath = timeline.GetDcmSongFilePath(outputFileName);
                OutputMotions(motions, outputPath);

                songElement.Add(new XElement("changeShapeKey", outputFileName));
            }
            catch (Exception e)
            {
                MTEUtils.LogException(e);
                MTEUtils.LogError("メイドシェイプキーの出力に失敗しました");
            }
        }

        private GUIComboBox<string> _slotNameComboBox = new GUIComboBox<string>
        {
            getName = (slotName, index) => slotName,
        };
        private Maid _maid = null;
        private List<string> _slotNames = new List<string>();

        private enum TabType
        {
            追加,
            操作,
        }

        private TabType _tabType = TabType.追加;

        public override void DrawWindow(GUIView view)
        {
            var maid = maidManager.maid;
            var maidSlotNo = maidManager.maidSlotNo;

            if (maid == null || maidSlotNo == -1 || !maidManager.IsValid())
            {
                view.DrawLabel("メイドを配置してください", -1, 20);
                return;
            }

            view.SetEnabled(!view.IsComboBoxFocused());

            _tabType = view.DrawTabs(_tabType, 50, 20);

            switch (_tabType)
            {
                case TabType.追加:
                    DrawBlendShapesAdd(view);
                    break;
                case TabType.操作:
                    DrawBlendShapesEdit(view);
                    break;
            }

            view.DrawComboBox();
        }

        public void DrawBlendShapesAdd(GUIView view)
        {
            var maid = maidManager.maid;
            var maidSlotNo = maidManager.maidSlotNo;

            if (maid != _maid)
            {
                _slotNames = maid.body0.goSlot
                    .Where(slot => slot.morph != null && slot.morph.hash.Count > 0)
                    .Select(slot => slot.Category)
                    .ToList();
            }

            _slotNameComboBox.items = _slotNames;
            _slotNameComboBox.DrawButton("対象スロット", view);

            var slotName = _slotNameComboBox.currentItem;

            if (string.IsNullOrEmpty(slotName) || !maid.body0.IsSlotNo(slotName))
            {
                view.DrawLabel("対象スロットがありません", -1, 20);
                return;
            }

            var morph = maid.body0.GetSlot(slotName).morph;
            var tags = morph.GetTags();
            tags.Sort();

            view.DrawHorizontalLine(Color.gray);

            view.AddSpace(5);

            view.BeginScrollView();

            foreach (string tag in tags)
            {
                var enable = timeline.HasMaidShapeKey(maidSlotNo, tag);

                view.DrawToggle(tag, enable, -1, 20, newValue =>
                {
                    if (newValue)
                    {
                        timeline.AddMaidShapeKey(maidSlotNo, tag);
                    }
                    else
                    {
                        timeline.RemoveMaidShapeKey(maidSlotNo, tag);
                    }
                });
            }

            view.SetEnabled(!view.IsComboBoxFocused());
            view.EndScrollView();
        }

        public void DrawBlendShapesEdit(GUIView view)
        {
            var maid = maidManager.maid;
            var maidSlotNo = maidManager.maidSlotNo;

            view.DrawHorizontalLine(Color.gray);

            view.AddSpace(5);

            view.BeginScrollView();

            view.SetEnabled(!view.IsComboBoxFocused() && studioHackManager.isPoseEditing);

            foreach (var menuItem in allMenuItems)
            {
                var shapeKey = menuItem.name;

                var blendShape = maidCache.GetBlendShape(shapeKey);
                if (blendShape == null)
                {
                    continue;
                }

                var weight = blendShape.weight;
                var updateTransform = false;

                view.DrawLabel(menuItem.displayName, -1, 20);

                updateTransform |= view.DrawSliderValue(
                    new GUIView.SliderOption
                    {
                        min = 0f,
                        max = 2f,
                        step = 0.01f,
                        defaultValue = 0f,
                        value = weight,
                        onChanged = x => weight = x,
                    });

                if (updateTransform)
                {
                    blendShape.weight = weight;
                    maidCache.FixBlendValues(new string[] { shapeKey });
                }
            }

            view.SetEnabled(!view.IsComboBoxFocused());
            view.EndScrollView();
        }

        public override TransformType GetTransformType(string name)
        {
            return TransformType.ShapeKey;
        }
    }
}