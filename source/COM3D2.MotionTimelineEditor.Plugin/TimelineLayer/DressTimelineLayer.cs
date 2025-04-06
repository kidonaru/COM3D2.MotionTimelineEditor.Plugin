using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    [TimelineLayerDesc("メイド衣装", 15)]
    public class DressTimelineLayer : TimelineLayerBase
    {
        public override Type layerType => typeof(DressTimelineLayer);
        public override string layerName => nameof(DressTimelineLayer);

        private List<string> _allBoneNames = null;
        public override List<string> allBoneNames
        {
            get
            {
                if (_allBoneNames == null)
                {
                    _allBoneNames = new List<string>(MaidPartUtils.equippableMaidPartTypes.Count);
                    foreach (var maidPartType in MaidPartUtils.equippableMaidPartTypes)
                    {
                        _allBoneNames.Add(maidPartType.ToName());
                    }
                }

                return _allBoneNames;
            }
        }

        private DressTimelineLayer(int slotNo) : base(slotNo)
        {
        }

        public static DressTimelineLayer Create(int slotNo)
        {
            return new DressTimelineLayer(slotNo);
        }

        protected override void InitMenuItems()
        {
            _allMenuItems.Clear();

            var categoryMenuItems = new Dictionary<MaidPartCategory, BoneSetMenuItem>();

            foreach (var pair in MaidPartUtils.maidPartCategoryJpNameMap)
            {
                var category = pair.Key;
                var categoryName = category.ToName();
                var categoryJpName = pair.Value;

                if (category == MaidPartCategory.None || category == MaidPartCategory.Set)
                {
                    continue;
                }

                var categoryMenuItem = new BoneSetMenuItem(categoryName, categoryJpName);
                _allMenuItems.Add(categoryMenuItem);

                categoryMenuItems.Add(category, categoryMenuItem);
            }

            foreach (var pair in MaidPartUtils.maidPartNameMap)
            {
                var maidPartType = pair.Key;
                var boneName = pair.Value;
                var displayName = maidPartType.ToJpName();
                var category = maidPartType.ToCategory();

                if (category == MaidPartCategory.None || category == MaidPartCategory.Set)
                {
                    continue;
                }

                var categoryMenuItem = categoryMenuItems[category];
                var menuItem = new BoneMenuItem(boneName, displayName);
                categoryMenuItem.AddChild(menuItem);
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

        private bool _propUpdated = false;

        protected override void ApplyPlayData()
        {
            var maid = this.maid;
            if (maid == null)
            {
                return;
            }

            _propUpdated = false;
            base.ApplyPlayData();

            if (_propUpdated)
            {
                MTEUtils.LogDebug("DressTimelineLayer: AllProcPropSeqStart");
                maid.AllProcPropSeqStart();
            }
        }

        protected override void ApplyMotion(MotionData motion, float t, bool indexUpdated, MotionPlayData playData)
        {
            var start = motion.start as TransformDataDress;
            if (string.IsNullOrEmpty(start.propName))
            {
                return;
            }

            if (indexUpdated)
            {
                var maidPartType = motion.name.ToMaidPartType();
                var mpn = maidPartType.ToMPN();
                var prop = maid.GetProp(mpn);

                if (prop != null && prop.strFileName != start.propName)
                {
                    maid.SetProp(mpn, start.propName, start.rid);
                    _propUpdated = true;
                }
            }
        }

        public override void UpdateFrame(FrameData frame, bool initialEdit, bool force)
        {
            if (initialEdit)
            {
                foreach (var maidPartType in MaidPartUtils.equippableMaidPartTypes)
                {
                    var boneName = maidPartType.ToName();
                    var prevBone = GetPrevBone(playingFrameNo + 1, boneName);
                    var initialPropInfo = maidCache.maidPropCache.GetInitialPropInfo(maidPartType);

                    // 前のフレームが存在する場合登録
                    if (prevBone != null)
                    {
                        var prevTrans = prevBone.transform as TransformDataDress;
                        var trans = frame.GetOrCreateTransformData<TransformDataDress>(boneName);
                        trans.propName = prevTrans.propName;
                        trans.rid = prevTrans.rid;
                    }
                    // 存在しない場合、初期状態を登録
                    else
                    {
                        var trans = frame.GetOrCreateTransformData<TransformDataDress>(boneName);
                        trans.propName = initialPropInfo.propName;
                        trans.rid = initialPropInfo.rid;
                    }
                }
                return;
            }

            foreach (var maidPartType in MaidPartUtils.equippableMaidPartTypes)
            {
                var mpn = maidPartType.ToMPN();
                var prop = maid.GetProp(mpn);
                if (prop == null)
                {
                    continue;
                }

                var boneName = maidPartType.ToName();
                var prevBone = GetPrevBone(playingFrameNo + 1, boneName);
                var initialPropInfo = maidCache.maidPropCache.GetInitialPropInfo(maidPartType);

                // 前のフレームが存在する場合か、初期状態からの差分があれば登録
                if (force || prevBone != null || prop.strFileName != initialPropInfo.propName)
                {
                    var trans = frame.GetOrCreateTransformData<TransformDataDress>(boneName);
                    trans.propName = prop.strFileName;
                    trans.rid = prop.nFileNameRID;
                }
            }
        }

        public override void DrawWindow(GUIView view)
        {
            DrawDress(view);
            view.DrawComboBox();
        }

        public void DrawDress(GUIView view)
        {
            view.SetEnabled(!view.IsComboBoxFocused());

            view.BeginHorizontal();
            {
                if (view.DrawButton("初期化", 60, 20))
                {
                    maidCache.maidPropCache.ApplyInitialProp();
                }

                if (view.DrawButton("初期値更新", 100, 20))
                {
                    maidCache.maidPropCache.UpdateInitialProp();
                }
            }
            view.EndLayout();

            view.DrawHorizontalLine();
            view.AddSpace(5);

            view.BeginScrollView();
            {
                var prevCategory = MaidPartCategory.None;

                foreach (var pair in MaidPartUtils.maidPartCategoryMap)
                {
                    var maidPartType = pair.Key;
                    var category = pair.Value;
                    if (category == MaidPartCategory.None || category == MaidPartCategory.Set)
                    {
                        continue;
                    }

                    if (category != prevCategory)
                    {
                        view.DrawHorizontalLine();
                        view.DrawLabel(category.ToJpName(), -1, 20);

                        prevCategory = category;
                    }

                    var mpn = maidPartType.ToMPN();
                    var prop = maid.GetProp(mpn);
                    if (prop == null)
                    {
                        continue;
                    }

                    var initialPropInfo = maidCache.maidPropCache.GetInitialPropInfo(maidPartType);
                    var color = prop.strFileName == initialPropInfo.propName ? Color.white : Color.green;

                    view.DrawLabel($"{maidPartType.ToJpName()}: {prop.strFileName}", -1, 20, color);
                }

                view.DrawHorizontalLine();
            }
            view.EndScrollView();
        }

        public override TransformType GetTransformType(string name)
        {
            return TransformType.Dress;
        }
    }
}