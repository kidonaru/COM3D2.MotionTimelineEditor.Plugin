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
    using ModelPlayData = MotionPlayData<ModelMotionData>;

    public class ModelTimeLineRow
    {
        public int frame;
        public string name;
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;
        public int easing;
    }

    public class ModelMotionData : IMotionData
    {
        public int stFrame { get; set; }
        public int edFrame { get; set; }

        public string name;
        public MyTransform myTm;
        public int easing;
    }

    [LayerDisplayName("モデル")]
    public partial class ModelTimelineLayer : TimelineLayerBase
    {
        public override int priority
        {
            get
            {
                return 21;
            }
        }

        public override string className
        {
            get
            {
                return typeof(ModelTimelineLayer).Name;
            }
        }

        public override List<string> allBoneNames
        {
            get
            {
                return modelManager.modelNames;
            }
        }

        private Dictionary<string, List<ModelTimeLineRow>> _timelineRowsMap = new Dictionary<string, List<ModelTimeLineRow>>();
        private Dictionary<string, ModelPlayData> _playDataMap = new Dictionary<string, ModelPlayData>();
        private List<ModelMotionData> _outputMotions = new List<ModelMotionData>(128);

        private ModelTimelineLayer(int slotNo) : base(slotNo)
        {
        }

        public static ModelTimelineLayer Create(int slotNo)
        {
            return new ModelTimelineLayer(0);
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
                var menuItem = new BoneMenuItem(model.name, model.displayName);
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
            var playingFrameNoFloat = this.playingFrameNoFloat;

            foreach (var modelName in _playDataMap.Keys)
            {
                var playData = _playDataMap[modelName];

                var model = modelManager.GetModel(modelName);
                if (model == null)
                {
                    continue;
                }

                playData.Update(playingFrameNoFloat);

                var current = playData.current;
                if (current != null)
                {
                    ApplyMotion(current, model.transform, playData.lerpFrame);
                }

                //PluginUtils.LogDebug("ApplyPlayData: modelName={0} lerpTime={1}, listIndex={2}", modelName, playData.lerpTime, playData.listIndex);
            }
        }

        private void ApplyMotion(ModelMotionData motion, Transform transform, float lerpTime)
        {
            if (transform == null)
            {
                return;
            }

            float easingTime = CalcEasingValue(lerpTime, motion.easing);
            transform.position = Vector3.Lerp(motion.myTm.stPos, motion.myTm.edPos, easingTime);
            transform.rotation = Quaternion.Euler(Vector3.Lerp(motion.myTm.stRot, motion.myTm.edRot, easingTime));
            transform.localScale = Vector3.Lerp(motion.myTm.stSca, motion.myTm.edSca, easingTime);
        }

        public override void OnPluginEnable()
        {
            base.OnPluginEnable();

            SetupModels();
        }

        private void SetupModels()
        {
            var existBoneNames = GetExistBoneNames();
            modelManager.SetupModels(new HashSet<string>(existBoneNames));
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
            AddFirstBones(new List<string> { model.name });
        }

        public override void OnModelRemoved(StudioModelStat model)
        {
            InitMenuItems();
            RemoveAllBones(new List<string> { model.name });
        }

        public override void UpdateFrameWithCurrentStat(FrameData frame)
        {
            foreach (var model in modelManager.modelMap.Values)
            {
                var modelName = model.name;

                var trans = CreateTransformData(modelName);
                trans.position = model.transform.position;
                trans.eulerAngles = model.transform.eulerAngles;
                trans.scale = model.transform.localScale;

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

        private void AddMotion(FrameData frame)
        {
            foreach (var name in allBoneNames)
            {
                var bone = frame.GetBone(name);
                if (bone == null)
                {
                    continue;
                }

                List<ModelTimeLineRow> rows;
                if (!_timelineRowsMap.TryGetValue(name, out rows))
                {
                    rows = new List<ModelTimeLineRow>();
                    _timelineRowsMap[name] = rows;
                }

                var trans = bone.transform;

                var row = new ModelTimeLineRow
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

        private void BuildPlayData(bool forOutput)
        {
            PluginUtils.LogDebug("BuildPlayData");
            _playDataMap.Clear();

            bool warpFrameEnabled = forOutput || !studioHack.isPoseEditing;

            foreach (var pair in _timelineRowsMap)
            {
                var name = pair.Key;
                var rows = pair.Value;

                var model = modelManager.GetModel(name);
                if (model == null)
                {
                    continue;
                }

                ModelPlayData playData;
                if (!_playDataMap.TryGetValue(name, out playData))
                {
                    playData = new ModelPlayData
                    {
                        motions = new List<ModelMotionData>(rows.Count),
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

                    var motion = new ModelMotionData
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
                AddMotion(keyFrame);
            }

            AddMotion(_dummyLastFrame);

            BuildPlayData(forOutput);

            return null;
        }

        public void SaveModelMotion(
            List<ModelMotionData> motions,
            string filePath)
        {
            var offsetTime = timeline.startOffsetTime;

            var builder = new StringBuilder();
            builder.Append("model,group,stTime,stPosX,stPosY,stPosZ,stRotX,stRotY,stRotZ,stScaX,stScaY,stScaZ," +
                            "edTime,edPosX,edPosY,edPosZ,edRotX,edRotY,edRotZ,edScaX,edScaY,edScaZ," +
                            "easing,maidSlotNo,option,type," +
                            "bezier1,bezier2,bezierType,slotName" +
                            "\r\n");

            Action<ModelMotionData, bool> appendMotion = (motion, isFirst) =>
            {
                var model = modelManager.GetModel(motion.name);
                if (model == null)
                {
                    return;
                }

                var stTime = motion.stFrame * timeline.frameDuration;
                var edTime = motion.edFrame * timeline.frameDuration;

                if (isFirst)
                {
                    stTime = 0;
                    edTime = offsetTime;
                }
                else
                {
                    stTime += offsetTime;
                    edTime += offsetTime;
                }

                builder.Append(model.info.fileNameOrId + ",");
                builder.Append(model.group + ",");
                builder.Append(stTime.ToString("0.000") + ",");
                builder.Append(motion.myTm.stPos.x.ToString("0.000") + ",");
                builder.Append(motion.myTm.stPos.y.ToString("0.000") + ",");
                builder.Append(motion.myTm.stPos.z.ToString("0.000") + ",");
                builder.Append(motion.myTm.stRot.x.ToString("0.000") + ",");
                builder.Append(motion.myTm.stRot.y.ToString("0.000") + ",");
                builder.Append(motion.myTm.stRot.z.ToString("0.000") + ",");
                builder.Append(motion.myTm.stSca.x.ToString("0.000") + ",");
                builder.Append(motion.myTm.stSca.y.ToString("0.000") + ",");
                builder.Append(motion.myTm.stSca.z.ToString("0.000") + ",");
                builder.Append(edTime.ToString("0.000") + ",");
                builder.Append(motion.myTm.edPos.x.ToString("0.000") + ",");
                builder.Append(motion.myTm.edPos.y.ToString("0.000") + ",");
                builder.Append(motion.myTm.edPos.z.ToString("0.000") + ",");
                builder.Append(motion.myTm.edRot.x.ToString("0.000") + ",");
                builder.Append(motion.myTm.edRot.y.ToString("0.000") + ",");
                builder.Append(motion.myTm.edRot.z.ToString("0.000") + ",");
                builder.Append(motion.myTm.edSca.x.ToString("0.000") + ",");
                builder.Append(motion.myTm.edSca.y.ToString("0.000") + ",");
                builder.Append(motion.myTm.edSca.z.ToString("0.000") + ",");
                builder.Append(motion.easing + ",");
                builder.Append(0 + ","); // maidSlotNo
                builder.Append("" + ","); // option
                builder.Append((int) model.info.type + ",");
                builder.Append(0 + ","); // bezier1
                builder.Append(0 + ","); // bezier2
                builder.Append(0 + ","); // bezierType
                builder.Append(""); // slotName
                builder.Append("\r\n");
            };

            if (motions.Count > 0 && offsetTime > 0f)
            {
                appendMotion(motions.First(), true);
            }
            
            foreach (var motion in motions)
            {
                appendMotion(motion, false);
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
                _outputMotions.Clear();

                foreach (var playData in _playDataMap.Values)
                {
                    _outputMotions.AddRange(playData.motions);
                }

                var outputFileName = "model.csv";
                var outputPath = timeline.GetDcmSongFilePath(outputFileName);
                SaveModelMotion(_outputMotions, outputPath);

                songElement.Add(new XElement("changeModel", outputFileName));
            }
            catch (Exception e)
            {
                PluginUtils.LogException(e);
                PluginUtils.ShowDialog("モデルチェンジの出力に失敗しました");
            }
        }

        public override float CalcEasingValue(float t, int easing)
        {
            return TimelineMotionEasing.MotionEasing(t, (EasingType) easing);
        }

        private int _targetModelIndex = 0;
        private FloatFieldValue[] _fieldValues = FloatFieldValue.CreateArray(
            new string[] { "X", "Y", "Z", "RX", "RY", "RZ", "SX", "SY", "SZ" }
        );

        public override void DrawWindow(GUIView view)
        {
            if (modelManager.modelNames.Count == 0)
            {
                view.DrawLabel("モデルが存在しません", 200, 20);
                return;
            }

            view.BeginLayout(GUIView.LayoutDirection.Horizontal);
            {
                view.DrawLabel("モデル選択", 70, 20);

                var newTargetModelIndex = view.DrawSelectList(
                    modelManager.modelNames,
                    (modelName, index) =>
                    {
                        var m = modelManager.GetModel(index);
                        return m != null ? m.displayName : string.Empty;
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

            var position = model.transform.position;
            var angle = model.transform.eulerAngles;
            var scale = model.transform.localScale;
            var updateTransform = false;

            GUI.enabled = studioHack.isPoseEditing;

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

            GUI.enabled = true;

            if (updateTransform)
            {
                model.transform.position = position;
                model.transform.eulerAngles = angle;
                model.transform.localScale = scale;
            }
        }

        public override ITransformData CreateTransformData(string name)
        {
            var transform = new TransformDataModel();
            transform.Initialize(name);
            return transform;
        }
    }
}