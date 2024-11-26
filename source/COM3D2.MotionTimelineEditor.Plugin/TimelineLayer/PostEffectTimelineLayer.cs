using System;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    using PostEffectPlayData = PlayDataBase<PostEffectMotionData>;

    public class PostEffectTimeLineRow
    {
        public int frame;
        public string name;
        public PostEffectType type;
        public int index;
        public int easing;
        public DepthOfFieldData depthOfField;
        public ParaffinData paraffin;
    }

    public class PostEffectMotionData : MotionDataBase
    {
        public PostEffectTimeLineRow start;
        public PostEffectTimeLineRow end;
    }

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

        private Dictionary<string, List<PostEffectTimeLineRow>> _timelineRowsMap = new Dictionary<string, List<PostEffectTimeLineRow>>();
        private Dictionary<string, PostEffectPlayData> _playDataMap = new Dictionary<string, PostEffectPlayData>();

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

        private void ApplyPlayData()
        {
            if (!isCurrent && !config.isPostEffectSync)
            {
                postEffectManager.DisableDepthOfField();
                postEffectManager.DisableParaffin();
                return;
            }

            var playingFrameNoFloat = this.playingFrameNoFloat;

            foreach (var postEffectName in _playDataMap.Keys)
            {
                var playData = _playDataMap[postEffectName];

                playData.Update(playingFrameNoFloat);

                var current = playData.current;
                if (current != null)
                {
                    ApplyMotion(current, playData.lerpFrame);
                }

                //PluginUtils.LogDebug("ApplyPlayData: postEffectName={0} lerpTime={1}, listIndex={2}", postEffectName, playData.lerpTime, playData.listIndex);
            }
        }

        private void ApplyMotion(PostEffectMotionData motion, float lerpTime)
        {
            switch (motion.start.type)
            {
                case PostEffectType.DepthOfField:
                    ApplyDepthOfField(motion, lerpTime);
                    break;
                case PostEffectType.Paraffin:
                    ApplyParaffin(motion, lerpTime);
                    break;
            }
        }

        private void ApplyDepthOfField(PostEffectMotionData motion, float lerpTime)
        {
            var depthOfField = postEffectManager.GetDepthOfFieldData();

            var start = motion.start;
            var end = motion.end;

            var stData = start.depthOfField;
            var edData = end.depthOfField;

            float easingTime = CalcEasingValue(lerpTime, start.easing);
            depthOfField.enabled = stData.enabled;
            depthOfField.focalLength = Mathf.Lerp(stData.focalLength, edData.focalLength, easingTime);
            depthOfField.focalSize = Mathf.Lerp(stData.focalSize, edData.focalSize, easingTime);
            depthOfField.aperture = Mathf.Lerp(stData.aperture, edData.aperture, easingTime);
            depthOfField.maxBlurSize = Mathf.Lerp(stData.maxBlurSize, edData.maxBlurSize, easingTime);
            depthOfField.maidSlotNo = stData.maidSlotNo;

            postEffectManager.ApplyDepthOfField(depthOfField);
        }

        private void ApplyParaffin(PostEffectMotionData motion, float lerpTime)
        {
            var index = motion.start.index;
            var paraffin = postEffectManager.GetParaffinData(index);

            var start = motion.start;
            var end = motion.end;

            var stData = start.paraffin;
            var edData = end.paraffin;

            float easingTime = CalcEasingValue(lerpTime, start.easing);
            paraffin.enabled = stData.enabled;
            paraffin.color1 = Color.Lerp(stData.color1, edData.color1, easingTime);
            paraffin.color2 = Color.Lerp(stData.color2, edData.color2, easingTime);
            paraffin.centerPosition = Vector2.Lerp(stData.centerPosition, edData.centerPosition, easingTime);
            paraffin.radiusFar = Mathf.Lerp(stData.radiusFar, edData.radiusFar, easingTime);
            paraffin.radiusNear = Mathf.Lerp(stData.radiusNear, edData.radiusNear, easingTime);
            paraffin.radiusScale = Vector2.Lerp(stData.radiusScale, edData.radiusScale, easingTime);
            paraffin.useNormal = Mathf.Lerp(stData.useNormal, edData.useNormal, easingTime);
            paraffin.useAdd = Mathf.Lerp(stData.useAdd, edData.useAdd, easingTime);
            paraffin.useMultiply = Mathf.Lerp(stData.useMultiply, edData.useMultiply, easingTime);
            paraffin.useOverlay = Mathf.Lerp(stData.useOverlay, edData.useOverlay, easingTime);
            paraffin.useSubstruct = Mathf.Lerp(stData.useSubstruct, edData.useSubstruct, easingTime);

            postEffectManager.ApplyParaffin(index, paraffin);
        }

        public override void UpdateFrame(FrameData frame)
        {
            foreach (var effectName in allBoneNames)
            {
                var effectType = PostEffectUtils.ToEffectType(effectName);
                var trans = CreateTransformData(effectName);
                var index = PluginUtils.ExtractGroup(effectName);

                switch (effectType)
                {
                    case PostEffectType.DepthOfField:
                    {
                        var depthOfField = postEffectManager.GetDepthOfFieldData();
                        trans.visible = depthOfField.enabled;
                        trans["focalLength"].value = depthOfField.focalLength;
                        trans["focalSize"].value = depthOfField.focalSize;
                        trans["aperture"].value = depthOfField.aperture;
                        trans["maxBlurSize"].value = depthOfField.maxBlurSize;
                        trans["maidSlotNo"].intValue = depthOfField.maidSlotNo;
                        break;
                    }
                    case PostEffectType.Paraffin:
                    {
                        var paraffin = postEffectManager.GetParaffinData(index);
                        trans.visible = paraffin.enabled;
                        trans.color = paraffin.color1;
                        trans.subColor = paraffin.color2;
                        trans["centerPositionX"].value = paraffin.centerPosition.x;
                        trans["centerPositionY"].value = paraffin.centerPosition.y;
                        trans["radiusFar"].value = paraffin.radiusFar;
                        trans["radiusNear"].value = paraffin.radiusNear;
                        trans["radiusScaleX"].value = paraffin.radiusScale.x;
                        trans["radiusScaleY"].value = paraffin.radiusScale.y;
                        trans["useNormal"].value = paraffin.useNormal;
                        trans["useAdd"].value = paraffin.useAdd;
                        trans["useMultiply"].value = paraffin.useMultiply;
                        trans["useOverlay"].value = paraffin.useOverlay;
                        trans["useSubstruct"].value = paraffin.useSubstruct;
                        break;
                    }
                }

                trans.easing = GetEasing(frame.frameNo, effectName);

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

        private void AddTimelineRow(FrameData frame)
        {
            foreach (var name in allBoneNames)
            {
                var bone = frame.GetBone(name);
                if (bone == null)
                {
                    continue;
                }

                List<PostEffectTimeLineRow> rows;
                if (!_timelineRowsMap.TryGetValue(name, out rows))
                {
                    rows = new List<PostEffectTimeLineRow>();
                    _timelineRowsMap[name] = rows;
                }

                var trans = bone.transform;
                var effectType = PostEffectUtils.ToEffectType(name);
                var index = PluginUtils.ExtractGroup(name);

                var row = new PostEffectTimeLineRow
                {
                    frame = frame.frameNo,
                    name = bone.name,
                    type = effectType,
                    index = index,
                    easing = trans.easing,
                };

                switch (effectType)
                {
                    case PostEffectType.DepthOfField:
                        row.depthOfField = new DepthOfFieldData
                        {
                            enabled = trans.visible,
                            focalLength = trans["focalLength"].value,
                            focalSize = trans["focalSize"].value,
                            aperture = trans["aperture"].value,
                            maxBlurSize = trans["maxBlurSize"].value,
                            maidSlotNo = trans["maidSlotNo"].intValue,
                        };
                        break;
                    case PostEffectType.Paraffin:
                        row.paraffin = new ParaffinData
                        {
                            enabled = trans.visible,
                            color1 = trans.color,
                            color2 = trans.subColor,
                            centerPosition = new Vector2(trans["centerPositionX"].value, trans["centerPositionY"].value),
                            radiusFar = trans["radiusFar"].value,
                            radiusNear = trans["radiusNear"].value,
                            radiusScale = new Vector2(trans["radiusScaleX"].value, trans["radiusScaleY"].value),
                            useNormal = trans["useNormal"].value,
                            useAdd = trans["useAdd"].value,
                            useMultiply = trans["useMultiply"].value,
                            useOverlay = trans["useOverlay"].value,
                            useSubstruct = trans["useSubstruct"].value,
                        };
                        break;
                }

                rows.Add(row);
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

                PostEffectPlayData playData;
                if (!_playDataMap.TryGetValue(name, out playData))
                {
                    playData = new PostEffectPlayData
                    {
                        motions = new List<PostEffectMotionData>(rows.Count),
                    };
                    _playDataMap[name] = playData;
                }

                for (var i = 0; i < rows.Count - 1; i++)
                {
                    var start = rows[i];
                    var end = rows[i + 1];

                    var stFrame = start.frame;
                    var edFrame = end.frame;

                    var motion = new PostEffectMotionData
                    {
                        stFrame = stFrame,
                        edFrame = edFrame,
                        start = start,
                        end = end,
                    };

                    playData.motions.Add(motion);
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
                AddTimelineRow(keyFrame);
            }

            AddTimelineRow(_dummyLastFrame);

            BuildPlayData(forOutput);

            return null;
        }

        public override void OutputDCM(XElement songElement)
        {
            // do nothing
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

        public override ITransformData CreateTransformData(string name)
        {
            ITransformData transform = null;

            var effectType = PostEffectUtils.ToEffectType(name);
            switch (effectType)
            {
                case PostEffectType.DepthOfField:
                    transform = new TransformDataDepthOfField();
                    transform.Initialize(name);
                    break;
                case PostEffectType.Paraffin:
                    transform = new TransformDataParaffin();
                    transform.Initialize(name);
                    break;
            }

            return transform;
        }
    }
}