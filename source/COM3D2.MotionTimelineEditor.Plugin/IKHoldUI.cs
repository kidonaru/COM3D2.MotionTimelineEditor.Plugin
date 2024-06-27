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

        private bool[] isHoldList
        {
            get
            {
                return timeline.isHoldList;
            }
        }

        private static IKHoldManager ikHoldManager
        {
            get
            {
                return IKHoldManager.instance;
            }
        }

        public IKHoldUI(SubWindow subWindow) : base(subWindow)
        {
        }

        public override void DrawContent(GUIView view)
        {
            if (timeline == null)
            {
                return;
            }

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
                    var name = ikHoldManager.GetHoldTypeName(type);
                    var isHold = ikHoldManager.IsHold(type);
                    view.DrawToggle(name, isHold, 80, 20, newValue =>
                    {
                        ikHoldManager.SetHold(type, newValue);
                    });
                }

                var isHolds = types.All(x => ikHoldManager.IsHold(x));
                if (isHolds)
                {
                    if (view.DrawButton("解除", 50, 20))
                    {
                        foreach (var type in types)
                        {
                            ikHoldManager.SetHold(type, false);
                        }
                    }
                }
                else
                {
                    if (view.DrawButton("固定", 50, 20))
                    {
                        foreach (var type in types)
                        {
                            ikHoldManager.SetHold(type, true);
                        }
                    }
                }

                view.EndLayout();
            }

            view.BeginHorizontal();
            {
                view.AddSpace(80, 20);
                view.AddSpace(80, 20);

                var isAllHold = isHoldList.All(x => x);
                if (isAllHold)
                {
                    if (view.DrawButton("全解除", 80, 20))
                    {
                        for (int i = 0; i < isHoldList.Length; i++)
                        {
                            ikHoldManager.SetHold((IKHoldType) i, false);
                        }
                    }
                }
                else
                {
                    if (view.DrawButton("全固定", 80, 20))
                    {
                        for (int i = 0; i < isHoldList.Length; i++)
                        {
                            ikHoldManager.SetHold((IKHoldType) i, true);
                        }
                    }
                }
            }
            view.EndLayout();

            view.DrawHorizontalLine(Color.gray);

            bool paramUpdated = false;
            bool isHoldFoot = ikHoldManager.IsHold(IKHoldType.Foot_L_Tip) || ikHoldManager.IsHold(IKHoldType.Foot_R_Tip);

            view.BeginHorizontal();
            {
                view.DrawToggle("足の接地処理", timeline.isFootGrounding, 120, 20, newValue =>
                {
                    timeline.isFootGrounding = newValue;
                    paramUpdated = true;
                });

                if (timeline.isFootGrounding && !isHoldFoot)
                {
                    view.DrawLabel("足首の固定化が必要です", -1, 20, Color.yellow);
                }
            }
            view.EndLayout();

            view.SetEnabled(!view.IsComboBoxFocused() && timeline.isFootGrounding && isHoldFoot);

            paramUpdated |= view.DrawSliderValue(
                new GUIView.SliderOption
                {
                    label = "床の高さ",
                    labelWidth = 60,
                    min = -config.positionRange,
                    max = config.positionRange,
                    step = 0.01f,
                    defaultValue = 0f,
                    value = timeline.floorHeight,
                    onChanged = newValue => timeline.floorHeight = newValue,
                });

            if (view.DrawButton("メイドの位置から推定", 150, 20))
            {
                var maidCache = maidManager.maidCache;
                if (maidCache != null)
                {
                    var footL = maidCache.ikManager.GetBone(IKManager.BoneType.Foot_L);
                    var footR = maidCache.ikManager.GetBone(IKManager.BoneType.Foot_R);
                    if (footL != null && footR != null)
                    {
                        timeline.floorHeight = (footL.transform.position.y + footR.transform.position.y) / 2f - timeline.footBaseOffset;
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
                    value = timeline.footBaseOffset,
                    onChanged = newValue => timeline.footBaseOffset = newValue,
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
                    value = timeline.footStretchHeight,
                    onChanged = newValue => timeline.footStretchHeight = newValue,
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
                    value = timeline.footStretchAngle,
                    onChanged = newValue => timeline.footStretchAngle = newValue,
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
                    value = timeline.footGroundAngle,
                    onChanged = newValue => timeline.footGroundAngle = newValue,
                });

            if (paramUpdated)
            {
                ikHoldManager.RequestUpdatePosition();
            }
        }
    }
}