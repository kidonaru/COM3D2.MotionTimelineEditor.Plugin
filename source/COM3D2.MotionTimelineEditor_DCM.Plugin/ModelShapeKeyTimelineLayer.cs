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
    using ModelShapeKeyPlayData = MotionPlayData<ModelShapeKeyMotionData>;

    public class ModelShapeKeyTimeLineRow
    {
        public int frame;
        public string name;
        public float weight;
        public int easing;
    }

    public class ModelShapeKeyMotionData : IMotionData
    {
        public int stFrame { get; set; }
        public int edFrame { get; set; }

        public string name;
        public float stWeight;
        public float edWeight;
        public int easing;
    }

    [LayerDisplayName("モデルシェイプキー")]
    public partial class ModelShapeKeyTimelineLayer : TimelineLayerBase
    {
        public override int priority
        {
            get
            {
                return 23;
            }
        }

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

        private Dictionary<string, List<ModelShapeKeyTimeLineRow>> _timelineRowsMap = new Dictionary<string, List<ModelShapeKeyTimeLineRow>>();
        private Dictionary<string, ModelShapeKeyPlayData> _playDataMap = new Dictionary<string, ModelShapeKeyPlayData>();
        private List<ModelShapeKeyMotionData> _outputMotions = new List<ModelShapeKeyMotionData>(128);

        private ModelShapeKeyTimelineLayer(int slotNo) : base(slotNo)
        {
        }

        public static ModelShapeKeyTimelineLayer Create(int slotNo)
        {
            return new ModelShapeKeyTimelineLayer(0);
        }

        public override void Init()
        {
            base.Init();
            SetupModels();
        }

        protected override void InitMenuItems()
        {
            allMenuItems.Clear();

            foreach (var model in modelManager.modelMap.Values)
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

            var models = modelManager.modelMap.Values;
            foreach (var model in models)
            {
                model.FixBlendValues();
            }
        }

        private void ApplyMotion(ModelShapeKeyMotionData motion, StudioModelBlendShape blendShape, float lerpTime)
        {
            float easingTime = CalcEasingValue(lerpTime, motion.easing);
            var weight = Mathf.Lerp(motion.stWeight, motion.edWeight, easingTime);
            blendShape.weight = weight;
        }

        public override void OnPluginEnable()
        {
            base.OnPluginEnable();

            SetupModels();
        }

        private void SetupModels()
        {
            var existModelNameHash = new HashSet<string>();

            var existBoneNames = GetExistBoneNames();
            foreach (var boneName in existBoneNames)
            {
                var modelName = boneName.Split('/')[0];
                if (!string.IsNullOrEmpty(modelName))
                {
                    existModelNameHash.Add(modelName);
                }
            }

            modelManager.SetupModels(existModelNameHash);
            InitMenuItems();
        }

        public override void OnPluginDisable()
        {
            base.OnPluginDisable();

            studioHack.DeleteAllModels();
        }

        public override void OnModelAdded(StudioModelStat model)
        {
            InitMenuItems();

            var boneNames = model.blendShapes.Select(x => x.name).ToList();
            AddFirstBones(boneNames);
        }

        public override void OnModelRemoved(StudioModelStat model)
        {
            InitMenuItems();

            var boneNames = model.blendShapes.Select(x => x.name).ToList();
            RemoveAllBones(boneNames);
        }

        public override void UpdateFrameWithCurrentStat(FrameData frame)
        {
            foreach (var blendShape in modelManager.blendShapeMap.Values)
            {
                var boneName = blendShape.name;

                var trans = CreateTransformData(boneName);
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
            PluginUtils.LogDebug("BuildPlayData");
            _playDataMap.Clear();

            bool warpFrameEnabled = forOutput || !studioHack.isPoseEditing;

            foreach (var pair in _timelineRowsMap)
            {
                var name = pair.Key;
                var rows = pair.Value;

                var blendShape = modelManager.GetBlendShape(name);
                if (blendShape == null)
                {
                    continue;
                }

                ModelShapeKeyPlayData playData;
                if (!_playDataMap.TryGetValue(name, out playData))
                {
                    playData = new ModelShapeKeyPlayData
                    {
                        motions = new List<ModelShapeKeyMotionData>(rows.Count),
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

                    var motion = new ModelShapeKeyMotionData
                    {
                        name = name,
                        stFrame = stFrame,
                        edFrame = edFrame,

                        stWeight = start.weight,
                        edWeight = end.weight,
                        easing = end.easing,
                    };

                    playData.motions.Add(motion);
                }
            }

            foreach (var pair in _playDataMap)
            {
                var name = pair.Key;
                var playData = pair.Value;
                PluginUtils.LogDebug("PlayData: name={0}, count={1}", name, playData.motions.Count);
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

                List<ModelShapeKeyTimeLineRow> rows;
                if (!_timelineRowsMap.TryGetValue(name, out rows))
                {
                    rows = new List<ModelShapeKeyTimeLineRow>();
                    _timelineRowsMap[name] = rows;
                }

                var trans = bone.transform;

                var row = new ModelShapeKeyTimeLineRow
                {
                    frame = frame.frameNo,
                    name = bone.name,
                    weight = trans["weight"].value,
                    easing = trans.easing
                };

                rows.Add(row);
            }
        }

        public override void OutputDCM(XElement songElement)
        {
            PluginUtils.LogWarning("モデルシェイプキーはDCMに対応していません");
        }

        public override float CalcEasingValue(float t, int easing)
        {
            return TimelineMotionEasing.MotionEasing(t, (EasingType) easing);
        }

        private int _targetModelIndex = 0;
        private List<FloatFieldValue> _fieldValues = new List<FloatFieldValue>();

        public override void DrawWindow(GUIView view)
        {
            if (modelManager.blendShapeNames.Count == 0)
            {
                view.DrawLabel("シェイプキーが存在しません", 200, 20);
                return;
            }

            view.BeginLayout(GUIView.LayoutDirection.Horizontal);
            {
                view.DrawLabel("モデル選択", 70, 20);

                var newTargetModelIndex = view.DrawSelectList(
                    modelManager.modelNames,
                    (modelName, index) =>
                    {
                        var b = modelManager.GetModel(index);
                        return b != null ? b.name : string.Empty;
                    },
                    140, 20, _targetModelIndex);

                if (newTargetModelIndex != _targetModelIndex)
                {
                    _targetModelIndex = newTargetModelIndex;
                }
            }
            view.EndLayout();

            var model = modelManager.GetModel(_targetModelIndex);
            if (model == null)
            {
                return;
            }

            var blendShapes = model.blendShapes;

            while (_fieldValues.Count < blendShapes.Count)
            {
                var fieldValue = new FloatFieldValue();
                _fieldValues.Add(fieldValue);
            }

            view.SetEnabled(studioHack.isPoseEditing);

            for (var i = 0; i < blendShapes.Count; i++)
            {
                var blendShape = blendShapes[i];
                var fieldValue = _fieldValues[i];

                fieldValue.label = blendShape.shapeKeyName;

                var weight = blendShape.weight;
                var updateTransform = false;

                updateTransform |= view.DrawSliderValue(fieldValue, -1f, 2f, 0f,
                    weight,
                    x => weight = x);

                if (updateTransform)
                {
                    blendShape.weight = weight;
                    model.FixBlendValues();
                }
            }

            view.SetEnabled(true);
        }

        public override ITransformData CreateTransformData(string name)
        {
            var transform = new TransformDataShapeKey();
            transform.Initialize(name);
            return transform;
        }
    }
}