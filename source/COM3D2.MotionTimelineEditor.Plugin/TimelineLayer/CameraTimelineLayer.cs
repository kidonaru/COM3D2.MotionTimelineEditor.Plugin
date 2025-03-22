using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    [TimelineLayerDesc("カメラ", 20)]
    public class CameraTimelineLayer : TimelineLayerBase
    {
        public override Type layerType => typeof(CameraTimelineLayer);
        public override string layerName => nameof(CameraTimelineLayer);

        public override bool isCameraLayer => true;

        private static Camera camera => PluginUtils.MainCamera;
        private static Camera subCamera => studioHack.subCamera;

        public static string CameraBoneName = "camera";
        public static string CameraDisplayName = "カメラ";

        private List<string> _allBoneNames = new List<string> { CameraBoneName };
        public override List<string> allBoneNames => _allBoneNames;

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

            if (!studioHackManager.isPoseEditing)
            {
                ApplyPlayData();
            }
        }

        public override void LateUpdate()
        {
            base.LateUpdate();
        }

        protected override void ApplyPlayData()
        {
            if (!isCurrent && !config.isCameraSync)
            {
                return;
            }

            base.ApplyPlayData();
        }

        protected override void ApplyMotion(MotionData motion, float t, bool indexUpdated)
        {
            Vector3 position, eulerAngles;
            float distance, viewAngle;

            var start = motion.start;
            var end = motion.end;

            if (timeline.isTangentCamera)
            {
                var t0 = motion.stFrame * timeline.frameDuration;
                var t1 = motion.edFrame * timeline.frameDuration;

                position = PluginUtils.HermiteVector3(
                    t0,
                    t1,
                    start.positionValues,
                    end.positionValues,
                    t);

                eulerAngles = PluginUtils.HermiteVector3(
                    t0,
                    t1,
                    start.eulerAnglesValues,
                    end.eulerAnglesValues,
                    t);

                var tempScale = PluginUtils.HermiteVector3(
                    t0,
                    t1,
                    start.scaleValues,
                    end.scaleValues,
                    t);

                distance = tempScale.x;
                viewAngle = tempScale.y;
            }
            else
            {
                float easing = CalcEasingValue(t, motion.easing);
                position = Vector3.Lerp(start.position, end.position, easing);
                eulerAngles = Vector3.Lerp(start.eulerAngles, end.eulerAngles, easing);
                distance = Mathf.Lerp(start.scale.x, end.scale.x, easing);
                viewAngle = Mathf.Lerp(start.scale.y, end.scale.y, easing);
            }

            if (config.isFixedFoV && !isCurrent && studioHackManager.isPoseEditing)
            {
                viewAngle = 35;
            }

            var uoCamera = GetUOCamera();
            uoCamera.SetTargetPos(position);
            uoCamera.SetDistance(distance);
            uoCamera.SetAroundAngle(new Vector2(eulerAngles.y, eulerAngles.x));
            camera.SetRotationZ(eulerAngles.z);
            camera.fieldOfView = viewAngle;

            if (subCamera != null)
            {
                subCamera.fieldOfView = viewAngle;
            }

            if (config.isFixedFocus && !isCurrent && studioHackManager.isPoseEditing)
            {
                var currentMaidCache = maidManager.maidCache;
                if (currentMaidCache != null)
                {
                    var target = currentMaidCache.GetPointTransform(MaidPointType.Head);
                    uoCamera.MoveTarget(target.position);
                }
            }

            //MTEUtils.LogDebug("ApplyMotion: position={0}, rotation={1}, distance={2}, viewAngle={3}", position, rotation, distance, viewAngle);
        }

        public static UltimateOrbitCamera GetUOCamera()
        {
            return camera.GetComponent<UltimateOrbitCamera>();
        }

        public override void UpdateFrame(FrameData frame, bool initialEdit, bool force)
        {
            var uoCamera = GetUOCamera();
            var target = uoCamera.target;
            var angle = uoCamera.GetAroundAngle();
            var rotZ = camera.GetRotationZ();

            var trans = CreateTransformData<TransformDataCamera>(CameraBoneName);
            trans.position = target.position;
            trans.eulerAngles = new Vector3(angle.y, angle.x, rotZ);
            trans.easing = GetEasing(frame.frameNo, CameraBoneName);
            trans.scale = new Vector3(uoCamera.distance, camera.fieldOfView, 0);

            var bone = frame.CreateBone(trans);
            frame.UpdateBone(bone);

            //MTEUtils.LogDebug("UpdateFromCurrentPose: position={0}, rotation={1}", _cameraManager.CurrentPosition,_cameraManager.CurrentRotation);
        }

        public void OutputBones(List<BoneData> rows, string filePath)
        {
            var offsetTime = timeline.startOffsetTime;
            var offsetFrame = (int) Mathf.Round(offsetTime * timeline.frameRate);
            var frameFactor = 30f / timeline.frameRate;

            var builder = new StringBuilder();
            builder.Append("frame,posX,posY,posZ,rotX,RotY,rotZ,distance,viewAngle,easing\r\n");

            Action<BoneData, bool> appendRow = (row, isFirst) =>
            {
                var frameNo = (int) Mathf.Round(row.frameNo * frameFactor);

                if (!isFirst)
                {
                    frameNo += offsetFrame;
                }

                var transform = row.transform;

                builder.Append(frameNo + ",");
                builder.Append(transform.position.x.ToString("0.000") + ",");
                builder.Append(transform.position.y.ToString("0.000") + ",");
                builder.Append(transform.position.z.ToString("0.000") + ",");
                builder.Append(transform.eulerAngles.x.ToString("0.000") + ",");
                builder.Append(transform.eulerAngles.y.ToString("0.000") + ",");
                builder.Append(transform.eulerAngles.z.ToString("0.000") + ",");
                builder.Append(transform.scale.x.ToString("0.000") + ",");
                builder.Append(transform.scale.y.ToString("0.000") + ",");
                builder.Append(transform.easing.ToString());
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

        public void OutputMotions(List<MotionData> motions, string filePath)
        {
            var offsetTime = timeline.startOffsetTime;

            var builder = new StringBuilder();
            builder.Append("easingType,stTime,stPosX,stPosY,stPosZ,stRotX,stRotY,stRotZ," +
                            "edTime,edPosX,edPosY,edPosZ,edRotX,edRotY,edRotZ," +
                            "stDistance,edDistance,stViewAngle,edViewAngle" +
                            "\r\n");
            
            Action<MotionData, bool> appendMotion = (motion, isFirst) =>
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

                var start = motion.start;
                var end = motion.end;

                var stDistance = start.scale.x;
                var edDistance = end.scale.x;
                var stViewAngle = start.scale.y;
                var edViewAngle = end.scale.y;

                builder.Append(start.easing + ",");
                builder.Append(stTime.ToString("0.000") + ",");
                builder.Append(start.position.x.ToString("0.000") + ",");
                builder.Append(start.position.y.ToString("0.000") + ",");
                builder.Append(start.position.z.ToString("0.000") + ",");
                builder.Append(start.eulerAngles.x.ToString("0.000") + ",");
                builder.Append(start.eulerAngles.y.ToString("0.000") + ",");
                builder.Append(start.eulerAngles.z.ToString("0.000") + ",");
                builder.Append(edTime.ToString("0.000") + ",");
                builder.Append(end.position.x.ToString("0.000") + ",");
                builder.Append(end.position.y.ToString("0.000") + ",");
                builder.Append(end.position.z.ToString("0.000") + ",");
                builder.Append(end.eulerAngles.x.ToString("0.000") + ",");
                builder.Append(end.eulerAngles.y.ToString("0.000") + ",");
                builder.Append(end.eulerAngles.z.ToString("0.000") + ",");
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
                    OutputBones(_timelineBonesMap[CameraBoneName], outputPath);

                    //songElement.Add(new XElement("customMotion", outputFileName));
                    songElement.Add(new XComment(string.Format("<customMotion>{0}</customMotion>", outputFileName)));
                }

                {
                    var outputFileName = "camera_motion.csv";
                    var outputPath = timeline.GetDcmSongFilePath(outputFileName);
                    OutputMotions(_playDataMap[CameraBoneName].motions, outputPath);

                    songElement.Add(new XElement("motion", outputFileName));
                }
            }
            catch (Exception e)
            {
                MTEUtils.LogException(e);
                MTEUtils.LogError("カメラモーションの出力に失敗しました");
            }
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
            var uoCamera = GetUOCamera();
            var target = uoCamera.target;
            var position = target.position;
            var aroundAngle = uoCamera.GetAroundAngle();
            var rotZ = camera.GetRotationZ();
            var angles = new Vector3(aroundAngle.y, aroundAngle.x, rotZ);
            var distance = uoCamera.distance;
            var prevBone = GetPrevBone(timelineManager.currentFrameNo, CameraBoneName);
            var prevAngles = prevBone != null ? prevBone.transform.eulerAngles : Vector3.zero;
            angles = TransformDataBase.GetFixedEulerAngles(angles, prevAngles);
            var updateTransform = false;

            var initialPosition = Vector3.zero;
            var initialEulerAngles = Vector3.zero;

            view.SetEnabled(!view.IsComboBoxFocused() && studioHackManager.isPoseEditing);

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
                    value = camera.fieldOfView,
                    onChanged = a => camera.fieldOfView = a,
                });

            view.DrawHorizontalLine(Color.gray);

            view.DrawLabel("対象設定", 70, 20);

            Action updateCamera = () =>
            {
                uoCamera.SetTargetPos(position);
                uoCamera.SetDistance(distance);
                uoCamera.SetAroundAngle(new Vector2(angles.y, angles.x));
                camera.SetRotationZ(angles.z);
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

        public override TransformType GetTransformType(string name)
        {
            return TransformType.Camera;
        }
    }
}