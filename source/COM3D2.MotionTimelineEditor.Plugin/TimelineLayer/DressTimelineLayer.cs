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
                        _allBoneNames.Add(MaidPartUtils.GetMaidPartName(maidPartType));
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

            Action<string, string, List<DressSlotID>> addCategory = (setName, setJpName, slotIds) =>
            {
                var setMenuItem = new BoneSetMenuItem(setName, setJpName);
                _allMenuItems.Add(setMenuItem);

                foreach (var slotId in slotIds)
                {
                    var displayName = DressUtils.GetDressSlotJpName(slotId);
                    var menuItem = new BoneMenuItem(slotId.ToString(), displayName);
                    setMenuItem.AddChild(menuItem);
                }
            };

            addCategory("clothing", "衣装", DressUtils.ClothingSlotIds);
            addCategory("headwear", "頭部衣装", DressUtils.HeadwearSlotIds);
            addCategory("accessory", "アクセ", DressUtils.AccessorySlotIds);
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

        protected override void ApplyMotion(MotionData motion, float t, bool indexUpdated)
        {
            var start = motion.start as TransformDataDress;
            if (string.IsNullOrEmpty(start.propName))
            {
                return;
            }

            if (indexUpdated)
            {
                var maidPartType = MaidPartUtils.ToMaidPartType(motion.name);
                var mpn = maidPartType.ToMPN();
                var prop = maid.GetProp(mpn);

                if (prop != null && prop.strFileName != start.propName)
                {
                    maid.SetProp(mpn, start.propName, start.rid);
                    _propUpdated = true;
                }
            }
        }

        public override void UpdateFrame(FrameData frame, bool initialEdit)
        {
            if (initialEdit)
            {
                var playingFrameNo = this.playingFrameNo;
                foreach (var boneName in allBoneNames)
                {
                    var prevBone = GetPrevBone(playingFrameNo + 1, boneName);
                    if (prevBone == null)
                    {
                        continue;
                    }

                    var trans = frame.GetOrCreateTransformData<TransformDataDress>(boneName);
                    trans.FromTransformData(prevBone.transform);
                }
                return;
            }

            foreach (var maidPartType in MaidPartUtils.equippableMaidPartTypes)
            {
                var mpn = maidPartType.ToMPN();
                var name = MaidPartUtils.GetMaidPartName(maidPartType);
                var prop = maid.GetProp(mpn);
                if (prop == null)
                {
                    continue;
                }

                var initialPropName = maidCache.maidPropCache.GetInitialPropName(maidPartType);
                if (prop.strFileName == initialPropName)
                {
                    continue;
                }

                var trans = frame.GetOrCreateTransformData<TransformDataDress>(name);
                trans.propName = prop.strFileName;
                trans.rid = prop.nFileNameRID;
            }
        }

        private enum TabType
        {
            操作,
        }

        private TabType _tabType = TabType.操作;

        public override void DrawWindow(GUIView view)
        {
            _tabType = view.DrawTabs(_tabType, 50, 20);

            switch (_tabType)
            {
                case TabType.操作:
                    DrawDress(view);
                    break;
            }

            view.DrawComboBox();
        }

        public void DrawDress(GUIView view)
        {
            view.SetEnabled(!view.IsComboBoxFocused());

            view.BeginScrollView();
            {
                view.BeginHorizontal();
                {
                    if (view.DrawButton("初期化", 60, 20))
                    {
                        maidCache.maidPropCache.ApplyInitialProp();
                    }

                    view.AddSpace(10);

                    if (view.DrawButton("初期値更新", 100, 20))
                    {
                        maidCache.maidPropCache.UpdateInitialProp();
                    }
                }
                view.EndLayout();

                view.DrawHorizontalLine();

                foreach (var pair in MaidPartUtils.maidPartJpNameMap)
                {
                    var maidPartType = pair.Key;
                    if (!MaidPartUtils.IsEquippableType(maidPartType))
                    {
                        continue;
                    }

                    var displayName = pair.Value;
                    var mpn = maidPartType.ToMPN();
                    var prop = maid.GetProp(mpn);
                    if (prop == null)
                    {
                        continue;
                    }

                    var initialPropName = maidCache.maidPropCache.GetInitialPropName(maidPartType);
                    var color = prop.strFileName == initialPropName ? Color.white : Color.green;

                    view.DrawLabel($"{displayName}: {prop.strFileName}", -1, 20, color);
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