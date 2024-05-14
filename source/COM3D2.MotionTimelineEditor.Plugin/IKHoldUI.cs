using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    using MTE = MotionTimelineEditor;

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

    public class IKHoldUI : SubWindowUIBase
    {
        public override string title
        {
            get
            {
                return "IK固定";
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

        public Vector3[] initialEditIkPositions = new Vector3[(int) IKHoldType.Max]
        {
            Vector3.zero,
            Vector3.zero,
            Vector3.zero,
            Vector3.zero,
            Vector3.zero,
            Vector3.zero,
            Vector3.zero,
            Vector3.zero,
        };

        private bool resetPositionRequested = false;
        private bool positionUpdated = false;

        private static StudioHackBase studioHack
        {
            get
            {
                return MTE.studioHack;
            }
        }

        private bool[] isHoldList
        {
            get
            {
                return timeline.isHoldList;
            }
        }

        private Vector3[] prevIkPositions = new Vector3[(int) IKHoldType.Max]
        {
            Vector3.zero,
            Vector3.zero,
            Vector3.zero,
            Vector3.zero,
            Vector3.zero,
            Vector3.zero,
            Vector3.zero,
            Vector3.zero,
        };

        public override void Init()
        {
            TimelineManager.onEditPoseUpdated += OnEditPoseUpdated;
        }

        public override void Update()
        {
            if (!studioHack.isPoseEditing)
            {
                return;
            }

            if (resetPositionRequested)
            {
                return;
            }

            for (int i = 0; i < isHoldList.Length; i++)
            {
                if (!isHoldList[i])
                {
                    continue;
                }

                var ikPosition = maidManager.GetIkPosition((IKHoldType) i);
                if (prevIkPositions[i] != ikPosition)
                {
                    //PluginUtils.LogDebug("IKHoldUI：UpdateIkPosition: " + (IKHoldType) i);
                    //PluginUtils.LogDebug("  current: " + ikPosition + "  target: " + initialEditIkPositions[i]);
                    maidManager.UpdateIkPosition((IKHoldType) i, initialEditIkPositions[i]);
                    positionUpdated = true;
                }
            }
        }

        public override void LateUpdate()
        {
            if (resetPositionRequested)
            {
                //PluginUtils.LogDebug("IKHoldUI：ResetPosition");

                for (int i = 0; i < initialEditIkPositions.Length; i++)
                {
                    prevIkPositions[i] = initialEditIkPositions[i] = maidManager.GetIkPosition((IKHoldType)i);
                }
                resetPositionRequested = false;
            }

            if (positionUpdated)
            {
                for (int i = 0; i < isHoldList.Length; i++)
                {
                    maidManager.PositonCorrection((IKHoldType) i);
                    if (isHoldList[i])
                    {
                        prevIkPositions[i] = maidManager.GetIkPosition((IKHoldType) i);
                    }
                }
                positionUpdated = false;
            }
        }

        public override void DrawWindow(int id)
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
                            SetHold((IKHoldType) i, false);
                        }
                    }
                }
                else
                {
                    if (view.DrawButton("全固定", 80, 20))
                    {
                        for (int i = 0; i < isHoldList.Length; i++)
                        {
                            SetHold((IKHoldType) i, true);
                        }
                    }
                }
            }

            GUI.DragWindow();
        }

        private void OnEditPoseUpdated()
        {
            resetPositionRequested = true;
        }

        private void SetHold(IKHoldType type, bool hold)
        {
            if (isHoldList[(int)type] == hold)
            {
                return;
            }

            isHoldList[(int)type] = hold;
            if (hold)
            {
                initialEditIkPositions[(int)type] = maidManager.GetIkPosition(type);
            }
        }

        private bool IsHold(IKHoldType type)
        {
            return isHoldList[(int)type];
        }

        private string GetHoldTypeName(IKHoldType type)
        {
            return IKHoldTypeNames[(int)type];
        }
    }
}