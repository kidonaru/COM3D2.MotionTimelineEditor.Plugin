using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    [TimelineLayerDesc("モデル", 21)]
    public class ModelTimelineLayer : ModelTimelineLayerBase
    {
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

        private Dictionary<string, List<BoneData>> _timelineRowsMap = new Dictionary<string, List<BoneData>>();
        private Dictionary<string, MotionPlayData> _playDataMap = new Dictionary<string, MotionPlayData>();
        private List<MotionData> _outputMotions = new List<MotionData>(128);

        private ModelTimelineLayer(int slotNo) : base(slotNo)
        {
        }

        public static ModelTimelineLayer Create(int slotNo)
        {
            return new ModelTimelineLayer(0);
        }

        protected override void InitMenuItems()
        {
            allMenuItems.Clear();

            foreach (var model in modelManager.models)
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
                if (model == null || model.transform == null)
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

        private void ApplyMotion(MotionData motion, Transform transform, float lerpTime)
        {
            if (transform == null)
            {
                return;
            }

            var start = motion.start;
            var end = motion.end;

            var stTrans = start.transform;
            var edTrans = end.transform;

            float easingTime = CalcEasingValue(lerpTime, stTrans.easing);
            transform.localPosition = Vector3.Lerp(stTrans.position, edTrans.position, easingTime);
            transform.localRotation = Quaternion.Euler(Vector3.Lerp(stTrans.eulerAngles, edTrans.eulerAngles, easingTime));
            transform.localScale = Vector3.Lerp(stTrans.scale, edTrans.scale, easingTime);
        }

        public override void OnModelAdded(StudioModelStat model)
        {
            InitMenuItems();
            AddFirstBones(new List<string> { model.name });
            ApplyCurrentFrame(true);
        }

        public override void OnModelRemoved(StudioModelStat model)
        {
            InitMenuItems();
            RemoveAllBones(new List<string> { model.name });
            ApplyCurrentFrame(true);
        }

        public override void OnCopyModel(StudioModelStat sourceModel, StudioModelStat newModel)
        {
            var sourceModelName = sourceModel.name;
            var newModelName = newModel.name;
            foreach (var keyFrame in keyFrames)
            {
                var sourceBone = keyFrame.GetBone(sourceModelName);
                if (sourceBone == null)
                {
                    continue;
                }

                var newBone = keyFrame.GetOrCreateBone(newModelName);
                newBone.transform.FromTransformData(sourceBone.transform);
            }
        }

        public override void UpdateFrame(FrameData frame)
        {
            foreach (var model in modelManager.models)
            {
                var modelName = model.name;

                var trans = CreateTransformData(modelName);
                trans.position = model.transform.localPosition;
                trans.eulerAngles = model.transform.localEulerAngles;
                trans.scale = model.transform.localScale;
                trans.easing = GetEasing(frame.frameNo, modelName);

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

                List<BoneData> rows;
                if (!_timelineRowsMap.TryGetValue(name, out rows))
                {
                    rows = new List<BoneData>();
                    _timelineRowsMap[name] = rows;
                }

                rows.Add(bone);
            }
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

                var model = modelManager.GetModel(name);
                if (model == null || model.transform == null)
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
                AddMotion(keyFrame);
            }

            AddMotion(_dummyLastFrame);

            BuildPlayData(forOutput);

            return null;
        }

        public void SaveModelMotion(
            List<MotionData> motions,
            string filePath)
        {
            var offsetTime = timeline.startOffsetTime;

            var builder = new StringBuilder();
            builder.Append("model,group,stTime,stPosX,stPosY,stPosZ,stRotX,stRotY,stRotZ,stScaX,stScaY,stScaZ," +
                            "edTime,edPosX,edPosY,edPosZ,edRotX,edRotY,edRotZ,edScaX,edScaY,edScaZ," +
                            "easing,maidSlotNo,option,type," +
                            "bezier1,bezier2,bezierType,slotName" +
                            "\r\n");

            Action<MotionData, bool> appendMotion = (motion, isFirst) =>
            {
                var model = modelManager.GetModel(motion.name);
                if (model == null || model.transform == null)
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

                var start = motion.start;
                var end = motion.end;

                var stTrans = start.transform;
                var edTrans = end.transform;

                builder.Append(model.info.fileNameOrId + ",");
                builder.Append(model.group + ",");
                builder.Append(stTime.ToString("0.000") + ",");
                builder.Append(stTrans.position.x.ToString("0.000") + ",");
                builder.Append(stTrans.position.y.ToString("0.000") + ",");
                builder.Append(stTrans.position.z.ToString("0.000") + ",");
                builder.Append(stTrans.eulerAngles.x.ToString("0.000") + ",");
                builder.Append(stTrans.eulerAngles.y.ToString("0.000") + ",");
                builder.Append(stTrans.eulerAngles.z.ToString("0.000") + ",");
                builder.Append(stTrans.scale.x.ToString("0.000") + ",");
                builder.Append(stTrans.scale.y.ToString("0.000") + ",");
                builder.Append(stTrans.scale.z.ToString("0.000") + ",");
                builder.Append(edTime.ToString("0.000") + ",");
                builder.Append(edTrans.position.x.ToString("0.000") + ",");
                builder.Append(edTrans.position.y.ToString("0.000") + ",");
                builder.Append(edTrans.position.z.ToString("0.000") + ",");
                builder.Append(edTrans.eulerAngles.x.ToString("0.000") + ",");
                builder.Append(edTrans.eulerAngles.y.ToString("0.000") + ",");
                builder.Append(edTrans.eulerAngles.z.ToString("0.000") + ",");
                builder.Append(edTrans.scale.x.ToString("0.000") + ",");
                builder.Append(edTrans.scale.y.ToString("0.000") + ",");
                builder.Append(edTrans.scale.z.ToString("0.000") + ",");
                builder.Append(stTrans.easing + ",");
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
                    DrawModelEdit(view);
                    break;
                case TabType.管理:
                    DrawModelManage(view);
                    break;
            }

            view.DrawComboBox();
        }
        
        public void DrawModelEdit(GUIView view)
        {
            var models = modelManager.models;
            if (models.Count == 0)
            {
                view.DrawLabel("モデルが存在しません", 200, 20);
                return;
            }

            view.SetEnabled(!view.IsComboBoxFocused());

            _transComboBox.DrawButton("操作種類", view);

            var editType = _transComboBox.currentItem;

            view.DrawHorizontalLine(Color.gray);

            view.AddSpace(5);

            view.BeginScrollView();

            view.SetEnabled(!view.IsComboBoxFocused() && studioHack.isPoseEditing);

            foreach (var model in models)
            {
                if (model == null || model.transform == null)
                {
                    continue;
                }

                view.DrawLabel(model.displayName, 200, 20);

                var initialPosition = Vector3.zero;
                var initialEulerAngles = Vector3.zero;
                var initialScale = Vector3.one;

                DrawTransform(
                    view,
                    model.transform,
                    editType,
                    DrawMaskAll,
                    model.name,
                    initialPosition,
                    initialEulerAngles,
                    initialScale);

                view.DrawHorizontalLine(Color.gray);
            }

            view.SetEnabled(!view.IsComboBoxFocused());
            view.EndScrollView();
        }

        public override ITransformData CreateTransformData(string name)
        {
            var transform = new TransformDataModel();
            transform.Initialize(name);
            return transform;
        }
    }
}