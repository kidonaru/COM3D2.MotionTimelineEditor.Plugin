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
    using CameraPlayData = MotionPlayData<CameraMotionData>;

    public class CameraTimeLineRow
    {
        public int frame;
        public Vector3 position;
        public Vector3 rotation;
        public float distance;
        public float viewAngle;
        public int easing;
    }

    public class CameraMotionData : IMotionData
    {
        public int stFrame { get; set; }
        public int edFrame { get; set; }

        public MyTransform myTm;
        public int easing;
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
        private DanceCameraMotion.Plugin.MaidManager _dcmMaidManager = new DanceCameraMotion.Plugin.MaidManager();

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

            //PluginUtils.LogDebug("ApplyCamera: lerpFrame={0}, listIndex={1}", playData.lerpFrame, playData.listIndex);

            ApplyMotion(_playData.current);
        }

        private void ApplyMotion(CameraMotionData  motion)
        {
            float easing = CalcEasingValue(_playData.lerpFrame, motion.easing);
            var position = Vector3.Lerp(motion.myTm.stPos, motion.myTm.edPos, easing);
            var rotation = Vector3.Lerp(motion.myTm.stRot, motion.myTm.edRot, easing);
            var distance = Mathf.Lerp(motion.myTm.stSca.x, motion.myTm.edSca.x, easing);
            var viewAngle = Mathf.Lerp(motion.myTm.stSca.y, motion.myTm.edSca.y, easing);

            var uoCamera = MyHelper.GetUOCamera();
            uoCamera.SetTargetPos(position);
            uoCamera.SetDistance(distance);
            uoCamera.SetAroundAngle(new Vector2(rotation.y, rotation.x));
            Camera.main.SetRotationZ(rotation.z);
            Camera.main.fieldOfView = viewAngle;

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
            trans["distance"].value = uOCamera.distance;
            trans["viewAngle"].value = Camera.main.fieldOfView;

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
                distance = trans["distance"].value,
                viewAngle = trans["viewAngle"].value
            };

            _timelineRows.Add(row);
        }

        private void BuildPlayData(bool forOutput)
        {
            _playData.motions.Clear();

            bool warpFrameEnabled = forOutput || !studioHack.isPoseEditing;

            foreach (var row in _timelineRows)
            {
                var motion = new CameraMotionData
                {
                    stFrame = row.frame,
                    edFrame = row.frame,
                    myTm = new MyTransform
                    {
                        stPos = row.position,
                        edPos = row.position,
                        stRot = row.rotation,
                        edRot = row.rotation,
                        stSca = new Vector3(row.distance, row.viewAngle, 0f),
                        edSca = new Vector3(row.distance, row.viewAngle, 0f)
                    }
                };
                _playData.motions.Add(motion);

                if (row.frame != 0)
                {
                    int prevIndex = _playData.motions.Count() - 2;
                    var prevMotion = _playData.motions[prevIndex];
                    if (row.frame - prevMotion.stFrame == 1 && warpFrameEnabled)
                    {
                        int stFrame = prevMotion.stFrame;
                        _playData.motions.Remove(prevMotion);
                        prevMotion = _playData.motions[prevIndex];
                        prevMotion.stFrame = stFrame;
                        prevMotion.edFrame = stFrame;
                    }
                    else
                    {
                        prevMotion.edFrame = row.frame;
                        prevMotion.myTm.edPos = row.position;
                        prevMotion.myTm.edRot = row.rotation;
                        prevMotion.myTm.edSca = new Vector3(row.distance, row.viewAngle, 0f);
                        prevMotion.easing = row.easing;
                    }
                }
            }
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
                builder.Append(row.distance.ToString("0.000") + ",");
                builder.Append(row.viewAngle.ToString("0.000") + ",");
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

        private int _targetMaidSlotNo = 0;
        private MaidPointType _targetMaidPoint = MaidPointType.Head;
        private int _targetModelIndex = 0;
        private FloatFieldValue[] _fieldValues = FloatFieldValue.CreateArray(
            new string[] { "X", "Y", "Z", "RX", "RY", "RZ", "距離", "FoV" }
        );

        private static readonly List<string> MaidPointTypeNames = new List<string>
        {
            "顔", "胸", "股", "尻"
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

            view.SetEnabled(studioHack.isPoseEditing);

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

            updateTransform |= view.DrawSliderValue(_fieldValues[3], prevAngles.x - 180f, prevAngles.x + 180f, 1f, 0f,
                angles.x,
                x => angles.x = x);

            updateTransform |= view.DrawSliderValue(_fieldValues[4], prevAngles.y - 180f, prevAngles.y + 180f, 1f, 0f,
                angles.y,
                y => angles.y = y);

            updateTransform |= view.DrawSliderValue(_fieldValues[5], prevAngles.z - 180f, prevAngles.z + 180f, 1f, 0f,
                angles.z,
                z => angles.z = z);

            updateTransform |= view.DrawValue(_fieldValues[6], 0.1f, 1f, 2f,
                distance,
                d => distance = d,
                d => distance += d);

            updateTransform |= view.DrawValue(_fieldValues[7], 0.1f, 1f, 35f,
                Camera.main.fieldOfView,
                a => Camera.main.fieldOfView = a,
                a => Camera.main.fieldOfView += a);

            view.DrawHorizontalLine(Color.gray);

            view.DrawLabel("対象設定", 70, 20);

            Action focusToMaid = () =>
            {
                var maidCache = maidManager.GetMaidCache(_targetMaidSlotNo);
                if (maidCache != null)
                {
                    var trans = _dcmMaidManager.GetMiadPatrsTrancform(maidCache.maid, _targetMaidPoint);
                    position = trans.position;
                    updateTransform = true;
                }
            };

            view.BeginLayout(GUIView.LayoutDirection.Horizontal);
            {
                view.DrawLabel("対象メイド", 70, 20);

                var newTargetMaidSlotNo = view.DrawSelectList(
                    maidManager.maidCaches,
                    (cache, index) => string.Format("{0}:{1}", index, cache.maid.status.fullNameJpStyle),
                    140, 20, _targetMaidSlotNo);
                
                if (newTargetMaidSlotNo != _targetMaidSlotNo)
                {
                    _targetMaidSlotNo = newTargetMaidSlotNo;
                    focusToMaid();
                }

                if (view.DrawButton("移動", 40, 20))
                {
                    focusToMaid();
                }
            }
            view.EndLayout();

            view.BeginLayout(GUIView.LayoutDirection.Horizontal);
            {
                view.DrawLabel("対象ポイント", 70, 20);

                var newTargetMaidPoint = (MaidPointType)view.DrawSelectList(
                    MaidPointTypeNames,
                    (name, index) => name,
                    80, 20, (int)_targetMaidPoint);
                
                if (newTargetMaidPoint != _targetMaidPoint)
                {
                    _targetMaidPoint = newTargetMaidPoint;
                    focusToMaid();
                }
            }
            view.EndLayout();

            Action focusToModel = () =>
            {
                var model = modelManager.GetModel(_targetModelIndex);
                if (model != null)
                {
                    var trans = model.transform;
                    position = trans.position;
                    updateTransform = true;
                }
            };

            view.BeginLayout(GUIView.LayoutDirection.Horizontal);
            {
                view.DrawLabel("対象モデル", 70, 20);

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
                    focusToModel();
                }

                if (view.DrawButton("移動", 40, 20))
                {
                    focusToModel();
                }
            }
            view.EndLayout();

            view.SetEnabled(true);

            if (updateTransform)
            {
                uoCamera.SetTargetPos(position);
                uoCamera.SetDistance(distance);
                uoCamera.SetAroundAngle(new Vector2(angles.y, angles.x));
                Camera.main.SetRotationZ(angles.z);
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