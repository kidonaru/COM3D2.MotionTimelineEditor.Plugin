using System;
using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    [TimelineLayerDesc("サブカメラ", 21)]
    public class SubCameraTimelineLayer : TimelineLayerBase
    {
        public override Type layerType => typeof(SubCameraTimelineLayer);
        public override string layerName => nameof(SubCameraTimelineLayer);

        public override bool isCameraLayer => true;

        public override List<string> allBoneNames => subCameraManager.subCameraNames;

        private static SubCameraManager subCameraManager => SubCameraManager.instance;

        private SubCameraData currentCamera;

        private SubCameraTimelineLayer(int slotNo) : base(slotNo)
        {
        }

        public static SubCameraTimelineLayer Create(int slotNo)
        {
            return new SubCameraTimelineLayer(0);
        }

        public override void Init()
        {
            // base.Init()内のInitMenuItemsでカメラを参照するため先に生成する
            subCameraManager.SetupCameras();

            base.Init();

            AddFirstBones(allBoneNames);

            SubCameraManager.onCameraAdded += OnCameraAdded;
            SubCameraManager.onCameraRemoved += OnCameraRemoved;
        }

        public override void Dispose()
        {
            base.Dispose();

            SubCameraManager.onCameraAdded -= OnCameraAdded;
            SubCameraManager.onCameraRemoved -= OnCameraRemoved;
        }

        public void OnCameraAdded(SubCameraData cameraData)
        {
            InitMenuItems();
            AddFirstBones(new List<string> { cameraData.name });
            ApplyCurrentFrame(true);
        }

        public void OnCameraRemoved(SubCameraData cameraData)
        {
            InitMenuItems();
            RemoveAllBones(new List<string> { cameraData.name });
            if (currentCamera == cameraData)
            {
                currentCamera = null;
            }
            ApplyCurrentFrame(true);
        }

        protected override void InitMenuItems()
        {
            allMenuItems.Clear();

            foreach (var cameraData in subCameraManager.subCameras)
            {
                var menuItem = new BoneMenuItem(cameraData.name, cameraData.displayName);
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

            if (!studioHackManager.isPoseEditing)
            {
                ApplyPlayData();
            }
        }

        protected override void ApplyPlayData()
        {
            if (!isCurrent && !config.isCameraSync)
            {
                return;
            }

            base.ApplyPlayData();
        }

        protected override void ApplyMotion(MotionData motion, float t, bool indexUpdated, MotionPlayData playData)
        {
            var cameraData = subCameraManager.GetOrCreateCamera(motion.name);
            if (cameraData == null || cameraData.camera == null)
            {
                return;
            }

            var start = motion.start as TransformDataSubCamera;
            var end = motion.end as TransformDataSubCamera;

            if (indexUpdated)
            {
                cameraData.visible = start.visible;
                cameraData.follow.maidSlotNo = start.maidSlotNo;
            }

            Vector3 position, eulerAngles;
            float fov;

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

                fov = PluginUtils.HermiteValue(
                    t0,
                    t1,
                    start.fovValue,
                    end.fovValue,
                    t);
            }
            else
            {
                float easing = CalcEasingValue(t, motion.easing);
                position = Vector3.Lerp(start.position, end.position, easing);
                eulerAngles = Vector3.Lerp(start.eulerAngles, end.eulerAngles, easing);
                fov = Mathf.Lerp(start.fov, end.fov, easing);
                t = easing;
            }

            var startViewport = start.viewport;
            var endViewport = end.viewport;
            var viewportRect = new Rect(
                Mathf.Lerp(startViewport.x, endViewport.x, t),
                Mathf.Lerp(startViewport.y, endViewport.y, t),
                Mathf.Lerp(startViewport.width, endViewport.width, t),
                Mathf.Lerp(startViewport.height, endViewport.height, t)
            );

            cameraData.position = position;
            cameraData.rotation = Quaternion.Euler(eulerAngles);
            cameraData.camera.fieldOfView = fov;
            cameraData.ApplyViewport(viewportRect);
        }

        public override void UpdateFrame(FrameData frame, bool initialEdit, bool force)
        {
            foreach (var cameraData in subCameraManager.subCameras)
            {
                if (cameraData == null || cameraData.camera == null)
                {
                    continue;
                }

                var cameraName = cameraData.name;

                var trans = CreateTransformData<TransformDataSubCamera>(cameraName);
                trans.position = cameraData.position;
                trans.eulerAngles = cameraData.rotation.eulerAngles;
                trans.easing = GetEasing(frame.frameNo, cameraName);
                trans.fov = cameraData.camera.fieldOfView;
                trans.viewport = cameraData.viewportRect;
                trans.maidSlotNo = cameraData.follow.maidSlotNo;
                trans.visible = cameraData.visible;

                var bone = frame.CreateBone(trans);
                frame.UpdateBone(bone);
            }
        }

        private GUIComboBox<SubCameraData> _cameraComboBox = new GUIComboBox<SubCameraData>
        {
            getName = (camera, _) => camera == null ? "未選択" : camera.displayName,
            buttonSize = new Vector2(100, 20),
            contentSize = new Vector2(150, 300),
        };

        private GUIComboBox<MaidCache> _maidComboBox = new GUIComboBox<MaidCache>
        {
            getName = (maidCache, _) => maidCache == null ? "なし" : maidCache.fullName,
            buttonSize = new Vector2(100, 20),
            contentSize = new Vector2(150, 300),
        };

        // 先頭に「なし」(null)を含む追従メイド選択肢
        private List<MaidCache> _followMaidItems = new List<MaidCache>();

        public override void DrawWindow(GUIView view)
        {
            view.SetEnabled(!view.IsComboBoxFocused() && studioHackManager.isPoseEditing);

            var subCameras = subCameraManager.subCameras;

            // カメラ破棄後の参照が残っている場合もリセットする
            if (currentCamera == null || currentCamera.camera == null)
            {
                currentCamera = subCameras.Count > 0 ? subCameras[0] : null;
            }

            view.BeginHorizontal();
            {
                _cameraComboBox.items = subCameras;
                _cameraComboBox.currentIndex = Mathf.Max(0, subCameras.IndexOf(currentCamera));
                _cameraComboBox.onSelected = (subCamera, index) =>
                {
                    currentCamera = subCamera;
                };
                _cameraComboBox.DrawButton(view);

                if (view.DrawButton("追加", 60, 20,
                        subCameras.Count < SubCameraManager.MaxSubCameraCount))
                {
                    var newCamera = subCameraManager.AddNewCamera();
                    if (newCamera != null)
                    {
                        currentCamera = newCamera;
                    }
                }

                if (view.DrawButton("削除", 60, 20,
                        subCameras.Count > SubCameraManager.MinSubCameraCount))
                {
                    subCameraManager.RemoveLastCamera();
                }
            }
            view.EndLayout();

            if (currentCamera == null || currentCamera.camera == null)
            {
                return;
            }

            var cameraData = currentCamera;
            var camera = cameraData.camera;
            var follow = cameraData.follow;

            view.DrawToggle("有効", cameraData.visible, 100, 20, newValue =>
            {
                cameraData.visible = newValue;
            });

            view.BeginHorizontal();
            {
                view.DrawLabel("追従メイド", 70, 20);

                _followMaidItems.Clear();
                _followMaidItems.Add(null);
                _followMaidItems.AddRange(maidManager.maidCaches);

                _maidComboBox.items = _followMaidItems;
                _maidComboBox.currentIndex = Mathf.Clamp(
                    follow.maidSlotNo + 1, 0, _followMaidItems.Count - 1);
                _maidComboBox.onSelected = (maidCache, index) =>
                {
                    follow.maidSlotNo = index - 1;
                };
                _maidComboBox.DrawButton(view);
            }
            view.EndLayout();

            var position = cameraData.position;
            var angles = cameraData.rotation.eulerAngles;
            var prevBone = GetPrevBone(timelineManager.currentFrameNo, cameraData.name);
            var prevAngles = prevBone != null ? prevBone.transform.eulerAngles : Vector3.zero;
            angles = TransformDataBase.GetFixedEulerAngles(angles, prevAngles);
            var updateTransform = false;

            var initialPosition = Vector3.zero;
            var initialEulerAngles = Vector3.zero;

            var positionLabel = follow.isFollow ? "オフセット" : "位置";
            view.DrawLabel(positionLabel, 100, 20);

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
            view.DrawLabel("ビューポート設定", 100, 20);

            var viewportRect = cameraData.viewportRect;
            var updateViewport = false;

            updateViewport |= view.DrawSliderValue(
                new GUIView.SliderOption
                {
                    label = "X",
                    labelWidth = 30,
                    min = 0,
                    max = 1,
                    step = 0.01f,
                    defaultValue = SubCameraManager.DefaultViewport.x,
                    value = viewportRect.x,
                    onChanged = x => viewportRect.x = x,
                });

            updateViewport |= view.DrawSliderValue(
                new GUIView.SliderOption
                {
                    label = "Y",
                    labelWidth = 30,
                    min = 0,
                    max = 1,
                    step = 0.01f,
                    defaultValue = SubCameraManager.DefaultViewport.y,
                    value = viewportRect.y,
                    onChanged = y => viewportRect.y = y,
                });

            updateViewport |= view.DrawSliderValue(
                new GUIView.SliderOption
                {
                    label = "幅",
                    labelWidth = 30,
                    min = 0,
                    max = 1,
                    step = 0.01f,
                    defaultValue = SubCameraManager.DefaultViewport.width,
                    value = viewportRect.width,
                    onChanged = w => viewportRect.width = w,
                });

            updateViewport |= view.DrawSliderValue(
                new GUIView.SliderOption
                {
                    label = "高さ",
                    labelWidth = 30,
                    min = 0,
                    max = 1,
                    step = 0.01f,
                    defaultValue = SubCameraManager.DefaultViewport.height,
                    value = viewportRect.height,
                    onChanged = h => viewportRect.height = h,
                });

            if (updateViewport)
            {
                subCameraManager.UpdateCameraViewport(cameraData.name, viewportRect);
            }

            if (updateTransform)
            {
                cameraData.position = position;
                cameraData.rotation = Quaternion.Euler(angles);
            }
        }

        public override TransformType GetTransformType(string name)
        {
            return TransformType.SubCamera;
        }
    }
}
