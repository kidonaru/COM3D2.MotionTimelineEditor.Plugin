using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using COM3D2.MotionTimelineEditor.Plugin;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor_DCM.Plugin
{
    using MyTransform = DanceCameraMotion.Plugin.MyTransform;
    using MyHelper = DanceCameraMotion.Plugin.MyHelper;
    using TimelineMotionEasing = DanceCameraMotion.Plugin.TimelineMotionEasing;
    using EasingType = DanceCameraMotion.Plugin.EasingType;
    using CameraPlayData = MotionPlayData<CameraMotionData>;

    public class CameraTimeLineRow
    {
        public int frame;
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;
        public int easing;
        public Vector3 positionInTangents;
        public Vector3 positionOutTangents;
        public Vector3 eulerAnglesInTangents;
        public Vector3 eulerAnglesOutTangents;
        public Vector3 scaleInTangents;
        public Vector3 scaleOutTangents;
    }

    public class CameraMotionData : MotionDataBase
    {
        public MyTransform myTm;
        public int easing;
        public Vector3 posOutTangents;
        public Vector3 posInTangents;
        public Vector3 rotOutTangents;
        public Vector3 rotInTangents;
        public Vector3 scaleOutTangents;
        public Vector3 scaleInTangents;
    }

    [LayerDisplayName("カメラ")]
    public class CameraTimelineLayer : TimelineLayerBase
    {
        public override int priority
        {
            get
            {
                return 20;
            }
        }

        public override string className
        {
            get
            {
                return typeof(CameraTimelineLayer).Name;
            }
        }

        public override bool isCameraLayer
        {
            get
            {
                return true;
            }
        }

        private static Camera subCamera
        {
            get
            {
                return studioHack.subCamera;
            }
        }

        public static string CameraBoneName = "camera";
        public static string CameraDisplayName = "カメラ";

        private List<string> _allBoneNames = new List<string> { CameraBoneName };
        public override List<string> allBoneNames
        {
            get
            {
                return _allBoneNames;
            }
        }

        private List<CameraTimeLineRow> _timelineRows = new List<CameraTimeLineRow>();
        private CameraPlayData _playData = new CameraPlayData();

        private CameraTimelineLayer(int slotNo) : base(slotNo)
        {
        }

        public static CameraTimelineLayer Create(int slotNo)
        {
            return new CameraTimelineLayer(0);
        }

        public override void Init()
        {
            base.Init();
        }

        protected override void InitMenuItems()
        {
            allMenuItems.Clear();

            var menuItem = new BoneMenuItem(CameraBoneName, CameraDisplayName);
            allMenuItems.Add(menuItem);
        }

        public override bool IsValidData()
        {
            errorMessage = "";
            return true;
        }

        public override void Update()
        {
            base.Update();

            if (!studioHack.isPoseEditing)
            {
                ApplyPlayData();
            }
        }

        public override void LateUpdate()
        {
            base.LateUpdate();
        }

        private void ApplyPlayData()
        {
            if (!isCurrent && !config.isCameraSync)
            {
                return;
            }

            var playingFrameNoFloat = this.playingFrameNoFloat;

            _playData.Update(playingFrameNoFloat);

            if (_playData.current == null)
            {
                PluginUtils.LogError("ApplyCamera: カメラデータの取得に失敗しました");
                return;
            }

            //PluginUtils.LogDebug("ApplyCamera: lerpFrame={0}, listIndex={1}, playingFrameNo={2}",
            //    _playData.lerpFrame, _playData.listIndex, playingFrameNoFloat);

            ApplyMotion(_playData.current);
        }

        private void ApplyMotion(CameraMotionData motion)
        {
            Vector3 position, rotation;
            float distance, viewAngle;

            if (timeline.isTangentCamera)
            {
                var t0 = motion.stFrame * timeline.frameDuration;
                var t1 = motion.edFrame * timeline.frameDuration;

                position = PluginUtils.HermiteVector3(
                    t0,
                    t1,
                    motion.myTm.stPos,
                    motion.myTm.edPos,
                    motion.posOutTangents,
                    motion.posInTangents,
                    _playData.lerpFrame);

                rotation = PluginUtils.HermiteVector3(
                    t0,
                    t1,
                    motion.myTm.stRot,
                    motion.myTm.edRot,
                    motion.rotOutTangents,
                    motion.rotInTangents,
                    _playData.lerpFrame);

                var tempScale = PluginUtils.HermiteVector3(
                    t0,
                    t1,
                    motion.myTm.stSca,
                    motion.myTm.edSca,
                    motion.scaleOutTangents,
                    motion.scaleInTangents,
                    _playData.lerpFrame);

                distance = tempScale.x;
                viewAngle = tempScale.y;
            }
            else
            {
                float easing = CalcEasingValue(_playData.lerpFrame, motion.easing);
                position = Vector3.Lerp(motion.myTm.stPos, motion.myTm.edPos, easing);
                rotation = Vector3.Lerp(motion.myTm.stRot, motion.myTm.edRot, easing);
                distance = Mathf.Lerp(motion.myTm.stSca.x, motion.myTm.edSca.x, easing);
                viewAngle = Mathf.Lerp(motion.myTm.stSca.y, motion.myTm.edSca.y, easing);
            }

            var uoCamera = MyHelper.GetUOCamera();
            uoCamera.SetTargetPos(position);
            uoCamera.SetDistance(distance);
            uoCamera.SetAroundAngle(new Vector2(rotation.y, rotation.x));
            Camera.main.SetRotationZ(rotation.z);
            Camera.main.fieldOfView = viewAngle;

            if (subCamera != null)
            {
                subCamera.fieldOfView = viewAngle;
            }

            //PluginUtils.LogDebug("ApplyMotion: position={0}, rotation={1}, distance={2}, viewAngle={3}", position, rotation, distance, viewAngle);
        }

        public override void UpdateFrame(FrameData frame)
        {
            var uOCamera = MyHelper.GetUOCamera();
            var target = uOCamera.target;
            var angle = uOCamera.GetAroundAngle();
            var rotZ = Camera.main.GetRotationZ();

            var trans = CreateTransformData(CameraBoneName);
            trans.position = target.position;
            trans.eulerAngles = new Vector3(angle.y, angle.x, rotZ);
            trans.easing = GetEasing(frame.frameNo, CameraBoneName);
            trans.scale = new Vector3(uOCamera.distance, Camera.main.fieldOfView, 0);

            var bone = frame.CreateBone(trans);
            frame.UpdateBone(bone);

            //PluginUtils.LogDebug("UpdateFromCurrentPose: position={0}, rotation={1}", _cameraManager.CurrentPosition,_cameraManager.CurrentRotation);
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

        protected override byte[] GetAnmBinaryInternal(
            bool forOutput,
            int startFrameNo,
            int endFrameNo)
        {
            _timelineRows.Clear();

            foreach (var keyFrame in keyFrames)
            {
                AddTimeLineRow(keyFrame);
            }

            if (_timelineRows.Count > 0 && _timelineRows.Last().frame != timeline.maxFrameNo)
            {
                AddTimeLineRow(_dummyLastFrame);
            }

            BuildPlayData(forOutput);
            return null;
        }

        private void AddTimeLineRow(FrameData frame)
        {
            var bone = frame.GetBone(CameraBoneName);
            if (bone == null)
            {
                return;
            }

            var trans = bone.transform;

            var row = new CameraTimeLineRow
            {
                frame = frame.frameNo,
                position = trans.position,
                rotation = trans.eulerAngles,
                easing = trans.easing,
                scale = trans.scale,
                positionInTangents = trans.positionInTangents.ToVector3(),
                positionOutTangents = trans.positionOutTangents.ToVector3(),
                eulerAnglesInTangents = trans.eulerAnglesInTangents.ToVector3(),
                eulerAnglesOutTangents = trans.eulerAnglesOutTangents.ToVector3(),
                scaleInTangents = trans.scaleInTangents.ToVector3(),
                scaleOutTangents = trans.scaleOutTangents.ToVector3(),
            };

            _timelineRows.Add(row);
        }

        private void BuildPlayData(bool forOutput)
        {
            _playData.motions.Clear();

            for (var i = 0; i < _timelineRows.Count - 1; i++)
            {
                var start = _timelineRows[i];
                var end = _timelineRows[i + 1];

                var motion = new CameraMotionData
                {
                    stFrame = start.frame,
                    edFrame = end.frame,
                    myTm = new MyTransform
                    {
                        stPos = start.position,
                        edPos = end.position,
                        stRot = start.rotation,
                        edRot = end.rotation,
                        stSca = start.scale,
                        edSca = end.scale,
                    },
                    easing = end.easing,
                    posOutTangents = start.positionOutTangents,
                    posInTangents = end.positionInTangents,
                    rotOutTangents = start.eulerAnglesOutTangents,
                    rotInTangents = end.eulerAnglesInTangents,
                    scaleOutTangents = start.scaleOutTangents,
                    scaleInTangents = end.scaleInTangents,
                };
                _playData.motions.Add(motion);
            }

            _playData.Setup(timeline.singleFrameType);
        }

        public void SaveCameraTimeLine(
            List<CameraTimeLineRow> rows,
            string filePath)
        {
            var offsetTime = timeline.startOffsetTime;
            var offsetFrame = (int) Mathf.Round(offsetTime * timeline.frameRate);
            var frameFactor = 30f / timeline.frameRate;

            var builder = new StringBuilder();
            builder.Append("frame,posX,posY,posZ,rotX,RotY,rotZ,distance,viewAngle,easing\r\n");

            Action<CameraTimeLineRow, bool> appendRow = (row, isFirst) =>
            {
                var frameNo = (int) Mathf.Round(row.frame * frameFactor);

                if (!isFirst)
                {
                    frameNo += offsetFrame;
                }

                builder.Append(frameNo + ",");
                builder.Append(row.position.x.ToString("0.000") + ",");
                builder.Append(row.position.y.ToString("0.000") + ",");
                builder.Append(row.position.z.ToString("0.000") + ",");
                builder.Append(row.rotation.x.ToString("0.000") + ",");
                builder.Append(row.rotation.y.ToString("0.000") + ",");
                builder.Append(row.rotation.z.ToString("0.000") + ",");
                builder.Append(row.scale.x.ToString("0.000") + ",");
                builder.Append(row.scale.y.ToString("0.000") + ",");
                builder.Append(row.easing.ToString());
                builder.Append("\r\n");
            };

            if (rows.Count > 0 && offsetFrame > 0)
            {
                appendRow(rows.First(), true);
            }

            foreach (var row in rows)
            {
                appendRow(row, false);
            }

            using (var streamWriter = new StreamWriter(filePath, false))
            {
                streamWriter.Write(builder.ToString());
            }
        }

        public void SaveCameraMotion(
            List<CameraMotionData> motions,
            string filePath)
        {
            var offsetTime = timeline.startOffsetTime;

            var builder = new StringBuilder();
            builder.Append("easingType,stTime,stPosX,stPosY,stPosZ,stRotX,stRotY,stRotZ," +
                            "edTime,edPosX,edPosY,edPosZ,edRotX,edRotY,edRotZ," +
                            "stDistance,edDistance,stViewAngle,edViewAngle" +
                            "\r\n");
            
            Action<CameraMotionData, bool> appendMotion = (motion, isFirst) =>
            {
                var stTime = motion.stFrame * timeline.frameDuration;
                var edTime = motion.edFrame * timeline.frameDuration;

                if (isFirst)
                {
                    stTime = 0f;
                    edTime = offsetTime;
                }
                else
                {
                    stTime += offsetTime;
                    edTime += offsetTime;
                }

                var stDistance = motion.myTm.stSca.x;
                var edDistance = motion.myTm.edSca.x;
                var stViewAngle = motion.myTm.stSca.y;
                var edViewAngle = motion.myTm.edSca.y;

                builder.Append(motion.easing + ",");
                builder.Append(stTime.ToString("0.000") + ",");
                builder.Append(motion.myTm.stPos.x.ToString("0.000") + ",");
                builder.Append(motion.myTm.stPos.y.ToString("0.000") + ",");
                builder.Append(motion.myTm.stPos.z.ToString("0.000") + ",");
                builder.Append(motion.myTm.stRot.x.ToString("0.000") + ",");
                builder.Append(motion.myTm.stRot.y.ToString("0.000") + ",");
                builder.Append(motion.myTm.stRot.z.ToString("0.000") + ",");
                builder.Append(edTime.ToString("0.000") + ",");
                builder.Append(motion.myTm.edPos.x.ToString("0.000") + ",");
                builder.Append(motion.myTm.edPos.y.ToString("0.000") + ",");
                builder.Append(motion.myTm.edPos.z.ToString("0.000") + ",");
                builder.Append(motion.myTm.edRot.x.ToString("0.000") + ",");
                builder.Append(motion.myTm.edRot.y.ToString("0.000") + ",");
                builder.Append(motion.myTm.edRot.z.ToString("0.000") + ",");
                builder.Append(stDistance.ToString("0.000") + ",");
                builder.Append(edDistance.ToString("0.000") + ",");
                builder.Append(stViewAngle.ToString("0.000") + ",");
                builder.Append(edViewAngle.ToString("0.000"));
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
                {
                    var outputFileName = "camera_timeline.csv";
                    var outputPath = timeline.GetDcmSongFilePath(outputFileName);
                    SaveCameraTimeLine(_timelineRows, outputPath);

                    //songElement.Add(new XElement("customMotion", outputFileName));
                    songElement.Add(new XComment(string.Format("<customMotion>{0}</customMotion>", outputFileName)));
                }

                {
                    var outputFileName = "camera_motion.csv";
                    var outputPath = timeline.GetDcmSongFilePath(outputFileName);
                    SaveCameraMotion(_playData.motions, outputPath);

                    songElement.Add(new XElement("motion", outputFileName));
                }
            }
            catch (Exception e)
            {
                PluginUtils.LogException(e);
                PluginUtils.ShowDialog("カメラモーションの出力に失敗しました");
            }
        }

        public override float CalcEasingValue(float t, int easing)
        {
            return TimelineMotionEasing.MotionEasing(t, (EasingType) easing);
        }

        private GUIComboBox<MaidCache> _maidComboBox = new GUIComboBox<MaidCache>
        {
            getName = (maidCache, _) => maidCache == null ? "未選択" : maidCache.fullName,
            buttonSize = new Vector2(100, 20),
            contentSize = new Vector2(150, 300),
        };

        private GUIComboBox<MaidPointType> _targetMaidPointComboBox = new GUIComboBox<MaidPointType>
        {
            items = Enum.GetValues(typeof(MaidPointType)).Cast<MaidPointType>().ToList(),
            getName = (type, index) => MaidCache.GetMaidPointTypeName(type),
            buttonSize = new Vector2(50, 20),
        };

        private GUIComboBox<StudioModelStat> _modelComboBox = new GUIComboBox<StudioModelStat>
        {
            getName = (model, index) => model.displayName,
            buttonSize = new Vector2(100, 20),
            contentSize = new Vector2(200, 300),
        };

        public override void DrawWindow(GUIView view)
        {
            var uoCamera = MyHelper.GetUOCamera();
            var target = uoCamera.target;
            var position = target.position;
            var aroundAngle = uoCamera.GetAroundAngle();
            var rotZ = Camera.main.GetRotationZ();
            var angles = new Vector3(aroundAngle.y, aroundAngle.x, rotZ);
            var distance = uoCamera.distance;
            var prevBone = GetPrevBone(timelineManager.currentFrameNo, CameraBoneName);
            var prevAngles = prevBone != null ? prevBone.transform.eulerAngles : Vector3.zero;
            angles = TransformDataBase.GetFixedEulerAngles(angles, prevAngles);
            var updateTransform = false;

            var initialPosition = Vector3.zero;
            var initialEulerAngles = Vector3.zero;

            view.SetEnabled(!view.IsComboBoxFocused() && studioHack.isPoseEditing);

            updateTransform |= view.DrawSliderValue(
                new GUIView.SliderOption
                {
                    label = "X",
                    labelWidth = 30,
                    min = -config.positionRange,
                    max = config.positionRange,
                    step = 0.01f,
                    defaultValue = initialPosition.x,
                    value = position.x,
                    onChanged = x => position.x = x,
                });
            
            updateTransform |= view.DrawSliderValue(
                new GUIView.SliderOption
                {
                    label = "Y",
                    labelWidth = 30,
                    min = -config.positionRange,
                    max = config.positionRange,
                    step = 0.01f,
                    defaultValue = initialPosition.y,
                    value = position.y,
                    onChanged = y => position.y = y,
                });

            updateTransform |= view.DrawSliderValue(
                new GUIView.SliderOption
                {
                    label = "Z",
                    labelWidth = 30,
                    min = -config.positionRange,
                    max = config.positionRange,
                    step = 0.01f,
                    defaultValue = initialPosition.z,
                    value = position.z,
                    onChanged = z => position.z = z,
                });

            updateTransform |= view.DrawSliderValue(
                new GUIView.SliderOption
                {
                    label = "RX",
                    labelWidth = 30,
                    min = prevAngles.x - 180f,
                    max = prevAngles.x + 180f,
                    step = 1f,
                    defaultValue = initialEulerAngles.x,
                    value = angles.x,
                    onChanged = x => angles.x = x,
                });

            updateTransform |= view.DrawSliderValue(
                new GUIView.SliderOption
                {
                    label = "RY",
                    labelWidth = 30,
                    min = prevAngles.y - 180f,
                    max = prevAngles.y + 180f,
                    step = 1f,
                    defaultValue = initialEulerAngles.y,
                    value = angles.y,
                    onChanged = y => angles.y = y,
                });

            updateTransform |= view.DrawSliderValue(
                new GUIView.SliderOption
                {
                    label = "RZ",
                    labelWidth = 30,
                    min = prevAngles.z - 180f,
                    max = prevAngles.z + 180f,
                    step = 1f,
                    defaultValue = initialEulerAngles.z,
                    value = angles.z,
                    onChanged = z => angles.z = z,
                });

            updateTransform |= view.DrawSliderValue(
                new GUIView.SliderOption
                {
                    label = "距離",
                    labelWidth = 30,
                    min = 0.1f,
                    max = 30,
                    step = 0.01f,
                    defaultValue = 2,
                    value = distance,
                    onChanged = d => distance = d,
                });

            updateTransform |= view.DrawSliderValue(
                new GUIView.SliderOption
                {
                    label = "FoV",
                    labelWidth = 30,
                    min = 1,
                    max = 179,
                    step = 0.1f,
                    defaultValue = 35,
                    value = Camera.main.fieldOfView,
                    onChanged = a => Camera.main.fieldOfView = a,
                });

            view.DrawHorizontalLine(Color.gray);

            view.DrawLabel("対象設定", 70, 20);

            Action updateCamera = () =>
            {
                uoCamera.SetTargetPos(position);
                uoCamera.SetDistance(distance);
                uoCamera.SetAroundAngle(new Vector2(angles.y, angles.x));
                Camera.main.SetRotationZ(angles.z);
            };

            Action focusToMaid = () =>
            {
                var maidCache = _maidComboBox.currentItem;
                if (maidCache != null)
                {
                    var trans = maidCache.GetPointTransform(_targetMaidPointComboBox.currentItem);
                    position = trans.position;
                    updateCamera();
                }
            };

            view.BeginHorizontal();
            {
                view.DrawLabel("対象メイド", 70, 20);

                _maidComboBox.items = maidManager.maidCaches;
                _maidComboBox.onSelected = (maidCache, index) =>
                {
                    focusToMaid();
                };
                _maidComboBox.DrawButton(view);

                if (view.DrawButton("移動", 40, 20))
                {
                    focusToMaid();
                }
            }
            view.EndLayout();

            view.BeginHorizontal();
            {
                view.DrawLabel("対象ポイント", 70, 20);

                _targetMaidPointComboBox.onSelected = (type, index) =>
                {
                    focusToMaid();
                };
                _targetMaidPointComboBox.DrawButton(view);
            }
            view.EndLayout();

            Action focusToModel = () =>
            {
                var model = _modelComboBox.currentItem;
                if (model != null)
                {
                    var trans = model.transform;
                    position = trans.position;
                    updateCamera();
                }
            };

            view.BeginHorizontal();
            {
                view.DrawLabel("対象モデル", 70, 20);

                _modelComboBox.items = modelManager.models;
                _modelComboBox.onSelected = (model, index) =>
                {
                    focusToModel();
                };
                _modelComboBox.DrawButton(view);

                if (view.DrawButton("移動", 40, 20))
                {
                    focusToModel();
                }
            }
            view.EndLayout();

            if (updateTransform)
            {
                updateCamera();
            }
        }

        public override ITransformData CreateTransformData(string name)
        {
            var transform = new TransformDataCamera();
            transform.Initialize(name);
            return transform;
        }
    }
}