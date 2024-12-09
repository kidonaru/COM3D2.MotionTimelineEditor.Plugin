using System;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    [TimelineLayerDesc("ポストエフェクト", 44)]
    public partial class PostEffectTimelineLayer : TimelineLayerBase
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
                    _allBoneNames.AddRange(rimlightNames);
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
            InitParrifinEffect();
            InitDistanceFogEffect();
            InitRimlightEffect();

            base.Init();

            AddFirstBones(allBoneNames);
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

            var boneCount = 1
                + timeline.paraffinCount
                + timeline.distanceFogCount
                + timeline.rimlightCount;
            if (allBoneNames.Count != boneCount)
            {
                _allBoneNames = null;
                InitParrifinEffect();
                InitDistanceFogEffect();
                InitRimlightEffect();
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
                case TransformType.Rimlight:
                    ApplyRimlight(motion, t);
                    break;
            }
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
                    case PostEffectType.Rimlight:
                    {
                        var trans = CreateTransformData<TransformDataRimlight>(effectName);
                        trans.rimlight = postEffectManager.GetRimlightData(trans.index);
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

        private ColorFieldCache _color1FieldValue = new ColorFieldCache("Color1", true);
        private ColorFieldCache _color2FieldValue = new ColorFieldCache("Color2", true);

        private enum TabType
        {
            被写界深度,
            パラフィン,
            距離フォグ,
            リムライト,
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
                case TabType.リムライト:
                    DrawRimlight(view);
                    break;
            }

            view.DrawComboBox();
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
                case PostEffectType.Rimlight:
                    return TransformType.Rimlight;
            }

            return TransformType.None;
        }
    }
}