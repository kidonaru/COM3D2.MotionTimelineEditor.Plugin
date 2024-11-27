using System;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    [TimelineLayerDesc("ステージライト", 42)]
    public class StageLightTimelineLayer : TimelineLayerBase
    {
        public override string className
        {
            get
            {
                return typeof(StageLightTimelineLayer).Name;
            }
        }

        private List<string> _allBoneNames = null;

        public override List<string> allBoneNames
        {
            get
            {
                if (_allBoneNames == null)
                {
                    _allBoneNames = new List<string>(stageLightManager.lightNames.Count + stageLightManager.controllerNames.Count);
                    _allBoneNames.AddRange(stageLightManager.lightNames);
                    _allBoneNames.AddRange(stageLightManager.controllerNames);
                }
                return _allBoneNames;
            }
        }

        private Dictionary<string, List<BoneData>> _lightTimelineRowsMap = new Dictionary<string, List<BoneData>>();
        private Dictionary<string, MotionPlayData> _lightPlayDataMap = new Dictionary<string, MotionPlayData>();

        private Dictionary<string, List<BoneData>> _controllerTimelineRowsMap = new Dictionary<string, List<BoneData>>();
        private Dictionary<string, MotionPlayData> _controllerPlayDataMap = new Dictionary<string, MotionPlayData>();

        private StageLightTimelineLayer(int slotNo) : base(slotNo)
        {
        }

        public static StageLightTimelineLayer Create(int slotNo)
        {
            return new StageLightTimelineLayer(0);
        }

        public static bool ValidateLayer()
        {
            return bundleManager.IsValid();
        }

        public override void Init()
        {
            base.Init();

            StageLightManager.onSetup += OnLightSetup;
            StageLightManager.onControllerAdded += OnControllerAdded;
            StageLightManager.onControllerRemoved += OnControllerRemoved;
            StageLightManager.onLightAdded += OnLightAdded;
            StageLightManager.onLightRemoved += OnLightRemoved;

            InitMenuItems();
        }

        protected override void InitMenuItems()
        {
            _allBoneNames = null;
            allMenuItems.Clear();

            foreach (var controller in stageLightManager.controllers)
            {
                var name = "StageLightGroup (" + controller.groupIndex + ")";
                var displayName = "グループ (" + controller.groupIndex + ")";
                var setMenuItem = new BoneSetMenuItem(name, displayName);
                allMenuItems.Add(setMenuItem);

                {
                    var menuItem = new BoneMenuItem(controller.name, controller.displayName);
                    setMenuItem.AddChild(menuItem);
                }

                foreach (var light in controller.lights)
                {
                    var menuItem = new BoneMenuItem(light.name, light.displayName);
                    setMenuItem.AddChild(menuItem);
                }
            }
        }

        public override void Dispose()
        {
            base.Dispose();

            StageLightManager.onSetup -= OnLightSetup;
            StageLightManager.onControllerAdded -= OnControllerAdded;
            StageLightManager.onControllerRemoved -= OnControllerRemoved;
            StageLightManager.onLightAdded -= OnLightAdded;
            StageLightManager.onLightRemoved -= OnLightRemoved;
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

            foreach (var playData in _lightPlayDataMap.Values)
            {
                playData.Update(playingFrameNoFloat);

                var current = playData.current;
                if (current != null)
                {
                    ApplyLightMotion(current, playData.lerpFrame);
                }

                //PluginUtils.LogDebug("ApplyPlayData: lightName={0} lerpTime={1}, listIndex={2}", lightName, playData.lerpTime, playData.listIndex);
            }

            foreach (var playData in _controllerPlayDataMap.Values)
            {
                playData.Update(playingFrameNoFloat);

                var current = playData.current;
                if (current != null)
                {
                    ApplyControllerMotion(current, playData.lerpFrame);
                }

                //PluginUtils.LogDebug("ApplyPlayData: lightName={0} lerpTime={1}, listIndex={2}", lightName, playData.lerpTime, playData.listIndex);
            }
        }

        private void ApplyLightMotion(MotionData motion, float lerpTime)
        {
            var light = stageLightManager.GetLight(motion.name);
            if (light == null)
            {
                return;
            }

            var transform = light.transform;
            var controller = light.controller;
            if (transform == null || controller == null)
            {
                return;
            }

            var start = motion.start as TransformDataStageLight;
            var end = motion.end as TransformDataStageLight;

            var t0 = motion.stFrame * timeline.frameDuration;
            var t1 = motion.edFrame * timeline.frameDuration;

            if (!controller.autoVisible)
            {
                light.visible = start.visible;
            }

            if (!controller.autoPosition)
            {
                transform.localPosition = PluginUtils.HermiteValues(
                    t0,
                    t1,
                    start.positionValues,
                    end.positionValues,
                    lerpTime
                ).ToVector3();
            }

            if (!controller.autoRotation)
            {
                transform.localRotation = PluginUtils.HermiteValues(
                    t0,
                    t1,
                    start.rotationValues,
                    end.rotationValues,
                    lerpTime
                ).ToQuaternion();
            }

            if (!controller.autoColor)
            {
                light.color = PluginUtils.HermiteValues(
                    t0,
                    t1,
                    start.colorValues,
                    end.colorValues,
                    lerpTime
                ).ToColor();
            }

            if (!controller.autoLightInfo)
            {
                light.spotAngle = PluginUtils.HermiteValue(
                    t0,
                    t1,
                    start.spotAngleValue,
                    end.spotAngleValue,
                    lerpTime);

                light.spotRange = PluginUtils.HermiteValue(
                    t0,
                    t1,
                    start.spotRangeValue,
                    end.spotRangeValue,
                    lerpTime);

                light.rangeMultiplier = start.rangeMultiplier;
                light.falloffExp = start.falloffExp;
                light.noiseStrength = start.noiseStrength;
                light.noiseScale = start.noiseScale;
                light.coreRadius = start.coreRadius;
                light.offsetRange = start.offsetRange;
                light.segmentAngle = start.segmentAngle;
                light.segmentRange = start.segmentRange;
            }
        }

        private void ApplyControllerMotion(MotionData motion, float lerpTime)
        {
            var controller = stageLightManager.GetController(motion.name);
            if (controller == null)
            {
                return;
            }

            var start = motion.start as TransformDataStageLightController;
            var end = motion.end as TransformDataStageLightController;

            var t0 = motion.stFrame * timeline.frameDuration;
            var t1 = motion.edFrame * timeline.frameDuration;

            controller.visible = start.visible;

            controller.positionMin = PluginUtils.HermiteValues(
                t0,
                t1,
                start.positionValues,
                end.positionValues,
                lerpTime
            ).ToVector3();

            controller.positionMax = PluginUtils.HermiteValues(
                t0,
                t1,
                start.subPositionValues,
                end.subPositionValues,
                lerpTime
            ).ToVector3();

            controller.rotationMin = PluginUtils.HermiteValues(
                t0,
                t1,
                start.eulerAnglesValues,
                end.eulerAnglesValues,
                lerpTime
            ).ToVector3();

            controller.rotationMax = PluginUtils.HermiteValues(
                t0,
                t1,
                start.subEulerAnglesValues,
                end.subEulerAnglesValues,
                lerpTime
            ).ToVector3();

            controller.colorMin = PluginUtils.HermiteValues(
                t0,
                t1,
                start.colorValues,
                end.colorValues,
                lerpTime
            ).ToColor();

            controller.colorMax = PluginUtils.HermiteValues(
                t0,
                t1,
                start.subColorValues,
                end.subColorValues,
                lerpTime
            ).ToColor();

            var lightInfo = controller.lightInfo;

            lightInfo.spotAngle = PluginUtils.HermiteValue(
                t0,
                t1,
                start.spotAngleValue,
                end.spotAngleValue,
                lerpTime);

            lightInfo.spotRange = PluginUtils.HermiteValue(
                t0,
                t1,
                start.spotRangeValue,
                end.spotRangeValue,
                lerpTime);

            lightInfo.rangeMultiplier = start.rangeMultiplier;
            lightInfo.falloffExp = start.falloffExp;
            lightInfo.noiseStrength = start.noiseStrength;
            lightInfo.noiseScale = start.noiseScale;
            lightInfo.coreRadius = start.coreRadius;
            lightInfo.offsetRange = start.offsetRange;
            lightInfo.segmentAngle = start.segmentAngle;
            lightInfo.segmentRange = start.segmentRange;

            controller.autoPosition = start.autoPosition;
            controller.autoRotation = start.autoRotation;
            controller.autoColor = start.autoColor;
            controller.autoLightInfo = start.autoLightInfo;
            controller.autoVisible = start.autoVisible;
        }

        public void OnLightSetup()
        {
            InitMenuItems();
            AddFirstBones(allBoneNames);
            ApplyCurrentFrame(true);
        }

        public void OnControllerAdded(string controllerName)
        {
            InitMenuItems();
            AddFirstBones(new List<string> { controllerName });
            ApplyCurrentFrame(true);
        }

        public void OnControllerRemoved(string controllerName)
        {
            InitMenuItems();
            RemoveAllBones(new List<string> { controllerName });
            ApplyCurrentFrame(true);
        }

        public void OnLightAdded(string lightName)
        {
            InitMenuItems();
            AddFirstBones(new List<string> { lightName });
            ApplyCurrentFrame(true);
        }

        public void OnLightRemoved(string lightName)
        {
            InitMenuItems();
            RemoveAllBones(new List<string> { lightName });
            ApplyCurrentFrame(true);
        }

        public override void UpdateFrame(FrameData frame)
        {
            foreach (var light in stageLightManager.lights)
            {
                if (light == null || light.transform == null)
                {
                    continue;
                }

                var lightName = light.name;

                var trans = CreateTransformData<TransformDataStageLight>(lightName);
                trans.FromStageLight(light);

                var bone = frame.CreateBone(trans);
                frame.UpdateBone(bone);
            }

            foreach (var controller in stageLightManager.controllers)
            {
                if (controller == null || controller.transform == null)
                {
                    continue;
                }

                var controllerName = controller.name;

                var trans = CreateTransformData<TransformDataStageLightController>(controllerName);
                trans.FromStageLightController(controller);

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

        private void AppendTimelineRow(FrameData frame)
        {
            var isLastFrame = frame.frameNo == maxFrameNo;
            foreach (var name in stageLightManager.lightNames)
            {
                var bone = frame.GetBone(name);
                _lightTimelineRowsMap.AppendBone(bone, isLastFrame);
            }

            foreach (var name in stageLightManager.controllerNames)
            {
                var bone = frame.GetBone(name);
                _controllerTimelineRowsMap.AppendBone(bone, isLastFrame);
            }
        }

        private void BuildPlayData(bool forOutput)
        {
            BuildPlayDataFromBonesMap(
                _lightTimelineRowsMap,
                _lightPlayDataMap,
                timeline.singleFrameType);

            BuildPlayDataFromBonesMap(
                _controllerTimelineRowsMap,
                _controllerPlayDataMap,
                timeline.singleFrameType);
        }

        protected override byte[] GetAnmBinaryInternal(bool forOutput, int startFrameNo, int endFrameNo)
        {
            _lightTimelineRowsMap.ClearBones();
            _controllerTimelineRowsMap.ClearBones();

            foreach (var keyFrame in keyFrames)
            {
                AppendTimelineRow(keyFrame);
            }

            AppendTimelineRow(_dummyLastFrame);

            BuildPlayData(forOutput);

            return null;
        }

        public override void OutputDCM(XElement songElement)
        {
            // do nothing
        }

        private GUIComboBox<StageLightController> _controllerComboBox = new GUIComboBox<StageLightController>
        {
            getName = (light, index) => light.displayName,
            labelWidth = 70,
            buttonSize = new Vector2(150, 20),
            contentSize = new Vector2(150, 300),
        };

        private GUIComboBox<StageLight> _lightComboBox = new GUIComboBox<StageLight>
        {
            getName = (light, index) => light.displayName,
            labelWidth = 70,
            buttonSize = new Vector2(150, 20),
            contentSize = new Vector2(150, 300),
        };

        private GUIComboBox<StageLight> _copyToLightComboBox = new GUIComboBox<StageLight>
        {
            getName = (light, index) => light.displayName,
            labelWidth = 70,
            buttonSize = new Vector2(150, 20),
            contentSize = new Vector2(150, 300),
        };

        private ColorFieldCache _color1FieldValue = new ColorFieldCache("Color1", true);
        private ColorFieldCache _color2FieldValue = new ColorFieldCache("Color2", true);

        private enum TabType
        {
            一括,
            個別,
        }

        private TabType _tabType = TabType.一括;

        public override void DrawWindow(GUIView view)
        {
            _tabType = view.DrawTabs(_tabType, 50, 20);

            switch (_tabType)
            {
                case TabType.一括:
                    DrawStageLightControllEdit(view);
                    break;
                case TabType.個別:
                    DrawStageLightEdit(view);
                    break;
            }

            view.DrawComboBox();
        }
        
        public void DrawStageLightControllEdit(GUIView view)
        {
            view.SetEnabled(!view.IsComboBoxFocused() && studioHack.isPoseEditing);

            view.BeginHorizontal();
            {
                view.margin = 0;

                view.DrawLabel("コントローラー数", view.labelWidth, 20);

                view.DrawIntField(new GUIView.IntFieldOption
                {
                    value = stageLightManager.controllers.Count,
                    width = view.viewRect.width - (view.labelWidth + 40 + view.padding.x * 2),
                    height = 20,
                });

                if (view.DrawButton("-", 20, 20))
                {
                    stageLightManager.RemoveController(true);
                }
                if (view.DrawButton("+", 20, 20))
                {
                    stageLightManager.AddController(true);
                }

                view.margin = GUIView.defaultMargin;
            }
            view.EndLayout();

            var controllers = stageLightManager.controllers;
            if (controllers.Count == 0)
            {
                view.DrawLabel("コントローラーが存在しません", 200, 20);
                return;
            }

            _controllerComboBox.items = controllers;
            _controllerComboBox.DrawButton("操作対象", view);

            var controller = _controllerComboBox.currentItem;
            if (controller == null)
            {
                view.DrawLabel("コントローラーを選択してください", 200, 20);
                return;
            }
            
            view.BeginHorizontal();
            {
                view.margin = 0;

                view.DrawLabel("ライト数", view.labelWidth, 20);

                view.DrawIntField(new GUIView.IntFieldOption
                {
                    value = controller.lights.Count,
                    width = view.viewRect.width - (view.labelWidth + 40 + view.padding.x * 2),
                    height = 20,
                });

                if (view.DrawButton("-", 20, 20))
                {
                    stageLightManager.RemoveLight(controller.groupIndex, true);
                }
                if (view.DrawButton("+", 20, 20))
                {
                    stageLightManager.AddLight(controller.groupIndex, true);
                }

                view.margin = GUIView.defaultMargin;
            }
            view.EndLayout();

            view.DrawHorizontalLine(Color.gray);
            view.AddSpace(5);

            view.SetEnabled(!view.IsComboBoxFocused());
            view.BeginScrollView();

            view.SetEnabled(!view.IsComboBoxFocused() && studioHack.isPoseEditing);

            view.DrawToggle("一括表示設定", controller.autoVisible, 200, 20, newValue =>
            {
                controller.autoVisible = newValue;
            });

            if (controller.autoVisible)
            {
                view.DrawToggle("表示", controller.visible, 120, 20, newValue =>
                {
                    controller.visible = newValue;
                });
            }

            view.DrawToggle("一括位置設定", controller.autoPosition, 200, 20, newValue =>
            {
                controller.autoPosition = newValue;
            });

            if (controller.autoPosition)
            {
                var initialPosition = new Vector3(-15f, 10f, 0f);
                var transformCache = view.GetTransformCache(null);
                transformCache.position = controller.positionMin;

                view.DrawLabel("最小位置", 200, 20);

                var updateTransform = DrawPosition(
                    view,
                    transformCache,
                    TransformEditType.全て,
                    initialPosition);

                if (updateTransform)
                {
                    controller.positionMin = transformCache.position;
                }

                initialPosition = new Vector3(15f, 10f, 0f);
                transformCache = view.GetTransformCache(null);
                transformCache.position = controller.positionMax;

                view.DrawLabel("最大位置", 200, 20);

                updateTransform = DrawPosition(
                    view,
                    transformCache,
                    TransformEditType.全て,
                    initialPosition);

                if (updateTransform)
                {
                    controller.positionMax = transformCache.position;
                }
            }

            view.DrawToggle("一括回転設定", controller.autoRotation, 200, 20, newValue =>
            {
                controller.autoRotation = newValue;
            });

            if (controller.autoRotation)
            {
                var initialEulerAngles = new Vector3(90f, 0f, 0f);
                var transformCache = view.GetTransformCache(null);
                transformCache.eulerAngles = controller.rotationMin;

                view.DrawLabel("最小回転", 200, 20);

                var updateTransform = DrawEulerAngles(
                    view,
                    transformCache,
                    TransformEditType.全て,
                    controller.name,
                    initialEulerAngles);

                if (updateTransform)
                {
                    controller.rotationMin = transformCache.eulerAngles;
                }

                transformCache = view.GetTransformCache(null);
                transformCache.eulerAngles = controller.rotationMax;

                view.DrawLabel("最大回転", 200, 20);

                updateTransform = DrawEulerAngles(
                    view,
                    transformCache,
                    TransformEditType.全て,
                    controller.name,
                    initialEulerAngles);

                if (updateTransform)
                {
                    controller.rotationMax = transformCache.eulerAngles;
                }
            }

            view.DrawToggle("一括色設定", controller.autoColor, 200, 20, newValue =>
            {
                controller.autoColor = newValue;
            });

            if (controller.autoColor)
            {
                view.DrawLabel("最小色", 200, 20);

                view.DrawColor(
                    _color1FieldValue,
                    controller.colorMin,
                    Color.white,
                    c => controller.colorMin = c);

                view.DrawLabel("最大色", 200, 20);

                view.DrawColor(
                    _color2FieldValue,
                    controller.colorMax,
                    Color.white,
                    c => controller.colorMax = c);
            }

            view.DrawToggle("一括ライト情報設定", controller.autoLightInfo, 200, 20, newValue =>
            {
                controller.autoLightInfo = newValue;
            });

            if (controller.autoLightInfo)
            {
                var lightInfo = controller.lightInfo;
                var updateTransform = false;

                updateTransform |= view.DrawSliderValue(new GUIView.SliderOption
                {
                    label = "角度",
                    labelWidth = 30,
                    min = 1f,
                    max = 179f,
                    step = 0.1f,
                    defaultValue = 10f,
                    value = lightInfo.spotAngle,
                    onChanged = newValue => lightInfo.spotAngle = newValue,
                });

                updateTransform |= view.DrawSliderValue(new GUIView.SliderOption
                {
                    label = "範囲",
                    labelWidth = 30,
                    min = 0f,
                    max = 100f,
                    step = 0.1f,
                    defaultValue = 10f,
                    value = lightInfo.spotRange,
                    onChanged = newValue => lightInfo.spotRange = newValue,
                });

                updateTransform |= view.DrawSliderValue(new GUIView.SliderOption
                {
                    label = "範囲補正",
                    labelWidth = 30,
                    min = 0f,
                    max = 1f,
                    step = 0.01f,
                    defaultValue = 0.8f,
                    value = lightInfo.rangeMultiplier,
                    onChanged = newValue => lightInfo.rangeMultiplier = newValue,
                });

                updateTransform |= view.DrawSliderValue(new GUIView.SliderOption
                {
                    label = "減衰指数",
                    labelWidth = 30,
                    min = 0f,
                    max = 1f,
                    step = 0.01f,
                    defaultValue = 0.5f,
                    value = lightInfo.falloffExp,
                    onChanged = newValue => lightInfo.falloffExp = newValue,
                });

                updateTransform |= view.DrawSliderValue(new GUIView.SliderOption
                {
                    label = "ﾉｲｽﾞ強度",
                    labelWidth = 30,
                    min = 0f,
                    max = 1f,
                    step = 0.01f,
                    defaultValue = 0.1f,
                    value = lightInfo.noiseStrength,
                    onChanged = newValue => lightInfo.noiseStrength = newValue,
                });

                updateTransform |= view.DrawSliderValue(new GUIView.SliderOption
                {
                    label = "ﾉｲｽﾞｻｲｽﾞ",
                    labelWidth = 30,
                    min = 1f,
                    max = 100f,
                    step = 0.1f,
                    defaultValue = 10f,
                    value = lightInfo.noiseScale,
                    onChanged = newValue => lightInfo.noiseScale = newValue,
                });

                updateTransform |= view.DrawSliderValue(new GUIView.SliderOption
                {
                    label = "中心半径",
                    labelWidth = 30,
                    min = 0f,
                    max = 1f,
                    step = 0.01f,
                    defaultValue = 0.8f,
                    value = lightInfo.coreRadius,
                    onChanged = newValue => lightInfo.coreRadius = newValue,
                });

                updateTransform |= view.DrawSliderValue(new GUIView.SliderOption
                {
                    label = "ｵﾌｾｯﾄ範囲",
                    labelWidth = 30,
                    min = 0f,
                    max = 10f,
                    step = 0.1f,
                    defaultValue = 0.5f,
                    value = lightInfo.offsetRange,
                    onChanged = newValue => lightInfo.offsetRange = newValue,
                });

                updateTransform |= view.DrawSliderValue(new GUIView.SliderOption
                {
                    label = "分割角度",
                    labelWidth = 30,
                    min = 0.1f,
                    max = 10f,
                    step = 0.1f,
                    defaultValue = 1f,
                    value = lightInfo.segmentAngle,
                    onChanged = newValue => lightInfo.segmentAngle = newValue,
                });

                updateTransform |= view.DrawSliderValue(new GUIView.SliderOption
                {
                    label = "分割範囲",
                    labelWidth = 30,
                    fieldType = FloatFieldType.Int,
                    min = 1,
                    max = 64,
                    step = 1,
                    defaultValue = 10,
                    value = lightInfo.segmentRange,
                    onChanged = newValue => lightInfo.segmentRange = (int) newValue,
                });
            }

            view.SetEnabled(!view.IsComboBoxFocused());
            view.EndScrollView();
        }

        public void DrawStageLightEdit(GUIView view)
        {
            view.SetEnabled(!view.IsComboBoxFocused());

            var controllers = stageLightManager.controllers;
            if (controllers.Count == 0)
            {
                view.DrawLabel("コントローラーが存在しません", 200, 20);
                return;
            }

            _controllerComboBox.items = controllers;
            _controllerComboBox.DrawButton("操作対象", view);

            var controller = _controllerComboBox.currentItem;
            if (controller == null)
            {
                view.DrawLabel("コントローラーを選択してください", 200, 20);
                return;
            }

            var lights = controller.lights;
            if (lights.Count == 0)
            {
                view.DrawLabel("ライトが存在しません", 200, 20);
                return;
            }

            _lightComboBox.items = lights;
            _lightComboBox.DrawButton("操作対象", view);

            var light = _lightComboBox.currentItem;

            if (light == null || light.transform == null)
            {
                view.DrawLabel("ライトを選択してください", 200, 20);
                return;
            }

            view.DrawHorizontalLine(Color.gray);

            view.AddSpace(5);

            view.BeginScrollView();

            view.SetEnabled(!view.IsComboBoxFocused() && studioHack.isPoseEditing);

            view.DrawLabel(light.displayName, 200, 20);

            if (!controller.autoVisible)
            {
                view.DrawToggle("表示", light.visible, 120, 20, newValue =>
                {
                    light.visible = newValue;
                });
            }

            var transform = light.transform;
            var transformCache = view.GetTransformCache(transform);
            var position = transform.localPosition;
            var initialPosition = StageLight.DefaultPosition;
            var initialEulerAngles = StageLight.DefaultEulerAngles;
            var initialScale = Vector3.one;
            var updateTransform = false;
            var editType = TransformEditType.全て;

            if (!controller.autoPosition)
            {
                updateTransform |= DrawPosition(view, transformCache, editType, initialPosition);
            }

            if (!controller.autoRotation)
            {
                updateTransform |= DrawEulerAngles(view, transformCache, editType, light.name, initialEulerAngles);
            }

            if (!controller.autoColor)
            {
                updateTransform |= view.DrawColor(
                    _color1FieldValue,
                    light.color,
                    Color.white,
                    c => light.color = c);
            }

            if (!controller.autoLightInfo)
            {
                updateTransform |= view.DrawSliderValue(new GUIView.SliderOption
                {
                    label = "角度",
                    labelWidth = 30,
                    min = 1f,
                    max = 179f,
                    step = 0.1f,
                    defaultValue = 10f,
                    value = light.spotAngle,
                    onChanged = newValue => light.spotAngle = newValue,
                });

                updateTransform |= view.DrawSliderValue(new GUIView.SliderOption
                {
                    label = "範囲",
                    labelWidth = 30,
                    min = 0f,
                    max = 100f,
                    step = 0.1f,
                    defaultValue = 10f,
                    value = light.spotRange,
                    onChanged = newValue => light.spotRange = newValue,
                });

                updateTransform |= view.DrawSliderValue(new GUIView.SliderOption
                {
                    label = "範囲補正",
                    labelWidth = 30,
                    min = 0f,
                    max = 1f,
                    step = 0.01f,
                    defaultValue = 0.8f,
                    value = light.rangeMultiplier,
                    onChanged = newValue => light.rangeMultiplier = newValue,
                });

                updateTransform |= view.DrawSliderValue(new GUIView.SliderOption
                {
                    label = "減衰指数",
                    labelWidth = 30,
                    min = 0f,
                    max = 1f,
                    step = 0.01f,
                    defaultValue = 0.5f,
                    value = light.falloffExp,
                    onChanged = newValue => light.falloffExp = newValue,
                });

                updateTransform |= view.DrawSliderValue(new GUIView.SliderOption
                {
                    label = "ﾉｲｽﾞ強度",
                    labelWidth = 30,
                    min = 0f,
                    max = 1f,
                    step = 0.01f,
                    defaultValue = 0.1f,
                    value = light.noiseStrength,
                    onChanged = newValue => light.noiseStrength = newValue,
                });

                updateTransform |= view.DrawSliderValue(new GUIView.SliderOption
                {
                    label = "ﾉｲｽﾞｻｲｽﾞ",
                    labelWidth = 30,
                    min = 1f,
                    max = 100f,
                    step = 0.1f,
                    defaultValue = 10f,
                    value = light.noiseScale,
                    onChanged = newValue => light.noiseScale = newValue,
                });

                updateTransform |= view.DrawSliderValue(new GUIView.SliderOption
                {
                    label = "中心半径",
                    labelWidth = 30,
                    min = 0f,
                    max = 1f,
                    step = 0.01f,
                    defaultValue = 0.8f,
                    value = light.coreRadius,
                    onChanged = newValue => light.coreRadius = newValue,
                });

                updateTransform |= view.DrawSliderValue(new GUIView.SliderOption
                {
                    label = "ｵﾌｾｯﾄ範囲",
                    labelWidth = 30,
                    min = 0f,
                    max = 10f,
                    step = 0.1f,
                    defaultValue = 0.5f,
                    value = light.offsetRange,
                    onChanged = newValue => light.offsetRange = newValue,
                });

                updateTransform |= view.DrawSliderValue(new GUIView.SliderOption
                {
                    label = "分割角度",
                    labelWidth = 30,
                    min = 0.1f,
                    max = 10f,
                    step = 0.1f,
                    defaultValue = 1f,
                    value = light.segmentAngle,
                    onChanged = newValue => light.segmentAngle = newValue,
                });

                updateTransform |= view.DrawSliderValue(new GUIView.SliderOption
                {
                    label = "分割範囲",
                    labelWidth = 30,
                    fieldType = FloatFieldType.Int,
                    min = 1,
                    max = 64,
                    step = 1,
                    defaultValue = 10,
                    value = light.segmentRange,
                    onChanged = newValue => light.segmentRange = (int) newValue,
                });
            }

            view.DrawHorizontalLine(Color.gray);

            {
                _copyToLightComboBox.items = lights;
                _copyToLightComboBox.DrawButton("コピー先", view);

                var copyToLight = _copyToLightComboBox.currentItem;

                if (view.DrawButton("コピー", 60, 20))
                {
                    if (copyToLight != null && copyToLight != light)
                    {
                        copyToLight.CopyFrom(light);
                    }
                }

                view.DrawHorizontalLine(Color.gray);
                view.AddSpace(5);
            }

            view.SetEnabled(!view.IsComboBoxFocused());
            view.EndScrollView();
        }

        public override TransformType GetTransformType(string name)
        {
            if (name.StartsWith("StageLightController", StringComparison.Ordinal))
            {
                return TransformType.StageLightController;
            }
            else
            {
                return TransformType.StageLight;
            }
        }
    }
}