using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class IKHoldUI : SubWindowUIBase
    {
        public override string title
        {
            get
            {
                return "IK固定";
            }
        }

        private static MaidCache maidCache
        {
            get
            {
                return maidManager.maidCache;
            }
        }

        public IKHoldUI(SubWindow subWindow) : base(subWindow)
        {
        }

        public override void DrawContent(GUIView view)
        {
            if (timeline == null || maidCache == null)
            {
                return;
            }

            view.DrawToggle("IKのアニメ制御", timeline.isIKAnime, 150, 20, newValue =>
            {
                timeline.isIKAnime = newValue;
            });

            view.DrawHorizontalLine(Color.gray);

            view.SetEnabled(!view.IsComboBoxFocused() && studioHack.isPoseEditing);

            var typesList = new List<IKHoldType[]>
            {
                new IKHoldType[] { IKHoldType.Arm_L_Joint, IKHoldType.Arm_R_Joint },
                new IKHoldType[] { IKHoldType.Arm_L_Tip, IKHoldType.Arm_R_Tip },

                new IKHoldType[] { IKHoldType.Foot_L_Joint, IKHoldType.Foot_R_Joint },
                new IKHoldType[] { IKHoldType.Foot_L_Tip, IKHoldType.Foot_R_Tip },
            };

            foreach (var types in typesList)
            {
                view.BeginHorizontal();

                foreach (var type in types)
                {
                    var name = IKHoldEntity.GetHoldTypeName(type);
                    var isHold = IsHold(type);
                    view.DrawToggle(name, isHold, 80, 20, newValue =>
                    {
                        SetHold(type, newValue);
                    });
                }

                var isHolds = types.All(x => IsHold(x));
                if (isHolds)
                {
                    if (view.DrawButton("解除", 50, 20))
                    {
                        foreach (var type in types)
                        {
                            SetHold(type, false);
                        }
                    }
                }
                else
                {
                    if (view.DrawButton("固定", 50, 20))
                    {
                        foreach (var type in types)
                        {
                            SetHold(type, true);
                        }
                    }
                }

                view.EndLayout();
            }

            view.BeginHorizontal();
            {
                view.AddSpace(80, 20);
                view.AddSpace(80, 20);

                var isAllHold = MaidCache.ikHoldTypeMap.Values.All(x => IsHold(x));
                if (isAllHold)
                {
                    if (view.DrawButton("全解除", 80, 20))
                    {
                        foreach (var holdType in MaidCache.ikHoldTypeMap.Values)
                        {
                            SetHold(holdType, false);
                        }
                    }
                }
                else
                {
                    if (view.DrawButton("全固定", 80, 20))
                    {
                        foreach (var holdType in MaidCache.ikHoldTypeMap.Values)
                        {
                            SetHold(holdType, true);
                        }
                    }
                }
            }
            view.EndLayout();

            view.DrawHorizontalLine(Color.gray);

            bool paramUpdated = false;
            bool isFootHold = IsHold(IKHoldType.Foot_L_Tip) || IsHold(IKHoldType.Foot_R_Tip);

            view.BeginHorizontal();
            {
                view.DrawToggle("足の接地処理", maidCache.isFootGrounding, 120, 20, newValue =>
                {
                    maidCache.isFootGrounding = newValue;
                    paramUpdated = true;
                });

                if (maidCache.isFootGrounding && !isFootHold)
                {
                    view.DrawLabel("足首の固定化が必要です", -1, 20, Color.yellow);
                }
            }
            view.EndLayout();

            view.SetEnabled(!view.IsComboBoxFocused() && studioHack.isPoseEditing && maidCache.isFootGrounding && isFootHold);

            paramUpdated |= view.DrawSliderValue(
                new GUIView.SliderOption
                {
                    label = "床の高さ",
                    labelWidth = 60,
                    min = -config.positionRange,
                    max = config.positionRange,
                    step = 0.01f,
                    defaultValue = 0f,
                    value = maidCache.floorHeight,
                    onChanged = newValue => maidCache.floorHeight = newValue,
                });

            if (view.DrawButton("メイドの位置から推定", 150, 20))
            {
                if (maidCache != null)
                {
                    var footL = maidCache.ikManager.GetBone(IKManager.BoneType.Foot_L);
                    var footR = maidCache.ikManager.GetBone(IKManager.BoneType.Foot_R);
                    if (footL != null && footR != null)
                    {
                        maidCache.floorHeight = (footL.transform.position.y + footR.transform.position.y) / 2f - maidCache.footBaseOffset;
                        paramUpdated = true;
                    }
                }
            }

            paramUpdated |= view.DrawSliderValue(
                new GUIView.SliderOption
                {
                    label = "足の高さ",
                    labelWidth = 60,
                    min = 0f,
                    max = 1f,
                    step = 0.01f,
                    defaultValue = 0.05f,
                    value = maidCache.footBaseOffset,
                    onChanged = newValue => maidCache.footBaseOffset = newValue,
                });
            
            paramUpdated |= view.DrawSliderValue(
                new GUIView.SliderOption
                {
                    label = "伸ばす高さ",
                    labelWidth = 60,
                    min = 0f,
                    max = 1f,
                    step = 0.01f,
                    defaultValue = 0.1f,
                    value = maidCache.footStretchHeight,
                    onChanged = newValue => maidCache.footStretchHeight = newValue,
                });
            
            paramUpdated |= view.DrawSliderValue(
                new GUIView.SliderOption
                {
                    label = "伸ばす角度",
                    labelWidth = 60,
                    min = -180f,
                    max = 180f,
                    step = 1f,
                    defaultValue = 45f,
                    value = maidCache.footStretchAngle,
                    onChanged = newValue => maidCache.footStretchAngle = newValue,
                });
            
            paramUpdated |= view.DrawSliderValue(
                new GUIView.SliderOption
                {
                    label = "接地時角度",
                    labelWidth = 60,
                    min = -180f,
                    max = 180f,
                    step = 1f,
                    defaultValue = 90f,
                    value = maidCache.footGroundAngle,
                    onChanged = newValue => maidCache.footGroundAngle = newValue,
                });

            if (paramUpdated)
            {
                ResetTargetPosition(IKHoldType.Foot_L_Tip);
                ResetTargetPosition(IKHoldType.Foot_R_Tip);
            }
        }

        private void ResetTargetPosition(IKHoldType type)
        {
            var idHoldEntity = maidCache.GetIKHoldEntity(type);
            if (idHoldEntity != null)
            {
                idHoldEntity.ResetTargetPosition();
            }
        }

        private void SetHold(IKHoldType type, bool hold)
        {
            var idHoldEntity = maidCache.GetIKHoldEntity(type);
            if (idHoldEntity == null || idHoldEntity.isHold == hold)
            {
                return;
            }

            idHoldEntity.isHold = hold;
            idHoldEntity.ResetTargetPosition();
        }

        private bool IsHold(IKHoldType type)
        {
            return maidCache.GetIKHoldEntity(type).isHold;
        }
    }
}