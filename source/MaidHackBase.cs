using System.Collections.Generic;
using System.Reflection;
using CM3D2.MultipleMaids.Plugin;
using RootMotion.FinalIK;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public abstract class MaidHackBase
    {
        protected Maid _maid = null;
        protected string _annName = "";
        protected Animation _animation = null;
        protected AnimationState _animationState = null;
        protected CacheBoneDataArray _cacheBoneData = null;
        protected string _errorMessage = "";

        public Maid maid
        {
            get
            {
                return _maid;
            }
        }

        public string errorMessage
        {
            get
            {
                return _errorMessage;
            }
        }

        public abstract string outputAnmPath { get; }

        public abstract bool isIkBoxVisibleRoot { get; set; }

        public abstract bool isIkBoxVisibleBody { get; set; }

        public abstract bool isPoseEditing { get; set; }

        public abstract bool isMotionPlaying { get; set; }

        public string annName
        {
            get
            {
                return _annName;
            }
        }

        public float anmSpeed
        {
            get
            {
                return _animationState != null ? _animationState.speed : 0;
            }
            set
            {
                if (_animationState != null)
                {
                    _animationState.speed = value;
                }
            }
        }

        public CacheBoneDataArray cacheBoneData
        {
            get
            {
                return _cacheBoneData;
            }
        }

        public abstract float motionSliderRate { get; set; }

        public abstract bool useMuneKeyL { get; set; }

        public abstract bool useMuneKeyR { get; set; }

        protected IKManager _ikManager = null;
        public IKManager ikManager
        {
            get
            {
                return _ikManager;
            }
        }

        private FieldInfo fieldLimbControlList = null;

        public List<LimbControl> limbControlList
        {
            get
            {
                if (_ikManager == null)
                {
                    return new List<LimbControl>();
                }
                if (fieldLimbControlList == null)
                {
                    fieldLimbControlList = typeof(IKManager).GetField("limb_control_list_", BindingFlags.NonPublic | BindingFlags.Instance);
                    PluginUtils.AssertNull(fieldLimbControlList != null);
                }
                return (List<LimbControl>) fieldLimbControlList.GetValue(_ikManager);
            }
        }

        public LimbControl GetLimbControl(LimbControl.Type type)
        {
            return limbControlList.Find(l => l.type == type);
        }

        public FABRIK GetIkFabrik(IKHoldType type)
        {
            switch (type)
            {
                case IKHoldType.Arm_R_Joint:
                case IKHoldType.Arm_R_Tip:
                    return GetLimbControl(LimbControl.Type.Arm_R).GetIkFabrik();
                case IKHoldType.Arm_L_Joint:
                case IKHoldType.Arm_L_Tip:
                    return GetLimbControl(LimbControl.Type.Arm_L).GetIkFabrik();
                case IKHoldType.Foot_R_Joint:
                case IKHoldType.Foot_R_Tip:
                    return GetLimbControl(LimbControl.Type.Foot_R).GetIkFabrik();
                case IKHoldType.Foot_L_Joint:
                case IKHoldType.Foot_L_Tip:
                    return GetLimbControl(LimbControl.Type.Foot_L).GetIkFabrik();
            }
            return null;
        }

        public IKDragPoint GetDragPoint(IKHoldType type)
        {
            switch (type)
            {
                case IKHoldType.Arm_R_Joint:
                    return GetLimbControl(LimbControl.Type.Arm_R).GetJointDragPoint();
                case IKHoldType.Arm_R_Tip:
                    return GetLimbControl(LimbControl.Type.Arm_R).GetTipDragPoint();
                case IKHoldType.Arm_L_Joint:
                    return GetLimbControl(LimbControl.Type.Arm_L).GetJointDragPoint();
                case IKHoldType.Arm_L_Tip:
                    return GetLimbControl(LimbControl.Type.Arm_L).GetTipDragPoint();
                case IKHoldType.Foot_R_Joint:
                    return GetLimbControl(LimbControl.Type.Foot_R).GetJointDragPoint();
                case IKHoldType.Foot_R_Tip:
                    return GetLimbControl(LimbControl.Type.Foot_R).GetTipDragPoint();
                case IKHoldType.Foot_L_Joint:
                    return GetLimbControl(LimbControl.Type.Foot_L).GetJointDragPoint();
                case IKHoldType.Foot_L_Tip:
                    return GetLimbControl(LimbControl.Type.Foot_L).GetTipDragPoint();
            }
            return null;
        }

        public Vector3 GetIkPosition(IKHoldType holdType)
        {
            var dragPoint = GetDragPoint(holdType);
            if (dragPoint != null)
            {
                return dragPoint.target_ik_point_trans.position;
            }
            return Vector3.zero;
        }

        public void UpdateIkPosition(IKHoldType holdType, Vector3 targetPosition)
        {
            var ikFabrik = GetIkFabrik(holdType);
            var dragPoint = GetDragPoint(holdType);
            if (ikFabrik != null && dragPoint != null)
            {
                dragPoint.drag_start_event.Invoke();
                dragPoint.transform.position = targetPosition;
                ikFabrik.solver.Update();
                dragPoint.drag_end_event.Invoke();
            }
        }

        public abstract void Init();

        public abstract bool IsValid();

        protected abstract Maid GetMaid();

        protected virtual void OnMaidChanged(Maid maid)
        {
            _maid = maid;
            PluginUtils.LogDebug("Maid changed: " + (_maid != null ? _maid.name : "null"));

            if (_maid != null)
            {
                _animation = _maid.body0.m_Bones.GetComponent<Animation>();
                _annName = _maid.body0.LastAnimeFN;
                _animationState = _animation[annName.ToLower()];
                _cacheBoneData = maid.gameObject.GetComponent<CacheBoneDataArray>();
                if (_cacheBoneData == null)
                {
                    _cacheBoneData = maid.gameObject.AddComponent<CacheBoneDataArray>();
                    _cacheBoneData.CreateCache(maid.body0.GetBone("Bip01"));
                }
                _ikManager = PoseEditWindow.GetMaidIKManager(maid);
            }
            else
            {
                _animation = null;
                _annName = "";  
                _animationState = null;
                _cacheBoneData = null;
                _ikManager = null;
            }
        }

        protected virtual void OnAnmChanged(string anmName)
        {
            PluginUtils.LogDebug("Animation changed: " + anmName);
            _annName = anmName;
            _animationState = _animation[_annName.ToLower()];
        }

        public virtual void Update()
        {
            var currentMaid = GetMaid();
            if (_maid != currentMaid)
            {
                OnMaidChanged(currentMaid);
            }

            if (_maid == null || _animation == null)
            {
                return;
            }

            // アニメ名更新
            var currentAnmName = _maid.body0.LastAnimeFN;
            if (_annName != currentAnmName)
            {
                OnAnmChanged(currentAnmName);
            }
        }

        public virtual bool HasBoneRotateVisible(IKManager.BoneType boneType)
        {
            return false;
        }

        public virtual bool IsBoneRotateVisible(IKManager.BoneType boneType)
        {
            return false;
        }

        public virtual void SetBoneRotateVisible(IKManager.BoneType boneType, bool visible)
        {
            // do nothing
        }

        public virtual void ClearBoneRotateVisible()
        {
            // do nothing
        }

        public virtual void OnMotionUpdated(Maid maid)
        {
            _annName = "";
            Update();
        }

        public virtual void OnUpdateMyPose(string anmPath, bool isExist)
        {
            _annName = "";
            Update();
        }
    }
}