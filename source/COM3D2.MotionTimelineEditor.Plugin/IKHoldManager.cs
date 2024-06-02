using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
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

    public class IKHoldManager
    {
        private static IKHoldManager _instance = null;
        public static IKHoldManager instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new IKHoldManager();
                }
                return _instance;
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

        private static MaidManager maidManager
        {
            get
            {
                return MaidManager.instance;
            }
        }

        private static TimelineData timeline
        {
            get
            {
                return TimelineManager.instance.timeline;
            }
        }

        private static StudioHackBase studioHack
        {
            get
            {
                return StudioHackManager.studioHack;
            }
        }

        private IKHoldManager()
        {
        }

        public void Init()
        {
            TimelineManager.onEditPoseUpdated += OnEditPoseUpdated;
        }

        public void Update()
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

        public void LateUpdate()
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

        public void SetHold(IKHoldType type, bool hold)
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

        public bool IsHold(IKHoldType type)
        {
            return isHoldList[(int)type];
        }

        public string GetHoldTypeName(IKHoldType type)
        {
            return IKHoldTypeNames[(int)type];
        }

        private void OnEditPoseUpdated()
        {
            resetPositionRequested = true;
        }
    }
}