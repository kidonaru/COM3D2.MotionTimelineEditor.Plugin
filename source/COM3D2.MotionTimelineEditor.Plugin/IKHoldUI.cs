using System;
using System.Collections.Generic;
using System.Linq;

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

            view.DrawLabel("IKを固定して中心点を移動できます", -1, 20);

            var typesList = new List<IKHoldType[]>
            {
                new IKHoldType[] { IKHoldType.Arm_L_Joint, IKHoldType.Arm_R_Joint },
                new IKHoldType[] { IKHoldType.Arm_L_Tip, IKHoldType.Arm_R_Tip },

                new IKHoldType[] { IKHoldType.Foot_L_Joint, IKHoldType.Foot_R_Joint },
                new IKHoldType[] { IKHoldType.Foot_L_Tip, IKHoldType.Foot_R_Tip },
            };

            foreach (var types in typesList)
            {
                view.BeginLayout(GUIView.LayoutDirection.Horizontal);

                foreach (var type in types)
                {
                    var name = ikHoldManager.GetHoldTypeName(type);
                    var isHold = ikHoldManager.IsHold(type);
                    var isHoldNew = view.DrawToggle(name, isHold, 80, 20);
                    ikHoldManager.SetHold(type, isHoldNew);
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

            view.AddSpace(10);

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
    }
}