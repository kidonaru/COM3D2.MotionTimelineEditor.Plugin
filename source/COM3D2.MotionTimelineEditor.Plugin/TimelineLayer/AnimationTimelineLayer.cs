using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    [TimelineLayerDesc("メイドアニメブレンド", 1)]
    public partial class AnimationTimelineLayer : TimelineLayerBase
    {
        public override Type layerType => typeof(AnimationTimelineLayer);
        public override string layerName => nameof(AnimationTimelineLayer);

        public override bool hasSlotNo => true;

        public static string AnimationBoneName = "Animation";
        public static string AnimationDisplayName = "アニメレイヤー";

        public static int MinLayerIndex = 2;
        public static int MaxLayerIndex = 8;

        private List<string> _allBoneNames = null;
        public override List<string> allBoneNames
        {
            get
            {
                if (_allBoneNames == null)
                {
                    _allBoneNames = new List<string>();
                    for (int i = MinLayerIndex; i <= MaxLayerIndex; i++)
                    {
                        _allBoneNames.Add(AnimationBoneName + i);
                    }
                }
                return _allBoneNames;
            }
        }

        private static Dictionary<string, int> _animationLayerNameMap = null;
        public static Dictionary<string, int> AnimationLayerNameMap
        {
            get
            {
                if (_animationLayerNameMap == null)
                {
                    _animationLayerNameMap = new Dictionary<string, int>();
                    for (int i = MinLayerIndex; i <= MaxLayerIndex; i++)
                    {
                        _animationLayerNameMap.Add(AnimationBoneName + i, i);
                    }
                }
                return _animationLayerNameMap;
            }
        }

        private AnimationTimelineLayer(int slotNo) : base(slotNo)
        {
        }

        public static AnimationTimelineLayer Create(int slotNo)
        {
            return new AnimationTimelineLayer(slotNo);
        }

        protected override void InitMenuItems()
        {
            allMenuItems.Clear();

            for (int i = MinLayerIndex; i <= MaxLayerIndex; i++)
            {
                var menuItem = new BoneMenuItem(AnimationBoneName + i, AnimationDisplayName + i);
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

            if (!studioHackManager.isPoseEditing)
            {
                ApplyPlayData();
            }
        }

        protected override void ApplyMotion(MotionData motion, float t, bool indexUpdated, MotionPlayData playData)
        {
            var start = motion.start as TransformDataAnimation;
            var end = motion.end as TransformDataAnimation;

            var layer = AnimationLayerNameMap.GetOrDefault(motion.name);
            var info = maidCache.GetAnimationLayerInfo(layer);
            if (info == null)
            {
                return;
            }

            if (indexUpdated)
            {
                info.anmName = start.AnmName;
                info.startTime = start.StartTime;
                info.weight = start.Weight;
                info.speed = start.Speed;
                info.loop = start.Loop;
                info.overrideTime = start.OverrideTime;
            }

            if (start.Weight != end.Weight)
            {
                float easingTime = CalcEasingValue(t, motion.easing);
                var weight = Mathf.Lerp(start.Weight, end.Weight, easingTime);
                info.weight = weight;
            }

            // モーション編集中はinfoの更新まで
            if (timelineManager.isMotionEditing)
            {
                return;
            }

            var dt = CalcAnimationTime(playData, info);
            maidCache.ApplyAnimationLayerInfo(info, dt);
        }

        private float CalcAnimationTime(MotionPlayData playData, AnimationLayerInfo info)
        {
            if (playData == null || playData.current == null)
            {
                MTEUtils.LogDebug($"playData.current is null layer:{info.layer} index:{playData?.listIndex} prevPlayingFrame:{playData?.prevPlayingFrame}");
                return info.startTime;
            }

            var baseFrame = playData.current.stFrame;
            var baseStartTime = info.startTime;

            // 時間上書きではない場合、前のフレーム基準に再生時間を計算する
            if (!string.IsNullOrEmpty(info.anmName) && !info.overrideTime)
            {
                for (var i = playData.listIndex; i >= 0; i--)
                {
                    var prevMotion = playData.motions.GetOrDefault(i);
                    if (prevMotion == null)
                    {
                        continue;
                    }

                    var prevTrans = prevMotion.start as TransformDataAnimation;
                    if (prevTrans == null)
                    {
                        continue;
                    }

                    if (prevTrans.AnmName != info.anmName)
                    {
                        break;
                    }

                    baseFrame = prevMotion.stFrame;
                    baseStartTime = prevTrans.StartTime;
                }
            }

            var t0 = baseFrame * timeline.frameDuration;
            return baseStartTime + (playingTime - t0) * info.speed;
        }

        public override void OnPoseEditEnd()
        {
            // モーション編集中はアニメーションレイヤー無効化
            if (timelineManager.isMotionEditing)
            {
                var stateUpdated = false;
                foreach (var info in maidCache.animationLayerInfos)
                {
                    if (info.state != null && info.state.enabled && info.layer > 0)
                    {
                        info.state.enabled = false;
                        info.state = null;
                        stateUpdated = true;
                    }
                }

                // 更新した場合デフォルトレイヤーのみで再サンプル
                if (stateUpdated)
                {
                    maidCache.animationState.enabled = true;
                    maidCache.animation.Sample();
                    maidCache.animationState.enabled = false;
                }
            }

            // モーション編集終了後は強制反映
            if (!studioHack.isPoseEditing)
            {
                foreach (var playData in _playDataMap.Values)
                {
                    playData.ResetIndex();
                }
            }
        }

        public override void UpdateFrame(FrameData frame, bool initialEdit, bool force)
        {
            var maid = this.maid;
            if (maid == null)
            {
                MTEUtils.LogError("メイドが配置されていません");
                return;
            }

            for (int i = MinLayerIndex; i <= MaxLayerIndex; i++)
            {
                var info = maidCache.GetAnimationLayerInfo(i);
                if (info == null)
                {
                    continue;
                }

                var boneName = AnimationBoneName + i;
                var trans = frame.GetOrCreateTransformData<TransformDataAnimation>(boneName);
                trans.AnmName = info.anmName;
                trans.StartTime = info.startTime;
                trans.Weight = info.weight;
                trans.Speed = info.speed;
                trans.Loop = info.loop;
                trans.OverrideTime = info.overrideTime;
                trans.easing = GetEasing(frame.frameNo, boneName);
            }
        }

        public override void DrawWindow(GUIView view)
        {
            if (maidCache == null)
            {
                view.DrawLabel("メイドを配置してください", -1, 20);
                return;
            }

            view.SetEnabled(!view.IsComboBoxFocused());
            view.BeginScrollView();

            for (int layer = MinLayerIndex; layer <= MaxLayerIndex; layer++)
            {
                DrawAnimeLayer(view, layer);
            }

            view.SetEnabled(!view.IsComboBoxFocused());
            view.EndScrollView();
        }

        public void DrawAnimeLayer(GUIView view, int layer)
        {
            var info = maidCache.GetAnimationLayerInfo(layer);
            if (info == null)
            {
                view.DrawLabel("アニメーションレイヤーが見つかりません", -1, 20);
                return;
            }

            view.SetEnabled(!view.IsComboBoxFocused() && studioHackManager.isPoseEditing);

            var defaultTrans = TransformDataAnimation.defaultTrans;
            var updateTransform = false;

            var state = info.state;
            var length = 1f;
            if (state != null)
            {
                length = state.length;
            }

            view.BeginHorizontal();
            {
                var layerName = AnimationDisplayName + layer;
                view.DrawLabel(layerName, 100, 20);

                updateTransform |= view.DrawToggle("ループ", info.loop, 60, 20, newValue =>
                {
                    info.loop = newValue;
                });

                updateTransform |= view.DrawToggle("時間上書き", info.overrideTime, 80, 20, newValue =>
                {
                    info.overrideTime = newValue;
                });
            }
            view.EndLayout();

            updateTransform |= view.DrawTextField(new GUIView.TextFieldOption
            {
                value = info.anmName,
                onChanged = value => info.anmName = value,
            });

            updateTransform |= view.DrawSliderValue(new GUIView.SliderOption
            {
                label = "開始時間",
                labelWidth = 40,
                fieldType = FloatFieldType.Float,
                min = 0f,
                max = length,
                step = 0.01f,
                defaultValue = 0f,
                value = info.startTime,
                onChanged = newValue => info.startTime = newValue,
            });

            updateTransform |= view.DrawCustomValueFloat(
                defaultTrans.WeightInfo,
                info.weight,
                newValue => info.weight = newValue);

            updateTransform |= view.DrawCustomValueFloat(
                defaultTrans.SpeedInfo,
                info.speed,
                newValue => info.speed = newValue);

            if (updateTransform)
            {
                var playData = _playDataMap.GetOrDefault(AnimationBoneName + layer);
                var dt = CalcAnimationTime(playData, info);
                maidCache.ApplyAnimationLayerInfo(info, dt);
                maidCache.animation.Sample();
            }

            view.DrawHorizontalLine();
        }

        public override SingleFrameType GetSingleFrameType(TransformType transformType)
        {
            return SingleFrameType.None;
        }

        public override TransformType GetTransformType(string name)
        {
            return TransformType.Animation;
        }
    }
}