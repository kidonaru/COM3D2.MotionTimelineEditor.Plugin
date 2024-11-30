using RootMotion.FinalIK;
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

    public class IKHoldEntity
    {
        public IKHoldType holdType;
        public Vector3 targetPosition;
        public bool isHold;
        public bool isAnime;
        public bool resetTargetPositionRequested;

        private MaidCache maidCache;
        private LimbControl limbControl;

        public IKManager.BoneType boneType
        {
            get
            {
                return holdType.ConvertBoneType();
            }
        }

        public GameObject bone
        {
            get
            {
                return ikManager.GetBone(boneType);
            }
        }

        public IKManager ikManager
        {
            get
            {
                return maidCache.ikManager;
            }
        }

        public FABRIK fabrik
        {
            get
            {
                return limbControl != null ? limbControl.GetIkFabrik() : null;
            }
        }

        public IKDragPoint dragPoint
        {
            get
            {
                switch (holdType)
                {
                    case IKHoldType.Arm_R_Joint:
                    case IKHoldType.Arm_L_Joint:
                    case IKHoldType.Foot_R_Joint:
                    case IKHoldType.Foot_L_Joint:
                        return limbControl.GetJointDragPoint();
                    case IKHoldType.Arm_R_Tip:
                    case IKHoldType.Arm_L_Tip:
                    case IKHoldType.Foot_R_Tip:
                    case IKHoldType.Foot_L_Tip:
                        return limbControl.GetTipDragPoint();
                }
                return null;
            }
        }

        public Vector3 position
        {
            get
            {
                var dragPoint = this.dragPoint;
                if (dragPoint != null && dragPoint.target_ik_point_trans != null)
                {
                    return dragPoint.target_ik_point_trans.position;
                }
                return Vector3.zero;
            }
        }

        public Transform transform
        {
            get
            {
                var dragPoint = this.dragPoint;
                if (dragPoint != null)
                {
                    return dragPoint.transform;
                }
                return null;
            }
        }

        public bool isDragging
        {
            get
            {
                return studioHack.IsIKDragging(holdType);
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

        private static MaidManager maidManager => MaidManager.instance;

        private static TimelineData timeline
        {
            get
            {
                return TimelineManager.instance.timeline;
            }
        }

        private static StudioHackBase studioHack => StudioHackManager.studioHack;

        public IKHoldEntity(IKHoldType holdType, MaidCache maidCache)
        {
            this.holdType = holdType;
            this.maidCache = maidCache;
            this.limbControl = GetLimbControl(holdType);
        }

        public bool isFootGroundingEnabled
        {
            get
            {
                if (holdType == IKHoldType.Foot_L_Tip && maidCache.isGroundingFootL)
                {
                    return true;
                }

                if (holdType == IKHoldType.Foot_R_Tip && maidCache.isGroundingFootR)
                {
                    return true;
                }

                return false;
            }
        }

        public void LateUpdate()
        {
            if (isHold)
            {
                if (!isAnime && !studioHack.isPoseEditing)
                {
                    return;
                }

                if (isDragging)
                {
                    this.targetPosition = position;
                    return;
                }

                if (resetTargetPositionRequested)
                {
                    this.targetPosition = position;
                    this.resetTargetPositionRequested = false;
                }

                var targetPosition = this.targetPosition;
                if (isFootGroundingEnabled)
                {
                    targetPosition.y = maidCache.floorHeight + maidCache.footBaseOffset;
                }

                //PluginUtils.LogDebug("UpdateIKPosition: " + holdType);
                //PluginUtils.LogDebug("  target: " + targetPosition);
                UpdateIKPosition(targetPosition);

                if (isFootGroundingEnabled)
                {
                    AdjustFootGrounding();
                }

                PositonCorrection();
            }
        }

        public static string GetHoldTypeName(IKHoldType type)
        {
            return IKHoldTypeNames[(int)type];
        }

        public void UpdateIKPosition(Vector3 targetPosition)
        {
            var limbControl = this.limbControl;
            var ikFabrik = this.fabrik;
            var dragPoint = this.dragPoint;

            if (limbControl != null && ikFabrik != null && dragPoint != null)
            {
                limbControl.joint_lock = false;
                dragPoint.drag_start_event.Invoke();
                dragPoint.transform.position = targetPosition;
                ikFabrik.solver.maxIterations = 4;
                ikFabrik.solver.Update();
                dragPoint.drag_end_event.Invoke();
            }
        }

        public void PositonCorrection()
        {
            var pos = maidCache.GetInitialPosition(boneType);
            bone.transform.localPosition = pos;
        }

        public void AdjustFootGrounding()
        {
            var footBone = ikManager.GetBone(holdType == IKHoldType.Foot_L_Tip ? IKManager.BoneType.Foot_L : IKManager.BoneType.Foot_R);
            if (footBone == null)
            {
                return;
            }

            var footGroundAngle = maidCache.footGroundAngle;
            var footStretchAngle = maidCache.footStretchAngle;
            var footBaseOffset = maidCache.footBaseOffset;
            var footStretchHeight = maidCache.footStretchHeight;
            var floorHeight = maidCache.floorHeight;

            // 地面と並行となるZ角度を計算
            float targetAngle;
            {
                var worldRotation = footBone.transform.rotation;
                var forward = worldRotation * Vector3.forward;
                forward.y = 0; // Y成分を0にして水平にする
                forward.Normalize();

                var targetRotation = Quaternion.LookRotation(forward, Vector3.up);
                var localTargetRotation = Quaternion.Inverse(footBone.transform.parent.rotation) * targetRotation;

                var localEulerAngles = localTargetRotation.eulerAngles;
                targetAngle = localEulerAngles.z + footGroundAngle;
            }

            // 足が地面より上にある場合、足を伸ばす
            Vector3 footPos = footBone.transform.position;
            float heightDifference = footPos.y - floorHeight - footBaseOffset;
            if (heightDifference > 0f)
            {
                // 近い方の角度を採用
                int diffAngle = (int) (footStretchAngle - targetAngle);
                if (diffAngle > 180)
                {
                    footStretchAngle -= (diffAngle + 180) / 360 * 360;
                }
                else if (diffAngle < -180)
                {
                    footStretchAngle -= (diffAngle - 180) / 360 * 360;
                }

                var heightRate = Mathf.Clamp01(heightDifference / footStretchHeight);
                targetAngle = Mathf.Lerp(targetAngle, footStretchAngle, heightRate);
            }

            // 角度を足首に適用
            Vector3 footRotation = footBone.transform.localEulerAngles;
            footRotation.z = targetAngle;
            footBone.transform.localEulerAngles = footRotation;
        }

        public void ResetTargetPosition()
        {
            resetTargetPositionRequested = true;
        }

        private LimbControl GetLimbControl(LimbControl.Type type)
        {
            return maidCache.limbControlList.Find(l => l.type == type);
        }

        private LimbControl GetLimbControl(IKHoldType type)
        {
            switch (type)
            {
                case IKHoldType.Arm_L_Joint:
                case IKHoldType.Arm_L_Tip:
                    return GetLimbControl(LimbControl.Type.Arm_L);
                case IKHoldType.Arm_R_Joint:
                case IKHoldType.Arm_R_Tip:
                    return GetLimbControl(LimbControl.Type.Arm_R);
                case IKHoldType.Foot_L_Joint:
                case IKHoldType.Foot_L_Tip:
                    return GetLimbControl(LimbControl.Type.Foot_L);
                case IKHoldType.Foot_R_Joint:
                case IKHoldType.Foot_R_Tip:
                    return GetLimbControl(LimbControl.Type.Foot_R);
            }
            return null;
        }
    }
}