using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    using SH = StudioHack;

    public enum IKHoldType
    {
        Arm_R_Joint,
        Arm_R_Tip,
        Arm_L_Joint,
        Arm_L_Tip,
        Foot_R_Joint,
        Foot_R_Tip,
        Foot_L_Joint,
        Foot_L_Tip,
        Max,
    }

    public class IKHoldUI : ISubWindowUI
    {
        public string title
        {
            get
            {
                return "IK固定";
            }
        }

        public static int WINDOW_WIDTH
        {
            get
            {
                return SubWindow.WINDOW_WIDTH;
            }
        }
        public static int WINDOW_HEIGHT
        {
            get
            {
                return SubWindow.WINDOW_HEIGHT;
            }
        }

        private readonly static string[] IKHoldTypeNames = new string[(int) IKHoldType.Max]
        {
            "肘(右)",
            "手首(右)",
            "肘(左)",
            "手首(左)",
            "膝(右)",
            "足首(右)",
            "膝(左)",
            "足首(左)",
        };

        private Vector3[] initialEditIkPositions 
        {
            get
            {
                return timelineManager.initialEditIkPositions;
            }
        }

        public bool isDrag 
        {
            get
            {
                return IKDragPoint.IsDrag;
            }
        }

        private static TimelineManager timelineManager
        {
            get
            {
                return TimelineManager.instance;
            }
        }

        private static TimelineData timeline
        {
            get
            {
                return timelineManager.timeline;
            }
        }

        private bool[] isHoldList
        {
            get
            {
                return timeline.isHoldList;
            }
        }

        public void OnOpen()
        {
        }

        public void OnClose()
        {
            if (isPrevDrag)
            {
                OnDragEnd();
                isPrevDrag = false;
            }
        }

        public void Update()
        {
            if (!isPrevDrag && isDrag)
            {
                OnDragStart();
            }
            if (isPrevDrag && !isDrag)
            {
                OnDragEnd();
            }

            isPrevDrag = isDrag;

            if (isDrag)
            {
                for (int i = 0; i < isHoldList.Length; i++)
                {
                    if (isHoldList[i])
                    {
                        var dragPoint = SH.GetDragPoint((IKHoldType)i);
                        if (dragPoint != null)
                        {
                            dragPoint.transform.position = initialEditIkPositions[i];
                        }
                    }
                }
            }
        }

        public void SetHold(IKHoldType type, bool hold)
        {
            isHoldList[(int)type] = hold;
        }

        public bool IsHold(IKHoldType type)
        {
            return isHoldList[(int)type];
        }

        public string GetHoldTypeName(IKHoldType type)
        {
            return IKHoldTypeNames[(int)type];
        }

        bool isPrevDrag = false;

        public void OnDragStart()
        {
            //Extensions.Log("IKHoldUI：OnDragStart");
            for (int i = 0; i < isHoldList.Length; i++)
            {
                if (isHoldList[i])
                {
                    var dragPoint = SH.GetDragPoint((IKHoldType)i);
                    if (dragPoint != null)
                    {
                        dragPoint.drag_start_event.Invoke();
                    }
                }
            }
        }

        public void OnDragEnd()
        {
            //Extensions.Log("IKHoldUI：OnDragEnd");
            for (int i = 0; i < isHoldList.Length; i++)
            {
                if (isHoldList[i])
                {
                    var dragPoint = SH.GetDragPoint((IKHoldType)i);
                    if (dragPoint != null)
                    {
                        dragPoint.drag_end_event.Invoke();
                    }
                }
            }
        }

        public void DrawWindow(int id)
        {
            {
                var view = new GUIView(0, 20, WINDOW_WIDTH, WINDOW_HEIGHT - 20);

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
                        var name = GetHoldTypeName(type);
                        var isHold = IsHold(type);
                        var isHoldNew = view.DrawToggle(name, isHold, 80, 20);
                        SetHold(type, isHoldNew);
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

                view.AddSpace(10);

                var isAllHold = isHoldList.All(x => x);
                if (isAllHold)
                {
                    if (view.DrawButton("全解除", 80, 20))
                    {
                        for (int i = 0; i < isHoldList.Length; i++)
                        {
                            isHoldList[i] = false;
                        }
                    }
                }
                else
                {
                    if (view.DrawButton("全固定", 80, 20))
                    {
                        for (int i = 0; i < isHoldList.Length; i++)
                        {
                            isHoldList[i] = true;
                        }
                    }
                }
            }

            GUI.DragWindow();
        }
    }
}