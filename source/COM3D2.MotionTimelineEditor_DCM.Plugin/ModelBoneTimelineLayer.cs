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

    public class ModelBoneMotionData : IMotionData
    {
        public int stFrame { get; set; }
        public int edFrame { get; set; }

        public string name;
        public MyTransform myTm;
        public int easing;
    }

    [LayerDisplayName("モデルボーン")]
    public partial class ModelBoneTimelineLayer : TimelineLayerBase
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

            foreach (var model in modelManager.modelMap.Values)
            {
                var setMenuItem = new BoneSetMenuItem(model.name, model.displayName);
                allMenuItems.Add(setMenuItem);

                foreach (var bone in model.bones)
                {
                    var menuItem = new BoneMenuItem(bone.name, bone.transform.name);
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

        private void ApplyMotion(ModelBoneMotionData motion, Transform transform, float lerpTime)
        {
            if (transform == null)
            {
                return;
            }

            float easingTime = CalcEasingValue(lerpTime, motion.easing);
            transform.localPosition = Vector3.Lerp(motion.myTm.stPos, motion.myTm.edPos, easingTime);
            transform.localRotation = Quaternion.Euler(Vector3.Lerp(motion.myTm.stRot, motion.myTm.edRot, easingTime));
            transform.localScale = Vector3.Lerp(motion.myTm.stSca, motion.myTm.edSca, easingTime);
        }

        public override void OnModelAdded(StudioModelStat model)
        {
            InitMenuItems();

            var boneNames = model.bones.Select(x => x.name).ToList();
            AddFirstBones(boneNames);
        }

        public override void OnModelRemoved(StudioModelStat model)
        {
            InitMenuItems();

            var boneNames = model.bones.Select(x => x.name).ToList();
            RemoveAllBones(boneNames);
        }

        public override void UpdateFrameWithCurrentStat(FrameData frame)
        {
            foreach (var sourceBone in modelManager.boneMap.Values)
            {
                var boneName = sourceBone.name;

                var trans = CreateTransformData(boneName);
                trans.position = sourceBone.transform.localPosition;
                trans.eulerAngles = sourceBone.transform.localEulerAngles;
                trans.scale = sourceBone.transform.localScale;

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

        public override float CalcEasingValue(float t, int easing)
        {
            return TimelineMotionEasing.MotionEasing(t, (EasingType) easing);
        }

        private FloatFieldValue[] _fieldValues = FloatFieldValue.CreateArray(
            new string[] { "X", "Y", "Z", "RX", "RY", "RZ", "SX", "SY", "SZ" }
        );
        private ComboBoxValue<StudioModelStat> modelComboBox = new ComboBoxValue<StudioModelStat>
        {
            getName = (model, index) =>
            {
                return model.name;
            }
        };
        private ComboBoxValue<StudioModelBone> boneComboBox = new ComboBoxValue<StudioModelBone>
        {
            getName = (bone, index) =>
            {
                return bone.transform.name;
            }
        };

        public override void DrawWindow(GUIView view)
        {
            DrawBone(view);
            DrawComboBox(view);
        }

        public void DrawBone(GUIView view)
        {
            modelComboBox.items = modelManager.models;

            if (modelManager.models.Count == 0)
            {
                view.DrawLabel("モデルが存在しません", 200, 20);
                return;
            }

            view.SetEnabled(!modelComboBox.focused && !boneComboBox.focused);

            view.DrawLabel("モデル選択", 200, 20);
            view.DrawComboBoxButton(modelComboBox, 260, 20, true);

            var model = modelComboBox.currentItem;
            if (model == null)
            {
                view.DrawLabel("モデルが見つかりません", 200, 20);
                return;
            }

            if (model.bones.Count == 0)
            {
                view.DrawLabel("ボーンが存在しません", 200, 20);
                return;
            }

            boneComboBox.items = model.bones;

            view.DrawLabel("ボーン選択", 200, 20);
            view.DrawComboBoxButton(boneComboBox, 260, 20, true);

            var bone = boneComboBox.currentItem;
            if (bone == null)
            {
                view.DrawLabel("ボーンが存在しません", 200, 20);
                return;
            }

            view.SetEnabled(!modelComboBox.focused && !boneComboBox.focused && studioHack.isPoseEditing);

            var position = bone.transform.localPosition;
            var angle = bone.transform.localEulerAngles;
            var scale = bone.transform.localScale;
            var updateTransform = false;

            updateTransform |= view.DrawValue(_fieldValues[0], 0.01f, 0.1f, 0f,
                position.x,
                x => position.x = x,
                x => position.x += x);

            updateTransform |= view.DrawValue(_fieldValues[1], 0.01f, 0.1f, 0f,
                position.y,
                y => position.y = y,
                y => position.y += y);

            updateTransform |= view.DrawValue(_fieldValues[2], 0.01f, 0.1f, 0f,
                position.z,
                z => position.z = z,
                z => position.z += z);

            updateTransform |= view.DrawValue(_fieldValues[3], 1f, 10f, 0f,
                angle.x,
                x => angle.x = x,
                x => angle.x += x);

            updateTransform |= view.DrawValue(_fieldValues[4], 1f, 10f, 0f,
                angle.y,
                y => angle.y = y,
                y => angle.y += y);

            updateTransform |= view.DrawValue(_fieldValues[5], 1f, 10f, 0f,
                angle.z,
                z => angle.z = z,
                z => angle.z += z);

            updateTransform |= view.DrawValue(_fieldValues[6], 0.01f, 0.1f, 1f,
                scale.x,
                x => scale.x = x,
                x => scale.x += x);

            updateTransform |= view.DrawValue(_fieldValues[7], 0.01f, 0.1f, 1f,
                scale.y,
                y => scale.y = y,
                y => scale.y += y);

            updateTransform |= view.DrawValue(_fieldValues[8], 0.01f, 0.1f, 1f,
                scale.z,
                z => scale.z = z,
                z => scale.z += z);

            if (updateTransform)
            {
                bone.transform.localPosition = position;
                bone.transform.localEulerAngles = angle;
                bone.transform.localScale = scale;
            }
        }

        private void DrawComboBox(GUIView view)
        {
            view.SetEnabled(true);

            view.DrawComboBoxContent(
                modelComboBox,
                240, 300,
                SubWindow.rc_stgw.width, SubWindow.rc_stgw.height,
                20);

            view.DrawComboBoxContent(
                boneComboBox,
                240, 300,
                SubWindow.rc_stgw.width, SubWindow.rc_stgw.height,
                20);
        }

        public override ITransformData CreateTransformData(string name)
        {
            var transform = new TransformDataModel();
            transform.Initialize(name);
            return transform;
        }
    }
}