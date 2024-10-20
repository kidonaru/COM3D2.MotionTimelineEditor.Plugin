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
    using ModelBonePlayData = MotionPlayData<ModelBoneMotionData>;

    public class ModelBoneTimeLineRow
    {
        public int frame;
        public string name;
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;
        public int easing;
    }

    public class ModelBoneMotionData : MotionDataBase
    {
        public string name;
        public MyTransform myTm;
        public int easing;
    }

    [LayerDisplayName("モデルボーン")]
    public partial class ModelBoneTimelineLayer : ModelTimelineLayerBase
    {
        public override int priority
        {
            get
            {
                return 22;
            }
        }

        public override string className
        {
            get
            {
                return typeof(ModelBoneTimelineLayer).Name;
            }
        }

        public override List<string> allBoneNames
        {
            get
            {
                return modelManager.boneNames;
            }
        }

        private Dictionary<string, List<ModelBoneTimeLineRow>> _timelineRowsMap = new Dictionary<string, List<ModelBoneTimeLineRow>>();
        private Dictionary<string, ModelBonePlayData> _playDataMap = new Dictionary<string, ModelBonePlayData>();

        private ModelBoneTimelineLayer(int slotNo) : base(slotNo)
        {
        }

        public static ModelBoneTimelineLayer Create(int slotNo)
        {
            return new ModelBoneTimelineLayer(0);
        }

        protected override void InitMenuItems()
        {
            allMenuItems.Clear();

            foreach (var model in modelManager.models)
            {
                if (model.bones.Count == 0)
                {
                    continue;
                }

                var setMenuItem = new BoneSetMenuItem(model.name, model.displayName);
                allMenuItems.Add(setMenuItem);

                foreach (var bone in model.bones)
                {
                    var menuItem = new ModelBoneMenuItem(bone.name, bone.transform.name);
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

            foreach (var boneName in _playDataMap.Keys)
            {
                var playData = _playDataMap[boneName];

                var bone = modelManager.GetBone(boneName);
                if (bone == null)
                {
                    continue;
                }

                playData.Update(playingFrameNoFloat);

                var current = playData.current;
                if (current != null)
                {
                    ApplyMotion(current, bone.transform, playData.lerpFrame);
                }

                //PluginUtils.LogDebug("ApplyPlayData: boneName={0} lerpFrame={1}, listIndex={2}", boneName, playData.lerpFrame, playData.listIndex);
            }
        }

        private void ApplyMotion(ModelBoneMotionData motion, Transform transform, float lerpFrame)
        {
            if (transform == null)
            {
                return;
            }

            float easingTime = CalcEasingValue(lerpFrame, motion.easing);
            transform.localPosition = Vector3.Lerp(motion.myTm.stPos, motion.myTm.edPos, easingTime);
            transform.localRotation = Quaternion.Euler(Vector3.Lerp(motion.myTm.stRot, motion.myTm.edRot, easingTime));
            transform.localScale = Vector3.Lerp(motion.myTm.stSca, motion.myTm.edSca, easingTime);
        }

        public override void OnModelAdded(StudioModelStat model)
        {
            InitMenuItems();

            var boneNames = model.bones.Select(x => x.name).ToList();
            AddFirstBones(boneNames);
            ApplyCurrentFrame(true);
        }

        public override void OnModelRemoved(StudioModelStat model)
        {
            InitMenuItems();

            var boneNames = model.bones.Select(x => x.name).ToList();
            RemoveAllBones(boneNames);
            ApplyCurrentFrame(true);
        }

        public override void OnCopyModel(StudioModelStat sourceModel, StudioModelStat newModel)
        {
            var sourceModelBones = sourceModel.bones;
            var newModelName = newModel.name;
            foreach (var keyFrame in keyFrames)
            {
                foreach (var sourceModelBone in sourceModelBones)
                {
                    var sourceBone = keyFrame.GetBone(sourceModelBone.name);
                    if (sourceBone == null)
                    {
                        continue;
                    }

                    var baseName = sourceModelBone.transform.name;
                    var newBoneName = string.Format("{0}/{1}", newModelName, baseName);

                    var newBone = keyFrame.GetOrCreateBone(newBoneName);
                    newBone.transform.FromTransformData(sourceBone.transform);
                }
            }
        }

        public override void UpdateFrame(FrameData frame)
        {
            foreach (var sourceBone in modelManager.boneMap.Values)
            {
                var boneName = sourceBone.name;

                var trans = CreateTransformData(boneName);
                trans.position = sourceBone.transform.localPosition;
                trans.eulerAngles = sourceBone.transform.localEulerAngles;
                trans.scale = sourceBone.transform.localScale;
                trans.easing = GetEasing(frame.frameNo, boneName);

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

            foreach (var pair in _timelineRowsMap)
            {
                var name = pair.Key;
                var rows = pair.Value;

                var bone = modelManager.GetBone(name);
                if (bone == null)
                {
                    continue;
                }

                ModelBonePlayData playData;
                if (!_playDataMap.TryGetValue(name, out playData))
                {
                    playData = new ModelBonePlayData
                    {
                        motions = new List<ModelBoneMotionData>(rows.Count),
                    };
                    _playDataMap[name] = playData;
                }

                playData.ResetIndex();
                playData.motions.Clear();

                for (var i = 0; i < rows.Count - 1; i++)
                {
                    var start = rows[i];
                    var end = rows[i + 1];

                    var stFrame = start.frame;
                    var edFrame = end.frame;

                    var motion = new ModelBoneMotionData
                    {
                        name = name,
                        stFrame = stFrame,
                        edFrame = edFrame,
                        myTm = new MyTransform
                        {
                            stPos = start.position,
                            stRot = start.rotation,
                            stSca = start.scale,
                            edPos = end.position,
                            edRot = end.rotation,
                            edSca = end.scale,
                        },
                        easing = end.easing,
                    };

                    playData.motions.Add(motion);
                }
            }

            foreach (var pair in _playDataMap)
            {
                var name = pair.Key;
                var playData = pair.Value;
                playData.Setup(timeline.singleFrameType);
                //PluginUtils.LogDebug("PlayData: name={0}, count={1}", name, playData.motions.Count);
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

                List<ModelBoneTimeLineRow> rows;
                if (!_timelineRowsMap.TryGetValue(name, out rows))
                {
                    rows = new List<ModelBoneTimeLineRow>();
                    _timelineRowsMap[name] = rows;
                }

                var trans = bone.transform;

                var row = new ModelBoneTimeLineRow
                {
                    frame = frame.frameNo,
                    name = bone.name,
                    position = trans.position,
                    rotation = trans.eulerAngles,
                    scale = trans.scale,
                    easing = trans.easing
                };

                rows.Add(row);
            }
        }

        public override void OutputDCM(XElement songElement)
        {
            PluginUtils.LogWarning("モデルボーンはDCMに対応していません");
        }

        private GUIComboBox<StudioModelStat> _modelComboBox = new GUIComboBox<StudioModelStat>
        {
            getName = (model, index) => model.displayName,
            buttonSize = new Vector2(200, 20),
            contentSize = new Vector2(200, 300),
        };

        private GUIComboBox<TransformEditType> _transComboBox = new GUIComboBox<TransformEditType>
        {
            items = Enum.GetValues(typeof(TransformEditType)).Cast<TransformEditType>().ToList(),
            getName = (type, index) => type.ToString(),
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
                    DrawBone(view);
                    break;
                case TabType.管理:
                    DrawModelManage(view);
                    break;
            }

            view.DrawComboBox();
        }

        public void DrawBone(GUIView view)
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

            var bones = model.bones;
            if (bones.Count == 0)
            {
                view.DrawLabel("ボーンが存在しません", 200, 20);
                return;
            }

            _transComboBox.DrawButton("操作種類", view);

            var editType = _transComboBox.currentItem;

            view.DrawHorizontalLine(Color.gray);

            view.AddSpace(5);

            view.BeginScrollView();
            {
                view.SetEnabled(!view.IsComboBoxFocused() && studioHack.isPoseEditing);

                foreach (var bone in bones)
                {
                    view.DrawLabel(bone.transform.name, 200, 20);

                    DrawTransform(
                        view,
                        bone.transform,
                        editType,
                        DrawMaskAll,
                        bone.name,
                        bone.initialPosition,
                        bone.initialEulerAngles,
                        bone.initialScale);

                    view.DrawHorizontalLine(Color.gray);
                }
            }
            view.SetEnabled(!view.IsComboBoxFocused());
            view.EndScrollView();
        }

        public override ITransformData CreateTransformData(string name)
        {
            var transform = new TransformDataModelBone();
            transform.Initialize(name);
            return transform;
        }
    }
}