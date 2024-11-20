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
    using PostEffectPlayData = MotionPlayData<PostEffectMotionData>;

    public class PostEffectTimeLineRow
    {
        public int frame;
        public string name;
        public PostEffectType type;
        public int easing;
        public DepthOfFieldData depthOfField;
    }

    public class PostEffectMotionData : MotionDataBase
    {
        public string name;
        public PostEffectType type;
        public int easing;
        public DepthOfFieldData stDepthOfField;
        public DepthOfFieldData edDepthOfField;
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

        public override List<string> allBoneNames
        {
            get
            {
                return PostEffectUtils.PostEffectNames;
            }
        }

        private Dictionary<string, List<PostEffectTimeLineRow>> _timelineRowsMap = new Dictionary<string, List<PostEffectTimeLineRow>>();
        private Dictionary<string, PostEffectPlayData> _playDataMap = new Dictionary<string, PostEffectPlayData>();
        private List<PostEffectMotionData> _outputMotions = new List<PostEffectMotionData>(128);

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
            switch (motion.type)
            {
                case PostEffectType.DepthOfField:
                    ApplyDepthOfField(motion, lerpTime);
                    break;
            }
        }

        private void ApplyDepthOfField(PostEffectMotionData motion, float lerpTime)
        {
            var depthOfField = postEffectManager.GetDepthOfFieldData();

            float easingTime = CalcEasingValue(lerpTime, motion.easing);
            depthOfField.enabled = motion.stDepthOfField.enabled;
            depthOfField.focalLength = Mathf.Lerp(motion.stDepthOfField.focalLength, motion.edDepthOfField.focalLength, easingTime);
            depthOfField.focalSize = Mathf.Lerp(motion.stDepthOfField.focalSize, motion.edDepthOfField.focalSize, easingTime);
            depthOfField.aperture = Mathf.Lerp(motion.stDepthOfField.aperture, motion.edDepthOfField.aperture, easingTime);
            depthOfField.maxBlurSize = Mathf.Lerp(motion.stDepthOfField.maxBlurSize, motion.edDepthOfField.maxBlurSize, easingTime);
            depthOfField.maidSlotNo = motion.stDepthOfField.maidSlotNo;

            postEffectManager.ApplyDepthOfField(depthOfField);
        }

        public override void UpdateFrame(FrameData frame)
        {
            var depthOfField = postEffectManager.GetDepthOfFieldData();

            foreach (var effectName in allBoneNames)
            {
                var effectType = PostEffectUtils.ToEffectType(effectName);
                var trans = CreateTransformData(effectName);

                switch (effectType)
                {
                    case PostEffectType.DepthOfField:
                        trans["enabled"].boolValue = depthOfField.enabled;
                        trans["focalLength"].value = depthOfField.focalLength;
                        trans["focalSize"].value = depthOfField.focalSize;
                        trans["aperture"].value = depthOfField.aperture;
                        trans["maxBlurSize"].value = depthOfField.maxBlurSize;
                        trans["maidSlotNo"].intValue = depthOfField.maidSlotNo;
                        break;
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

        private void AddMotion(FrameData frame)
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

                var row = new PostEffectTimeLineRow
                {
                    frame = frame.frameNo,
                    name = bone.name,
                    type = effectType,
                    easing = trans.easing,
                };

                switch (effectType)
                {
                    case PostEffectType.DepthOfField:
                        row.depthOfField = new DepthOfFieldData
                        {
                            enabled = trans["enabled"].boolValue,
                            focalLength = trans["focalLength"].value,
                            focalSize = trans["focalSize"].value,
                            aperture = trans["aperture"].value,
                            maxBlurSize = trans["maxBlurSize"].value,
                            maidSlotNo = trans["maidSlotNo"].intValue,
                        };
                        break;
                }

                rows.Add(row);
            }
        }

        private void BuildPlayData(bool forOutput)
        {
            PluginUtils.LogDebug("BuildPlayData");
            _playDataMap.Clear();

            foreach (var pair in _timelineRowsMap)
            {
                var name = pair.Key;
                var rows = pair.Value;

                PostEffectPlayData playData;
                if (!_playDataMap.TryGetValue(name, out playData))
                {
                    playData = new PostEffectPlayData
                    {
                        motions = new List<PostEffectMotionData>(rows.Count),
                    };
                    _playDataMap[name] = playData;
                }

                playData.ResetIndex();
                playData.motions.Clear();

                for (var i = 0; i < rows.Count - 1; i++)
                {
                    var start = rows[i];
                    var end = rows[i + 1];

                    var stFrame = start.frame;
                    var edFrame = end.frame;

                    var motion = new PostEffectMotionData
                    {
                        name = name,
                        type = start.type,
                        stFrame = stFrame,
                        edFrame = edFrame,
                        easing = end.easing,
                        stDepthOfField = start.depthOfField,
                        edDepthOfField = end.depthOfField,
                    };

                    playData.motions.Add(motion);
                }
            }

            foreach (var pair in _playDataMap)
            {
                var name = pair.Key;
                var playData = pair.Value;
                playData.Setup(timeline.singleFrameType);
                //PluginUtils.LogDebug("PlayData: name={0}, count={1}", name, playData.motions.Count);
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

        public override void OutputDCM(XElement songElement)
        {
            // do nothing
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

        private enum TabType
        {
            被写界深度,
        }

        private TabType _tabType = TabType.被写界深度;

        public override void DrawWindow(GUIView view)
        {
            _tabType = view.DrawTabs(_tabType, 50, 20);

            view.SetEnabled(!view.IsComboBoxFocused());
            view.DrawHorizontalLine(Color.gray);
            view.AddSpace(5);
            view.BeginScrollView();

            switch (_tabType)
            {
                case TabType.被写界深度:
                    DrawDepthOfField(view);
                    break;
            }

            view.SetEnabled(!view.IsComboBoxFocused());
            view.EndScrollView();

            view.DrawComboBox();
        }
        
        public void DrawDepthOfField(GUIView view)
        {
            view.SetEnabled(!view.IsComboBoxFocused() && studioHack.isPoseEditing);

            var depthOfField = postEffectManager.GetDepthOfFieldData();
            var updateTransform = false;

            view.DrawToggle("有効化", depthOfField.enabled, 80, 20, newValue =>
            {
                depthOfField.enabled = newValue;
                updateTransform = true;
            });

            updateTransform |= view.DrawSliderValue(
                new GUIView.SliderOption
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
            
            updateTransform |= view.DrawSliderValue(
                new GUIView.SliderOption
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

            updateTransform |= view.DrawSliderValue(
                new GUIView.SliderOption
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

            updateTransform |= view.DrawSliderValue(
                new GUIView.SliderOption
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
            }

            return transform;
        }
    }
}