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
    using ShapeKeyPlayData = MotionPlayData<ShapeKeyMotionData>;

    public class ShapeKeyTimeLineRow
    {
        public int frame;
        public string name;
        public float weight;
        public int easing;
    }

    public class ShapeKeyMotionData : IMotionData
    {
        public int stFrame { get; set; }
        public int edFrame { get; set; }

        public string tag;
        public string slotName;
        public int maidSlotNo;
        public float stWeight;
        public float edWeight;
        public int easing;

        public ShapeKeyMotionData Clone()
        {
            return new ShapeKeyMotionData
            {
                stFrame = stFrame,
                edFrame = edFrame,

                tag = tag,
                slotName = slotName,
                maidSlotNo = maidSlotNo,
                stWeight = stWeight,
                edWeight = edWeight,
                easing = easing,
            };
        }
    }

    [LayerDisplayName("メイドシェイプ")]
    public partial class ShapeKeyTimelineLayer : TimelineLayerBase
    {
        public override int priority
        {
            get
            {
                return 13;
            }
        }

        public override string className
        {
            get
            {
                return typeof(ShapeKeyTimelineLayer).Name;
            }
        }

        public override bool hasSlotNo
        {
            get
            {
                return true;
            }
        }

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

        private Dictionary<string, List<ShapeKeyTimeLineRow>> _timelineRowsMap = new Dictionary<string, List<ShapeKeyTimeLineRow>>();
        private Dictionary<string, ShapeKeyPlayData> _playDataMap = new Dictionary<string, ShapeKeyPlayData>();
        private List<ShapeKeyMotionData> _dcmOutputMotions = new List<ShapeKeyMotionData>(128);

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

            if (!studioHack.isPoseEditing)
            {
                ApplyPlayData();
            }
        }

        private void ApplyPlayData()
        {
            if (maidCache == null)
            {
                return;
            }

            var playingFrameNoFloat = this.playingFrameNoFloat;

            foreach (var shapeKey in _playDataMap.Keys)
            {
                var playData = _playDataMap[shapeKey];

                playData.Update(playingFrameNoFloat);

                var current = playData.current;
                if (current != null)
                {
                    ApplyMotion(current, playData.lerpFrame);
                }

                //PluginUtils.LogDebug("ApplyPlayData: boneName={0} lerpFrame={1}, listIndex={2}", boneName, playData.lerpFrame, playData.listIndex);
            }

            maidCache.FixBlendValues(_playDataMap.Keys);
        }

        private void ApplyMotion(ShapeKeyMotionData motion, float lerpTime)
        {
            if (maidCache != null)
            {
                float easingTime = CalcEasingValue(lerpTime, motion.easing);
                var weight = Mathf.Lerp(motion.stWeight, motion.edWeight, easingTime);
                maidCache.SetBlendShapeValue(motion.tag, weight);
            }
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
                var trans = CreateTransformData(boneName);
                trans.easing = GetEasing(frame.frameNo, boneName);
                trans["weight"].value = maidCache.GetBlendShapeValue(boneName);

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

        private void BuildPlayData(bool forOutput)
        {
            PluginUtils.LogDebug("BuildPlayData");
            _playDataMap.Clear();

            bool warpFrameEnabled = forOutput || !studioHack.isPoseEditing;

            foreach (var pair in _timelineRowsMap)
            {
                var name = pair.Key;
                var rows = pair.Value;

                ShapeKeyPlayData playData;
                if (!_playDataMap.TryGetValue(name, out playData))
                {
                    playData = new ShapeKeyPlayData
                    {
                        motions = new List<ShapeKeyMotionData>(rows.Count),
                    };
                    _playDataMap[name] = playData;
                }

                playData.ResetIndex();
                playData.motions.Clear();

                bool isWarpFrame = false;

                for (var i = 0; i < rows.Count - 1; i++)
                {
                    var start = rows[i];
                    var end = rows[i + 1];

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

                    var motion = new ShapeKeyMotionData
                    {
                        tag = name,
                        stFrame = stFrame,
                        edFrame = edFrame,

                        stWeight = start.weight,
                        edWeight = end.weight,
                        easing = end.easing,
                    };

                    playData.motions.Add(motion);
                }
            }
        }

        protected override byte[] GetAnmBinaryInternal(bool forOutput, int startFrameNo, int endFrameNo)
        {
            _timelineRowsMap.Clear();

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
            foreach (var name in allBoneNames)
            {
                var bone = frame.GetBone(name);
                if (bone == null)
                {
                    continue;
                }

                List<ShapeKeyTimeLineRow> rows;
                if (!_timelineRowsMap.TryGetValue(name, out rows))
                {
                    rows = new List<ShapeKeyTimeLineRow>();
                    _timelineRowsMap[name] = rows;
                }

                var trans = bone.transform;

                var row = new ShapeKeyTimeLineRow
                {
                    frame = frame.frameNo,
                    name = bone.name,
                    weight = trans["weight"].value,
                    easing = trans.easing
                };

                rows.Add(row);
            }
        }

        public void SaveMotions(
            List<ShapeKeyMotionData> motions,
            string filePath)
        {
            var offsetTime = timeline.startOffsetTime;

            var builder = new StringBuilder();
            builder.Append("stTime,edTime,maidSlotNo,tag,slot,minSize,maxSize,outInterval,inInterval,outKeepTime,inKeepTime,outEasing,inEasing,isOneWay,isReverse" +
                            "\r\n");

            Action<ShapeKeyMotionData, bool> appendMotion = (motion, isFirst) =>
            {
                var stTime = motion.stFrame * timeline.frameDuration;
                var edTime = motion.edFrame * timeline.frameDuration;
                var dt = edTime - stTime;

                stTime += offsetTime;
                edTime += offsetTime;

                var isReverse = false;
                var minSize = (int) (motion.stWeight * 100);
                var maxSize = (int) (motion.edWeight * 100);

                if (minSize > maxSize)
                {
                    var tmp = minSize;
                    minSize = maxSize;
                    maxSize = tmp;
                    isReverse = true;
                }

                builder.Append(stTime.ToString("0.000") + ",");
                builder.Append(edTime.ToString("0.000") + ",");
                builder.Append(motion.maidSlotNo.ToString() + ",");
                builder.Append(motion.tag + ",");
                builder.Append(motion.slotName + ",");
                builder.Append(minSize.ToString() + ",");
                builder.Append(maxSize.ToString() + ",");
                builder.Append(dt.ToString("0.000") + ","); // outInterval
                builder.Append(dt.ToString("0.000") + ","); // inInterval
                builder.Append("0.000"+ ","); // outKeepTime
                builder.Append("0.000"+ ","); // inKeepTime
                builder.Append(motion.easing + ",");
                builder.Append(motion.easing + ",");
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

        public List<ShapeKeyMotionData> BuildDcmMotions()
        {
            _dcmOutputMotions.Clear();

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
                        newMotion.slotName = entity.morph.bodyskin.Category;
                        newMotion.maidSlotNo = slotNo;
                        _dcmOutputMotions.Add(newMotion);
                    }
                }
            }

            return _dcmOutputMotions;
        }

        public override void OutputDCM(XElement songElement)
        {
            if (slotNo != 0)
            {
                return;
            }

            try
            {
                var motions = new List<ShapeKeyMotionData>();
                foreach (var layer in timelineManager.FindLayers<ShapeKeyTimelineLayer>(className))
                {
                    motions.AddRange(layer.BuildDcmMotions());
                }

                var outputFileName = string.Format("shape_key.csv", slotNo);
                var outputPath = timeline.GetDcmSongFilePath(outputFileName);
                SaveMotions(motions, outputPath);

                songElement.Add(new XElement("changeShapeKey", outputFileName));
            }
            catch (Exception e)
            {
                PluginUtils.LogException(e);
                PluginUtils.ShowDialog("メイドシェイプキーの出力に失敗しました");
            }
        }

        public override float CalcEasingValue(float t, int easing)
        {
            return TimelineMotionEasing.MotionEasing(t, (EasingType) easing);
        }

        private ComboBoxValue<string> _slotNameComboBox = new ComboBoxValue<string>
        {
            getName = (slotName, index) =>
            {
                return slotName;
            }
        };
        private Maid _maid = null;
        private List<string> _slotNames = new List<string>();
        private Rect _contentRect = new Rect(0, 0, SubWindow.WINDOW_WIDTH, SubWindow.WINDOW_HEIGHT);
        private Vector2 _scrollPosition = Vector2.zero;
        private bool _isEditMode = false;

        public override void DrawWindow(GUIView view)
        {
            var maid = maidManager.maid;
            var maidSlotNo = maidManager.maidSlotNo;

            if (maid == null || maidSlotNo == -1 || !maidManager.IsValid())
            {
                view.DrawLabel("メイドを配置してください", -1, 20);
                return;
            }

            view.SetEnabled(view.guiEnabled && !_slotNameComboBox.focused);

            view.BeginLayout(GUIView.LayoutDirection.Horizontal);
            {
                var color = !_isEditMode ? Color.green : Color.white;
                if (view.DrawButton("追加", 80, 20, true, color))
                {
                    _isEditMode = false;
                }

                color = _isEditMode ? Color.green : Color.white;
                if (view.DrawButton("編集", 80, 20, true, color))
                {
                    _isEditMode = true;
                }
            }
            view.EndLayout();

            view.DrawHorizontalLine(Color.gray);

            if (_isEditMode)
            {
                DrawBlendShapesEdit(view);
            }
            else
            {
                DrawBlendShapesSelect(view);
            }

            DrawComboBox(view);
        }

        public void DrawBlendShapesSelect(GUIView view)
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

            view.BeginLayout(GUIView.LayoutDirection.Horizontal);
            {
                view.DrawLabel("対象スロット", 80, 20);

                _slotNameComboBox.items = _slotNames;
                view.DrawComboBoxButton(_slotNameComboBox, 140, 20, true);
            }
            view.EndLayout();

            var slotName = _slotNameComboBox.currentItem;

            if (string.IsNullOrEmpty(slotName) || !maid.body0.IsSlotNo(slotName))
            {
                view.DrawLabel("対象スロットがありません", -1, 20);
                return;
            }

            var morph = maid.body0.GetSlot(slotName).morph;
            var tags = morph.GetTags();
            tags.Sort();

            _contentRect.width = view.viewRect.width - 20;

            _scrollPosition = view.BeginScrollView(
                view.viewRect.width,
                view.viewRect.height - view.currentPos.y,
                _contentRect,
                _scrollPosition,
                false,
                true);

            foreach (string tag in tags)
            {
                var enable = timeline.HasMaidShapeKey(maidSlotNo, tag);

                var newEnable = view.DrawToggle(
                    tag,
                    enable,
                    _contentRect.width - 20,
                    20);

                if (newEnable != enable)
                {
                    if (newEnable)
                    {
                        timeline.AddMaidShapeKey(maidSlotNo, tag);
                    }
                    else
                    {
                        timeline.RemoveMaidShapeKey(maidSlotNo, tag);
                    }
                }
            }

            _contentRect.height = view.currentPos.y + 20;

            view.EndScrollView();
        }

        public void DrawBlendShapesEdit(GUIView view)
        {
            var maid = maidManager.maid;
            var maidSlotNo = maidManager.maidSlotNo;

            _contentRect.width = view.viewRect.width - 20;

            _scrollPosition = view.BeginScrollView(
                view.viewRect.width,
                view.viewRect.height - view.currentPos.y,
                _contentRect,
                _scrollPosition,
                false,
                true);

            view.SetEnabled(view.guiEnabled && studioHack.isPoseEditing);

            foreach (var menuItem in allMenuItems)
            {
                var shapeKey = menuItem.name;

                var blendShape = maidCache.GetBlendShape(shapeKey);
                if (blendShape == null)
                {
                    continue;
                }

                var fieldValue = GetFieldValue(menuItem.displayName);
                var weight = blendShape.weight;
                var updateTransform = false;

                updateTransform |= view.DrawSliderValue(fieldValue, 0f, 2f, 0.01f, 0f,
                    weight,
                    x => weight = x,
                    80);

                if (updateTransform)
                {
                    blendShape.weight = weight;
                    maidCache.FixBlendValues(new string[] { shapeKey });
                }
            }

            _contentRect.height = view.currentPos.y + 20;

            view.EndScrollView();
        }

        private void DrawComboBox(GUIView view)
        {
            view.SetEnabled(true);

            view.DrawComboBoxContent(
                _slotNameComboBox,
                130, 300,
                view.viewRect.width, view.viewRect.height,
                20);
        }

        public override ITransformData CreateTransformData(string name)
        {
            var transform = new TransformDataShapeKey();
            transform.Initialize(name);
            return transform;
        }
    }
}