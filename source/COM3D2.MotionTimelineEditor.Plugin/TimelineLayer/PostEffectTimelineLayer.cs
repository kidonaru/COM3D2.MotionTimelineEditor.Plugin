using System;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    [TimelineLayerDesc("ポストエフェクト", 44)]
    public class PostEffectTimelineLayer : TimelineLayerBase
    {
        public override string className
        {
            get
            {
                return typeof(PostEffectTimelineLayer).Name;
            }
        }

        public override bool isPostEffectLayer
        {
            get
            {
                return true;
            }
        }

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
                }
                return _allBoneNames;
            }
        }

        private static PostEffectManager postEffectManager
        {
            get
            {
                return PostEffectManager.instance;
            }
        }

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
            AddFirstBones(allBoneNames);
        }

        private void InitParrifinEffect()
        {
            while (postEffectManager.GetParaffinCount() < timeline.paraffinCount)
            {
                postEffectManager.AddParaffinData(new ParaffinData());
            }
            while (postEffectManager.GetParaffinCount() > timeline.paraffinCount)
            {
                postEffectManager.RemoveParaffinData();
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
        }

        public override void LateUpdate()
        {
            base.LateUpdate();

            var boneCount = 1 + timeline.paraffinCount;
            if (allBoneNames.Count != boneCount)
            {
                _allBoneNames = null;
                InitParrifinEffect();
                InitMenuItems();
                AddFirstBones(allBoneNames);
            }

            if (!studioHack.isPoseEditing)
            {
                ApplyPlayData();
            }
        }

        protected override void ApplyPlayData()
        {
            if (!isCurrent && !config.isPostEffectSync)
            {
                postEffectManager.DisableDepthOfField();
                postEffectManager.DisableParaffin();
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
            }
        }

        private void ApplyDepthOfField(MotionData motion, float t)
        {
            var start = motion.start as TransformDataDepthOfField;
            var end = motion.end as TransformDataDepthOfField;

            var stData = start.depthOfField;
            var edData = end.depthOfField;

            float easingTime = CalcEasingValue(t, start.easing);
            var depthOfField = DepthOfFieldData.Lerp(stData, edData, easingTime);

            postEffectManager.ApplyDepthOfField(depthOfField);
        }

        private void ApplyParaffin(MotionData motion, float t)
        {
            var start = motion.start as TransformDataParaffin;
            var end = motion.end as TransformDataParaffin;

            var stData = start.paraffin;
            var edData = end.paraffin;

            float easingTime = CalcEasingValue(t, start.easing);
            var paraffin = ParaffinData.Lerp(stData, edData, easingTime);

            var index = start.index;
            postEffectManager.ApplyParaffin(index, paraffin);
        }

        public override void UpdateFrame(FrameData frame)
        {
            foreach (var effectName in allBoneNames)
            {
                var effectType = GetEffectType(effectName);
                var index = GetEffectIndex(effectName);

                switch (effectType)
                {
                    case PostEffectType.DepthOfField:
                    {
                        var depthOfField = postEffectManager.GetDepthOfFieldData();
                        var trans = CreateTransformData<TransformDataDepthOfField>(effectName);
                        trans.depthOfField = depthOfField;
                        trans.easing = GetEasing(frame.frameNo, effectName);

                        var bone = frame.CreateBone(trans);
                        frame.UpdateBone(bone);
                        break;
                    }
                    case PostEffectType.Paraffin:
                    {
                        var paraffin = postEffectManager.GetParaffinData(index);
                        var trans = CreateTransformData<TransformDataParaffin>(effectName);
                        trans.paraffin = paraffin;
                        trans.easing = GetEasing(frame.frameNo, effectName);

                        var bone = frame.CreateBone(trans);
                        frame.UpdateBone(bone);
                        break;
                    }
                }
            }
        }

        protected override void BuildPlayData()
        {
            foreach (var pair in _timelineBonesMap)
            {
                var name = pair.Key;
                var bones = pair.Value;

                if (bones.Count == 0)
                {
                    continue;
                }

                switch (bones[0].transform.type)
                {
                    case TransformType.Paraffin:
                        foreach (var bone in bones)
                        {
                            var trans = bone.transform as TransformDataParaffin;
                            trans.index = GetEffectIndex(name);
                        }
                        break;
                }
            }

            base.BuildPlayData();
        }

        private static Dictionary<string, int> _indexCache = new Dictionary<string, int>(16);

        private int GetEffectIndex(string name)
        {
            int index;
            if (_indexCache.TryGetValue(name, out index))
            {
                return index;
            }

            index = PluginUtils.ExtractGroup(name);
            _indexCache[name] = index;
            return index;
        }

        private static Dictionary<string, PostEffectType> _effectTypeCache = new Dictionary<string, PostEffectType>(16);

        private PostEffectType GetEffectType(string name)
        {
            PostEffectType type;
            if (_effectTypeCache.TryGetValue(name, out type))
            {
                return type;
            }

            type = PostEffectUtils.ToEffectType(name);
            _effectTypeCache[name] = type;
            return type;
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

        private ColorFieldCache _color1FieldValue = new ColorFieldCache("Color1", true);
        private ColorFieldCache _color2FieldValue = new ColorFieldCache("Color2", true);

        private enum TabType
        {
            被写界深度,
            パラフィン,
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

                timeline.paraffinCount = Mathf.Clamp(timeline.paraffinCount, 0, 4);

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

            if (timeline.useParaffinExtra)
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
                        paraffin.color1 = newValue;
                        paraffin.color2 = newValue;
                        paraffin.color2.a = 0f;
                    }
                );
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

            updateTransform = view.DrawToggle("デバッグ表示", config.paraffinDebug, 150, 20, newValue =>
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

        public override TransformType GetTransformType(string name)
        {
            var effectType = GetEffectType(name);
            switch (effectType)
            {
                case PostEffectType.DepthOfField:
                    return TransformType.DepthOfField;
                case PostEffectType.Paraffin:
                    return TransformType.Paraffin;
            }

            return TransformType.None;
        }
    }
}