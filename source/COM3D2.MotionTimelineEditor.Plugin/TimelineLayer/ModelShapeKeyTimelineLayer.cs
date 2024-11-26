using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    [TimelineLayerDesc("モデルシェイプ", 23)]
    public partial class ModelShapeKeyTimelineLayer : ModelTimelineLayerBase
    {
        public override string className
        {
            get
            {
                return typeof(ModelShapeKeyTimelineLayer).Name;
            }
        }

        public override List<string> allBoneNames
        {
            get
            {
                return modelManager.blendShapeNames;
            }
        }

        private Dictionary<string, List<BoneData>> _timelineRowsMap = new Dictionary<string, List<BoneData>>();
        private Dictionary<string, MotionPlayData> _playDataMap = new Dictionary<string, MotionPlayData>();

        private ModelShapeKeyTimelineLayer(int slotNo) : base(slotNo)
        {
        }

        public static ModelShapeKeyTimelineLayer Create(int slotNo)
        {
            return new ModelShapeKeyTimelineLayer(0);
        }

        protected override void InitMenuItems()
        {
            allMenuItems.Clear();

            foreach (var model in modelManager.models)
            {
                if (model.blendShapes.Count == 0)
                {
                    continue;
                }

                var setMenuItem = new BoneSetMenuItem(model.name, model.displayName);
                allMenuItems.Add(setMenuItem);

                foreach (var blendShape in model.blendShapes)
                {
                    var menuItem = new BoneMenuItem(blendShape.name, blendShape.shapeKeyName);
                    setMenuItem.AddChild(menuItem);
                }
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
            var playingFrameNoFloat = this.playingFrameNoFloat;

            foreach (var shapeName in _playDataMap.Keys)
            {
                var playData = _playDataMap[shapeName];

                var blendShape = modelManager.GetBlendShape(shapeName);
                if (blendShape == null)
                {
                    continue;
                }

                playData.Update(playingFrameNoFloat);

                var current = playData.current;
                if (current != null)
                {
                    ApplyMotion(current, blendShape, playData.lerpFrame);
                }

                //PluginUtils.LogDebug("ApplyPlayData: boneName={0} lerpFrame={1}, listIndex={2}", boneName, playData.lerpFrame, playData.listIndex);
            }

            var models = modelManager.models;
            foreach (var model in models)
            {
                model.FixBlendValues();
            }
        }

        private void ApplyMotion(MotionData motion, ModelBlendShape blendShape, float lerpTime)
        {
            var start = motion.start;
            var end = motion.end;

            var stTrans = start.transform;
            var edTrans = end.transform;

            float easingTime = CalcEasingValue(lerpTime, stTrans.easing);
            var weight = Mathf.Lerp(stTrans["weight"].value, edTrans["weight"].value, easingTime);
            blendShape.weight = weight;
        }

        public override void OnModelAdded(StudioModelStat model)
        {
            InitMenuItems();

            var boneNames = model.blendShapes.Select(x => x.name).ToList();
            AddFirstBones(boneNames);
            ApplyCurrentFrame(true);
        }

        public override void OnModelRemoved(StudioModelStat model)
        {
            InitMenuItems();

            var boneNames = model.blendShapes.Select(x => x.name).ToList();
            RemoveAllBones(boneNames);
            ApplyCurrentFrame(true);
        }

        public override void OnCopyModel(StudioModelStat sourceModel, StudioModelStat newModel)
        {
            var blendShapes = sourceModel.blendShapes;
            var newModelName = newModel.name;
            foreach (var keyFrame in keyFrames)
            {
                foreach (var blendShape in blendShapes)
                {
                    var sourceBone = keyFrame.GetBone(blendShape.name);
                    if (sourceBone == null)
                    {
                        continue;
                    }

                    var baseName = blendShape.shapeKeyName;
                    var newBoneName = string.Format("{0}/{1}", newModelName, baseName);

                    var newBone = keyFrame.GetOrCreateBone(newBoneName);
                    newBone.transform.FromTransformData(sourceBone.transform);
                }
            }
        }

        public override void UpdateFrame(FrameData frame)
        {
            foreach (var blendShape in modelManager.blendShapeMap.Values)
            {
                var boneName = blendShape.name;

                var trans = CreateTransformData(boneName);
                trans.easing = GetEasing(frame.frameNo, boneName);
                trans["weight"].value = blendShape.weight;

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
            foreach (var playData in _playDataMap.Values)
            {
                playData.ResetIndex();
                playData.motions.Clear();
            }

            foreach (var pair in _timelineRowsMap)
            {
                var name = pair.Key;
                var rows = pair.Value;

                if (rows.Count == 0)
                {
                    continue;
                }

                var blendShape = modelManager.GetBlendShape(name);
                if (blendShape == null)
                {
                    continue;
                }

                MotionPlayData playData;
                if (!_playDataMap.TryGetValue(name, out playData))
                {
                    playData = new MotionPlayData(rows.Count);
                    _playDataMap[name] = playData;
                }

                for (var i = 0; i < rows.Count - 1; i++)
                {
                    var start = rows[i];
                    var end = rows[i + 1];
                    playData.motions.Add(new MotionData(start, end));
                }

                playData.Setup(timeline.singleFrameType);
            }
        }

        protected override byte[] GetAnmBinaryInternal(bool forOutput, int startFrameNo, int endFrameNo)
        {
            foreach (var rows in _timelineRowsMap.Values)
            {
                rows.Clear();
            }

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

                List<BoneData> rows;
                if (!_timelineRowsMap.TryGetValue(name, out rows))
                {
                    rows = new List<BoneData>();
                    _timelineRowsMap[name] = rows;
                }

                rows.Add(bone);
            }
        }

        public override void OutputDCM(XElement songElement)
        {
            PluginUtils.LogWarning("モデルシェイプキーはDCMに対応していません");
        }

        private GUIComboBox<StudioModelStat> _modelComboBox = new GUIComboBox<StudioModelStat>
        {
            getName = (model, index) => model.displayName,
            buttonSize = new Vector2(200, 20),
            contentSize = new Vector2(200, 300),
        };

        private enum TabType
        {
            操作,
            管理,
        }

        private TabType _tabType = TabType.操作;

        public override void DrawWindow(GUIView view)
        {
            _tabType = view.DrawTabs(_tabType, 50, 20);

            switch (_tabType)
            {
                case TabType.操作:
                    DrawBlendShapes(view);
                    break;
                case TabType.管理:
                    DrawModelManage(view);
                    break;
            }

            view.DrawComboBox();
        }

        public void DrawBlendShapes(GUIView view)
        {
            _modelComboBox.items = modelManager.models;

            if (modelManager.models.Count == 0)
            {
                view.DrawLabel("モデルが存在しません", 200, 20);
                return;
            }

            view.SetEnabled(!view.IsComboBoxFocused());

            view.DrawLabel("モデル選択", 200, 20);
            _modelComboBox.DrawButton(view);

            var model = _modelComboBox.currentItem;
            if (model == null || model.transform == null)
            {
                view.DrawLabel("モデルが見つかりません", 200, 20);
                return;
            }

            var blendShapes = model.blendShapes;
            if (blendShapes.Count == 0)
            {
                view.DrawLabel("シェイプキーが存在しません", 200, 20);
                return;
            }

            view.DrawHorizontalLine(Color.gray);

            view.AddSpace(5);

            view.BeginScrollView();
            {
                view.SetEnabled(!view.IsComboBoxFocused() && studioHack.isPoseEditing);

                for (var i = 0; i < blendShapes.Count; i++)
                {
                    var blendShape = blendShapes[i];
                    var weight = blendShape.weight;
                    var updateTransform = false;

                    view.DrawLabel(blendShape.shapeKeyName, -1, 20);

                    updateTransform |= view.DrawSliderValue(
                        new GUIView.SliderOption
                        {
                            min = -1f,
                            max = 2f,
                            step = 0.01f,
                            defaultValue = 0f,
                            value = weight,
                            onChanged = x => weight = x,
                        });

                    if (updateTransform)
                    {
                        blendShape.weight = weight;
                        model.FixBlendValues();
                    }
                }
            }
            view.SetEnabled(!view.IsComboBoxFocused());
            view.EndScrollView();
        }

        public override ITransformData CreateTransformData(string name)
        {
            var transform = new TransformDataShapeKey();
            transform.Initialize(name);
            return transform;
        }
    }
}