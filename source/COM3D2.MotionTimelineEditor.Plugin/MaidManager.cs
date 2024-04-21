using System.Collections.Generic;
using System.Reflection;
using RootMotion.FinalIK;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    using MTE = MotionTimelineEditor;

    public class MaidManager
    {
        private Maid _maid = null;
        private string _annName = "";
        private AnimationState _animationState = null;
        private CacheBoneDataArray _cacheBoneData = null;
        private string _errorMessage = "";

        public event UnityAction<Maid> onMaidChanged;
        public event UnityAction<string> onAnmChanged;

        private static MaidManager _instance = null;
        public static MaidManager instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new MaidManager();
                }
                return _instance;
            }
        }

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

        public Animation animation
        {
            get
            {
                return _maid != null ? _maid.GetAnimation() : null;
            }
        }

        public AnimationState animationState
        {
            get
            {
                return _animationState;
            }
        }

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

        private IKManager _ikManager = null;
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
                    PluginUtils.AssertNull(fieldLimbControlList != null, "fieldLimbControlList is null");
                }
                return (List<LimbControl>) fieldLimbControlList.GetValue(_ikManager);
            }
        }

        private static StudioHackBase studioHack
        {
            get
            {
                return MTE.studioHack;
            }
        }

        private MaidManager()
        {
            SceneManager.sceneLoaded += OnChangedSceneLevel;
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
            if (dragPoint != null && dragPoint.target_ik_point_trans != null)
            {
                return dragPoint.target_ik_point_trans.position;
            }
            return Vector3.zero;
        }

        public bool IsIkDragging(IKHoldType holdType)
        {
            var dragPoint = GetDragPoint(holdType);
            if (dragPoint != null && dragPoint.axis_obj != null)
            {
                return dragPoint.axis_obj.is_drag || dragPoint.axis_obj.is_grip;
            }
            return false;
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
                dragPoint.PositonCorrection();
            }
        }

        public bool IsValid()
        {
            _errorMessage = "";

            if (studioHack == null)
            {
                _errorMessage = "シーンが無効です";
                return false;
            }

            if (GameMain.Instance.CharacterMgr.IsBusy())
            {
                _errorMessage = "メイド処理中です";
                return false;
            }

            var maid = studioHack.activeMaid;
            if (maid == null)
            {
                _errorMessage = "メイドを配置してください";
                return false;
            }

            if (maid.body0 == null || maid.body0.m_Bones == null)
            {
                _errorMessage = "メイド生成中です";
                return false;
            }

            return true;
        }

        public void Reset()
        {
            _maid = null;
            _annName = "";
            _animationState = null;
            _cacheBoneData = null;
            _ikManager = null;
        }

        public void Update()
        {
            if (!IsValid())
            {
                Reset();
                return;
            }

            var activeMaid = studioHack.activeMaid;
            if (_maid != activeMaid)
            {
                OnMaidChanged(activeMaid);
            }

            if (_maid == null || animation == null)
            {
                return;
            }

            // アニメ名更新
            var anmName = _maid.body0.LastAnimeFN;
            if (_annName != anmName)
            {
                OnAnmChanged(anmName);
            }
        }

        public void OnMotionUpdated()
        {
            _annName = "";
            _animationState = null;
            Update();
        }

        private void OnMaidChanged(Maid maid)
        {
            PluginUtils.LogDebug("Maid changed: " + (maid != null ? maid.name : "null"));

            _maid = maid;
            _annName = "";
            _animationState = null;
            _cacheBoneData = null;
            _ikManager = null;

            if (maid == null)
            {
                return;
            }

            _cacheBoneData = maid.gameObject.GetComponent<CacheBoneDataArray>();
            if (_cacheBoneData == null)
            {
                _cacheBoneData = maid.gameObject.AddComponent<CacheBoneDataArray>();
                _cacheBoneData.CreateCache(maid.body0.GetBone("Bip01"));
            }
            _ikManager = PoseEditWindow.GetMaidIKManager(maid);

            if (onMaidChanged != null)
            {
                onMaidChanged(maid);
            }
        }

        private void OnAnmChanged(string anmName)
        {
            PluginUtils.LogDebug("Animation changed: " + anmName);

            _annName = anmName;
            _animationState = null;

            if (string.IsNullOrEmpty(_annName))
            {
                return;
            }

            _animationState = animation[_annName.ToLower()];

            if (onAnmChanged != null)
            {
                onAnmChanged(anmName);
            }
        }

        private void OnChangedSceneLevel(Scene sceneName, LoadSceneMode SceneMode)
        {
            Reset();
        }
    }
}