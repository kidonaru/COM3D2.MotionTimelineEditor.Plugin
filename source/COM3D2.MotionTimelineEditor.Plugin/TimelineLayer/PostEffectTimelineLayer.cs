using System;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    [TimelineLayerDesc("ポストエフェクト", 44)]
    public class PostEffectTimelineLayer : TimelineLayerBase
    {
        public override string className => typeof(PostEffectTimelineLayer).Name;

        public override bool isPostEffectLayer => true;

        private List<string> _allBoneNames = null;
        public override List<string> allBoneNames
        {
            get
            {
                if (_allBoneNames == null)
                {
                    _allBoneNames = new List<string>(1 + timeline.paraffinCount);
                    _allBoneNames.Add("DepthOfField");
                    _allBoneNames.AddRange(paraffinNames);
                    _allBoneNames.AddRange(distanceFogNames);
                }
                return _allBoneNames;
            }
        }

        private static PostEffectManager postEffectManager => PostEffectManager.instance;

        private PostEffectTimelineLayer(int slotNo) : base(slotNo)
        {
        }

        public static PostEffectTimelineLayer Create(int slotNo)
        {
            return new PostEffectTimelineLayer(0);
        }

        public override void Init()
        {
            base.Init();
            InitParrifinEffect();
            InitDistanceFogEffect();
            AddFirstBones(allBoneNames);
        }

        private void InitParrifinEffect()
        {
            while (postEffectManager.GetParaffinCount() < timeline.paraffinCount)
            {
                postEffectManager.AddParaffinData();
            }
            while (postEffectManager.GetParaffinCount() > timeline.paraffinCount)
            {
                postEffectManager.RemoveParaffinData();
            }
        }

        private void InitDistanceFogEffect()
        {
            while (postEffectManager.GetDistanceFogCount() < timeline.distanceFogCount)
            {
                postEffectManager.AddDistanceFogData();
            }
            while (postEffectManager.GetDistanceFogCount() > timeline.distanceFogCount)
            {
                postEffectManager.RemoveDistanceFogData();
            }
        }

        protected override void InitMenuItems()
        {
            allMenuItems.Clear();

            foreach (var effectName in allBoneNames)
            {
                var jpName = PostEffectUtils.ToJpName(effectName);
                var menuItem = new BoneMenuItem(effectName, jpName);
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

            var boneCount = 1 + timeline.paraffinCount + timeline.distanceFogCount;
            if (allBoneNames.Count != boneCount)
            {
                _allBoneNames = null;
                InitParrifinEffect();
                InitDistanceFogEffect();
                InitMenuItems();
                AddFirstBones(allBoneNames);
            }

            if (!studioHack.isPoseEditing)
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
            if (!isCurrent && !config.isPostEffectSync)
            {
                postEffectManager.DisableAllEffects();
                return;
            }

            base.ApplyPlayData();
        }

        protected override void ApplyMotion(MotionData motion, float t, bool indexUpdated)
        {
            switch (motion.start.type)
            {
                case TransformType.DepthOfField:
                    ApplyDepthOfField(motion, t);
                    break;
                case TransformType.Paraffin:
                    ApplyParaffin(motion, t);
                    break;
                case TransformType.DistanceFog:
                    ApplyDistanceFog(motion, t);
                    break;
            }
        }

        private void ApplyDepthOfField(MotionData motion, float t)
        {
            var start = motion.start as TransformDataDepthOfField;
            var end = motion.end as TransformDataDepthOfField;

            float easingTime = CalcEasingValue(t, start.easing);
            var depthOfField = DepthOfFieldData.Lerp(start.depthOfField, end.depthOfField, easingTime);

            postEffectManager.ApplyDepthOfField(depthOfField);
        }

        private void ApplyParaffin(MotionData motion, float t)
        {
            var start = motion.start as TransformDataParaffin;
            var end = motion.end as TransformDataParaffin;

            float easingTime = CalcEasingValue(t, start.easing);
            var paraffin = ColorParaffinData.Lerp(start.paraffin, end.paraffin, easingTime);

            var index = start.index;
            postEffectManager.ApplyParaffin(index, paraffin);
        }

        private void ApplyDistanceFog(MotionData motion, float t)
        {
            var start = motion.start as TransformDataDistanceFog;
            var end = motion.end as TransformDataDistanceFog;

            float easingTime = CalcEasingValue(t, start.easing);
            var distanceFog = DistanceFogData.Lerp(start.distanceFog, end.distanceFog, easingTime);

            var index = start.index;
            postEffectManager.ApplyDistanceFog(index, distanceFog);
        }

        public override void UpdateFrame(FrameData frame)
        {
            foreach (var effectName in allBoneNames)
            {
                var effectType = PostEffectUtils.GetEffectType(effectName);

                switch (effectType)
                {
                    case PostEffectType.DepthOfField:
                    {
                        var trans = CreateTransformData<TransformDataDepthOfField>(effectName);
                        trans.depthOfField = postEffectManager.GetDepthOfFieldData();
                        trans.easing = GetEasing(frame.frameNo, effectName);

                        var bone = frame.CreateBone(trans);
                        frame.UpdateBone(bone);
                        break;
                    }
                    case PostEffectType.Paraffin:
                    {
                        var trans = CreateTransformData<TransformDataParaffin>(effectName);
                        trans.paraffin = postEffectManager.GetParaffinData(trans.index);
                        trans.easing = GetEasing(frame.frameNo, effectName);

                        var bone = frame.CreateBone(trans);
                        frame.UpdateBone(bone);
                        break;
                    }
                    case PostEffectType.DistanceFog:
                    {
                        var trans = CreateTransformData<TransformDataDistanceFog>(effectName);
                        trans.distanceFog = postEffectManager.GetDistanceFogData(trans.index);
                        trans.easing = GetEasing(frame.frameNo, effectName);

                        var bone = frame.CreateBone(trans);
                        frame.UpdateBone(bone);
                        break;
                    }
                }
            }
        }

        private GUIComboBox<MaidCache> _maidComboBox = new GUIComboBox<MaidCache>
        {
            getName = (maidCache, _) => maidCache == null ? "未選択" : maidCache.fullName,
            buttonSize = new Vector2(100, 20),
            contentSize = new Vector2(150, 300),
        };

        private GUIComboBox<string> _paraffinNameComboBox = new GUIComboBox<string>
        {
            getName = (name, index) => name,
        };

        private GUIComboBox<string> _copyToParaffinComboBox = new GUIComboBox<string>
        {
            getName = (name, index) => name,
        };

        private GUIComboBox<string> _distanceFogNameComboBox = new GUIComboBox<string>
        {
            getName = (name, index) => name,
        };

        private GUIComboBox<string> _copyToDistanceFogComboBox = new GUIComboBox<string>
        {
            getName = (name, index) => name,
        };

        private ColorFieldCache _color1FieldValue = new ColorFieldCache("Color1", true);
        private ColorFieldCache _color2FieldValue = new ColorFieldCache("Color2", true);

        private enum TabType
        {
            被写界深度,
            パラフィン,
            距離フォグ,
        }

        private TabType _tabType = TabType.被写界深度;

        public override void DrawWindow(GUIView view)
        {
            _tabType = view.DrawTabs(_tabType, 80, 20);

            switch (_tabType)
            {
                case TabType.被写界深度:
                    DrawDepthOfField(view);
                    break;
                case TabType.パラフィン:
                    DrawParaffin(view);
                    break;
                case TabType.距離フォグ:
                    DrawDistanceFog(view);
                    break;
            }

            view.DrawComboBox();
        }
        
        public void DrawDepthOfField(GUIView view)
        {
            view.SetEnabled(!view.IsComboBoxFocused());
            view.DrawHorizontalLine(Color.gray);
            view.AddSpace(5);

            view.SetEnabled(!view.IsComboBoxFocused() && studioHack.isPoseEditing);

            var depthOfField = postEffectManager.GetDepthOfFieldData();
            var updateTransform = false;

            view.DrawToggle("有効化", depthOfField.enabled, 80, 20, newValue =>
            {
                depthOfField.enabled = newValue;
                updateTransform = true;
            });

            updateTransform |= view.DrawSliderValue(new GUIView.SliderOption
            {
                label = "焦点距離",
                labelWidth = 30,
                min = 0f,
                max = config.positionRange,
                step = 0.1f,
                defaultValue = 10f,
                value = depthOfField.focalLength,
                onChanged = newValue => depthOfField.focalLength = newValue,
            });
            
            updateTransform |= view.DrawSliderValue(new GUIView.SliderOption
            {
                label = "焦点サイズ",
                labelWidth = 30,
                min = 0f,
                max = 2f,
                step = 0.01f,
                defaultValue = 0.05f,
                value = depthOfField.focalSize,
                onChanged = newValue => depthOfField.focalSize = newValue,
            });

            updateTransform |= view.DrawSliderValue(new GUIView.SliderOption
            {
                label = "絞り値",
                labelWidth = 30,
                min = 0f,
                max = 60f,
                step = 0.1f,
                defaultValue = 11.5f,
                value = depthOfField.aperture,
                onChanged = newValue => depthOfField.aperture = newValue,
            });

            updateTransform |= view.DrawSliderValue(new GUIView.SliderOption
            {
                label = "最大ブラー",
                labelWidth = 30,
                min = 0f,
                max = 10f,
                step = 0.1f,
                defaultValue = 2f,
                value = depthOfField.maxBlurSize,
                onChanged = newValue => depthOfField.maxBlurSize = newValue,
            });

            view.BeginHorizontal();
            {
                view.DrawLabel("追従メイド", 70, 20);

                view.DrawToggle("", depthOfField.maidSlotNo >= 0, 20, 20, newValue =>
                {
                    depthOfField.maidSlotNo = newValue ? _maidComboBox.currentIndex : -1;
                    updateTransform = true;
                });

                _maidComboBox.items = maidManager.maidCaches;
                _maidComboBox.onSelected = (maidCache, index) =>
                {
                    depthOfField.maidSlotNo = index;
                    updateTransform = true;
                };
                _maidComboBox.DrawButton(view);
            }
            view.EndLayout();

            view.SetEnabled(!view.IsComboBoxFocused());

            view.DrawHorizontalLine(Color.gray);

            view.DrawLabel("共通設定", 100, 20);

            view.BeginHorizontal();
            {
                view.DrawToggle("高解像度", config.dofHighResolution, 100, 20, newValue =>
                {
                    config.dofHighResolution = newValue;
                    config.dirty = true;
                    updateTransform = true;
                });

                view.DrawToggle("近距離ブラー", config.dofNearBlur, 100, 20, newValue =>
                {
                    config.dofNearBlur = newValue;
                    config.dirty = true;
                    updateTransform = true;
                });
            }
            view.EndLayout();

            view.DrawToggle("フォーカスの可視化", config.dofVisualizeFocus, 150, 20, newValue =>
            {
                config.dofVisualizeFocus = newValue;
                config.dirty = true;
                updateTransform = true;
            });

            if (updateTransform)
            {
                postEffectManager.ApplyDepthOfField(depthOfField);
            }
        }

        private List<string> _paraffinNames = new List<string>();
        private List<string> paraffinNames
        {
            get
            {
                var paraffinCount = timeline.paraffinCount;
                if (_paraffinNames.Count != paraffinCount)
                {
                    _paraffinNames.Clear();
                    for (var i = 0; i < paraffinCount; i++)
                    {
                        _paraffinNames.Add(PostEffectUtils.GetParaffinName(i));
                    }
                }

                return _paraffinNames;
            }
        }

        private List<string> _paraffinJpNames = new List<string>();
        private List<string> paraffinJpNames
        {
            get
            {
                var paraffinCount = timeline.paraffinCount;
                if (_paraffinJpNames.Count != paraffinCount)
                {
                    _paraffinJpNames.Clear();
                    for (var i = 0; i < paraffinCount; i++)
                    {
                        _paraffinJpNames.Add(PostEffectUtils.GetParaffinJpName(i));
                    }
                }

                return _paraffinJpNames;
            }
        }

        public void DrawParaffin(GUIView view)
        {
            view.SetEnabled(!view.IsComboBoxFocused() && studioHack.isPoseEditing);

            view.BeginHorizontal();
            {
                view.margin = 0;

                view.DrawLabel("エフェクト数", view.labelWidth, 20);

                view.DrawIntField(new GUIView.IntFieldOption
                {
                    value = timeline.paraffinCount,
                    width = view.viewRect.width - (view.labelWidth + 40 + view.padding.x * 2),
                    height = 20,
                    onChanged = x => timeline.paraffinCount = x,
                });

                if (view.DrawButton("-", 20, 20))
                {
                    timeline.paraffinCount--;
                }
                if (view.DrawButton("+", 20, 20))
                {
                    timeline.paraffinCount++;
                }

                timeline.paraffinCount = Mathf.Clamp(timeline.paraffinCount, 0, 8);

                view.margin = GUIView.defaultMargin;
            }
            view.EndLayout();

            if (timeline.paraffinCount == 0)
            {
                view.DrawLabel("エフェクトを追加してください", 200, 20);
                return;
            }

            _paraffinNameComboBox.items = paraffinJpNames;
            _paraffinNameComboBox.DrawButton("対象", view);

            var index = _paraffinNameComboBox.currentIndex;

            var paraffin = postEffectManager.GetParaffinData(index);
            if (paraffin == null)
            {
                view.DrawLabel("エフェクトを選択してください", 200, 20);
                return;
            }

            view.SetEnabled(!view.IsComboBoxFocused());
            view.DrawHorizontalLine(Color.gray);
            view.AddSpace(5);
            view.BeginScrollView();

            view.SetEnabled(!view.IsComboBoxFocused() && studioHack.isPoseEditing);

            var updateTransform = false;

            updateTransform = view.DrawToggle("有効化", paraffin.enabled, 80, 20, newValue =>
            {
                paraffin.enabled = newValue;
            });

            if (timeline.usePostEffectExtra)
            {
                updateTransform |= view.DrawColor(
                    _color1FieldValue,
                    paraffin.color1,
                    Color.white,
                    newValue => paraffin.color1 = newValue
                );

                updateTransform |= view.DrawColor(
                    _color2FieldValue,
                    paraffin.color2,
                    new Color(1f, 1f, 1f, 0f),
                    newValue => paraffin.color2 = newValue
                );
            }
            else
            {
                updateTransform |= view.DrawColor(
                    _color1FieldValue,
                    paraffin.color1,
                    Color.white,
                    newValue =>
                    {
                        var alpha2 = paraffin.color2.a;
                        paraffin.color1 = newValue;
                        paraffin.color2 = newValue;
                        paraffin.color2.a = alpha2;
                    }
                );

                updateTransform |= view.DrawSliderValue(new GUIView.SliderOption
                {
                    label = "A2",
                    labelWidth = 30,
                    min = 0f,
                    max = 1f,
                    step = 0.01f,
                    defaultValue = 0f,
                    value = paraffin.color2.a,
                    onChanged = newValue => paraffin.color2.a = newValue,
                });
            }

            updateTransform |= view.DrawSliderValue(new GUIView.SliderOption
            {
                label = "X",
                labelWidth = 30,
                min = -1f,
                max = 2f,
                step = 0.01f,
                defaultValue = 0.5f,
                value = paraffin.centerPosition.x,
                onChanged = newValue => paraffin.centerPosition.x = newValue,
            });

            updateTransform |= view.DrawSliderValue(new GUIView.SliderOption
            {
                label = "Y",
                labelWidth = 30,
                min = -1f,
                max = 2f,
                step = 0.01f,
                defaultValue = 0.5f,
                value = paraffin.centerPosition.y,
                onChanged = newValue => paraffin.centerPosition.y = newValue,
            });
            
            updateTransform |= view.DrawSliderValue(new GUIView.SliderOption
            {
                label = "外側半径",
                labelWidth = 30,
                min = 0f,
                max = 1f,
                step = 0.01f,
                defaultValue = 1f,
                value = paraffin.radiusFar,
                onChanged = newValue => paraffin.radiusFar = newValue,
            });

            updateTransform |= view.DrawSliderValue(new GUIView.SliderOption
            {
                label = "内側半径",
                labelWidth = 30,
                min = 0f,
                max = 1f,
                step = 0.01f,
                defaultValue = 0f,
                value = paraffin.radiusNear,
                onChanged = newValue => paraffin.radiusNear = newValue,
            });

            updateTransform |= view.DrawSliderValue(new GUIView.SliderOption
            {
                label = "SX",
                labelWidth = 30,
                min = 0f,
                max = 5f,
                step = 0.01f,
                defaultValue = 1f,
                value = paraffin.radiusScale.x,
                onChanged = newValue => paraffin.radiusScale.x = newValue,
            });

            updateTransform |= view.DrawSliderValue(new GUIView.SliderOption
            {
                label = "SY",
                labelWidth = 30,
                min = 0f,
                max = 5f,
                step = 0.01f,
                defaultValue = 1f,
                value = paraffin.radiusScale.y,
                onChanged = newValue => paraffin.radiusScale.y = newValue,
            });

            updateTransform |= view.DrawSliderValue(new GUIView.SliderOption
            {
                label = "最小深度",
                labelWidth = 30,
                min = 0f,
                max = paraffin.depthMax,
                step = 0.1f,
                defaultValue = 0f,
                value = paraffin.depthMin,
                onChanged = newValue => paraffin.depthMin = newValue,
            });

            updateTransform |= view.DrawSliderValue(new GUIView.SliderOption
            {
                label = "最大深度",
                labelWidth = 30,
                min = paraffin.depthMin,
                max = 100f,
                step = 0.1f,
                defaultValue = 0f,
                value = paraffin.depthMax,
                onChanged = newValue => paraffin.depthMax = newValue,
            });

            updateTransform |= view.DrawSliderValue(new GUIView.SliderOption
            {
                label = "深度幅",
                labelWidth = 30,
                min = 0f,
                max = Camera.main.farClipPlane,
                step = 0.1f,
                defaultValue = 0f,
                value = paraffin.depthFade,
                onChanged = newValue => paraffin.depthFade = newValue,
            });

            view.DrawLabel("ブレンドモード", 100, 20);

            updateTransform |= view.DrawSliderValue(new GUIView.SliderOption
            {
                label = "通常",
                labelWidth = 30,
                min = 0f,
                max = 1f,
                step = 0.01f,
                defaultValue = 0f,
                value = paraffin.useNormal,
                onChanged = newValue => paraffin.useNormal = newValue,
            });

            updateTransform |= view.DrawSliderValue(new GUIView.SliderOption
            {
                label = "加算",
                labelWidth = 30,
                min = 0f,
                max = 1f,
                step = 0.01f,
                defaultValue = 0f,
                value = paraffin.useAdd,
                onChanged = newValue => paraffin.useAdd = newValue,
            });

            updateTransform |= view.DrawSliderValue(new GUIView.SliderOption
            {
                label = "乗算",
                labelWidth = 30,
                min = 0f,
                max = 1f,
                step = 0.01f,
                defaultValue = 0f,
                value = paraffin.useMultiply,
                onChanged = newValue => paraffin.useMultiply = newValue,
            });

            updateTransform |= view.DrawSliderValue(new GUIView.SliderOption
            {
                label = "オーバーレイ",
                labelWidth = 30,
                min = 0f,
                max = 1f,
                step = 0.01f,
                defaultValue = 0f,
                value = paraffin.useOverlay,
                onChanged = newValue => paraffin.useOverlay = newValue,
            });

            updateTransform |= view.DrawSliderValue(new GUIView.SliderOption
            {
                label = "減算",
                labelWidth = 30,
                min = 0f,
                max = 1f,
                step = 0.01f,
                defaultValue = 0f,
                value = paraffin.useSubstruct,
                onChanged = newValue => paraffin.useSubstruct = newValue,
            });

            view.DrawHorizontalLine(Color.gray);

            _copyToParaffinComboBox.items = _paraffinJpNames;
            _copyToParaffinComboBox.DrawButton("コピー先", view);

            if (view.DrawButton("コピー", 60, 20))
            {
                var copyToIndex = _copyToParaffinComboBox.currentIndex;
                if (copyToIndex != -1 && copyToIndex != index)
                {
                    postEffectManager.ApplyParaffin(copyToIndex, paraffin);
                }
            }

            view.SetEnabled(!view.IsComboBoxFocused());

            view.DrawHorizontalLine(Color.gray);

            view.DrawLabel("共通設定", 100, 20);

            updateTransform |= view.DrawToggle("デバッグ表示", config.paraffinDebug, 150, 20, newValue =>
            {
                config.paraffinDebug = newValue;
                config.dirty = true;
            });

            if (updateTransform)
            {
                postEffectManager.ApplyParaffin(index, paraffin);
            }

            view.SetEnabled(!view.IsComboBoxFocused());
            view.EndScrollView();
        }

        private List<string> _distanceFogNames = new List<string>();
        private List<string> distanceFogNames
        {
            get
            {
                var distanceFogCount = timeline.distanceFogCount;
                if (_distanceFogNames.Count != distanceFogCount)
                {
                    _distanceFogNames.Clear();
                    for (var i = 0; i < distanceFogCount; i++)
                    {
                        _distanceFogNames.Add(PostEffectUtils.GetDistanceFogName(i));
                    }
                }

                return _distanceFogNames;
            }
        }

        private List<string> _distanceFogJpNames = new List<string>();
        private List<string> distanceFogJpNames
        {
            get
            {
                var distanceFogCount = timeline.distanceFogCount;
                if (_distanceFogJpNames.Count != distanceFogCount)
                {
                    _distanceFogJpNames.Clear();
                    for (var i = 0; i < distanceFogCount; i++)
                    {
                        _distanceFogJpNames.Add(PostEffectUtils.GetDistanceFogJpName(i));
                    }
                }

                return _distanceFogJpNames;
            }
        }

        public void DrawDistanceFog(GUIView view)
        {
            view.SetEnabled(!view.IsComboBoxFocused() && studioHack.isPoseEditing);

            view.BeginHorizontal();
            {
                view.margin = 0;

                view.DrawLabel("エフェクト数", view.labelWidth, 20);

                view.DrawIntField(new GUIView.IntFieldOption
                {
                    value = timeline.distanceFogCount,
                    width = view.viewRect.width - (view.labelWidth + 40 + view.padding.x * 2),
                    height = 20,
                    onChanged = x => timeline.distanceFogCount = x,
                });

                if (view.DrawButton("-", 20, 20))
                {
                    timeline.distanceFogCount--;
                }
                if (view.DrawButton("+", 20, 20))
                {
                    timeline.distanceFogCount++;
                }

                timeline.distanceFogCount = Mathf.Clamp(timeline.distanceFogCount, 0, 4);

                view.margin = GUIView.defaultMargin;
            }
            view.EndLayout();

            if (timeline.distanceFogCount == 0)
            {
                view.DrawLabel("エフェクトを追加してください", 200, 20);
                return;
            }

            _distanceFogNameComboBox.items = distanceFogJpNames;
            _distanceFogNameComboBox.DrawButton("対象", view);

            var index = _distanceFogNameComboBox.currentIndex;

            var distanceFog = postEffectManager.GetDistanceFogData(index);
            if (distanceFog == null)
            {
                view.DrawLabel("エフェクトを選択してください", 200, 20);
                return;
            }

            view.SetEnabled(!view.IsComboBoxFocused());
            view.DrawHorizontalLine(Color.gray);
            view.AddSpace(5);
            view.BeginScrollView();

            view.SetEnabled(!view.IsComboBoxFocused() && studioHack.isPoseEditing);

            var updateTransform = false;

            updateTransform = view.DrawToggle("有効化", distanceFog.enabled, 80, 20, newValue =>
            {
                distanceFog.enabled = newValue;
            });

            if (timeline.usePostEffectExtra)
            {
                updateTransform |= view.DrawColor(
                    _color1FieldValue,
                    distanceFog.color1,
                    Color.white,
                    newValue => distanceFog.color1 = newValue
                );

                updateTransform |= view.DrawColor(
                    _color2FieldValue,
                    distanceFog.color2,
                    new Color(1f, 1f, 1f, 0f),
                    newValue => distanceFog.color2 = newValue
                );
            }
            else
            {
                updateTransform |= view.DrawColor(
                    _color2FieldValue,
                    distanceFog.color2,
                    Color.white,
                    newValue =>
                    {
                        var alpha1 = distanceFog.color1.a;
                        distanceFog.color1 = newValue;
                        distanceFog.color2 = newValue;
                        distanceFog.color1.a = alpha1;
                    }
                );

                updateTransform |= view.DrawSliderValue(new GUIView.SliderOption
                {
                    label = "A1",
                    labelWidth = 30,
                    min = 0f,
                    max = 1f,
                    step = 0.01f,
                    defaultValue = 0f,
                    value = distanceFog.color1.a,
                    onChanged = newValue => distanceFog.color1.a = newValue,
                });
            }

            updateTransform |= view.DrawSliderValue(new GUIView.SliderOption
            {
                label = "開始深度",
                labelWidth = 30,
                min = 0f,
                max = distanceFog.fogEnd,
                step = 0.1f,
                defaultValue = 0f,
                value = distanceFog.fogStart,
                onChanged = newValue => distanceFog.fogStart = newValue,
            });

            updateTransform |= view.DrawSliderValue(new GUIView.SliderOption
            {
                label = "終了深度",
                labelWidth = 30,
                min = distanceFog.fogStart,
                max = 100f,
                step = 0.1f,
                defaultValue = 50f,
                value = distanceFog.fogEnd,
                onChanged = newValue => distanceFog.fogEnd = newValue,
            });

            updateTransform |= view.DrawSliderValue(new GUIView.SliderOption
            {
                label = "指数",
                labelWidth = 30,
                min = 0f,
                max = 10f,
                step = 0.01f,
                defaultValue = 1f,
                value = distanceFog.fogExp,
                onChanged = newValue => distanceFog.fogExp = newValue,
            });

            view.DrawHorizontalLine(Color.gray);

            _copyToDistanceFogComboBox.items = _distanceFogJpNames;
            _copyToDistanceFogComboBox.DrawButton("コピー先", view);

            if (view.DrawButton("コピー", 60, 20))
            {
                var copyToIndex = _copyToDistanceFogComboBox.currentIndex;
                if (copyToIndex != -1 && copyToIndex != index)
                {
                    postEffectManager.ApplyDistanceFog(copyToIndex, distanceFog);
                }
            }

            view.SetEnabled(!view.IsComboBoxFocused());

            view.DrawHorizontalLine(Color.gray);

            view.DrawLabel("共通設定", 100, 20);

            updateTransform |= view.DrawToggle("デバッグ表示", config.distanceFogDebug, 150, 20, newValue =>
            {
                config.distanceFogDebug = newValue;
                config.dirty = true;
            });

            if (updateTransform)
            {
                postEffectManager.ApplyDistanceFog(index, distanceFog);
            }

            view.SetEnabled(!view.IsComboBoxFocused());
            view.EndScrollView();
        }

        public override TransformType GetTransformType(string name)
        {
            var effectType = PostEffectUtils.GetEffectType(name);
            switch (effectType)
            {
                case PostEffectType.DepthOfField:
                    return TransformType.DepthOfField;
                case PostEffectType.Paraffin:
                    return TransformType.Paraffin;
                case PostEffectType.DistanceFog:
                    return TransformType.DistanceFog;
            }

            return TransformType.None;
        }
    }
}