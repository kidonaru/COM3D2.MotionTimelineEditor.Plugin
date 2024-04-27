using System.Collections.Generic;
using System.Reflection;
using RootMotion.FinalIK;
using UnityEngine;
using UnityEngine.Events;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    using MTE = MotionTimelineEditor;

    public class MaidCache
    {
        public int slotNo = 0;
        public Maid maid = null;
        public string annName = "";
        public long anmId = 0;
        public AnimationState animationState = null;
        public int anmStartFrameNo = 0;
        public int anmEndFrameNo = 0;
        public CacheBoneDataArray cacheBoneData;
        public IKManager ikManager = null;

        public static event UnityAction<int, Maid> onMaidChanged;
        public static event UnityAction<int, string> onAnmChanged;

        public Animation animation
        {
            get
            {
                return maid != null ? maid.GetAnimation() : null;
            }
        }

        public float anmSpeed
        {
            get
            {
                return animationState != null ? animationState.speed : 0;
            }
            set
            {
                if (animationState != null)
                {
                    animationState.speed = value;
                }
            }
        }

        public bool isMotionPlaying
        {
            get
            {
                if (animationState != null)
                {
                    return animationState.enabled;
                }
                return false;
            }
            set
            {
                if (animationState != null)
                {
                    animationState.enabled = value;
                }
            }
        }

        public float _motionSliderRate = 0f;
        public float motionSliderRate
        {
            get
            {
                return _motionSliderRate;
            }
            set
            {
                PluginUtils.LogDebug("Update motionSliderRate slot={0} rate={1}", slotNo, value);
                _motionSliderRate = value;

                if (animationState != null)
                {
                    var isPlaying = this.isMotionPlaying;
                    var maxNum = animationState.length;
                    var current = Mathf.Clamp01(value) * maxNum;
                    animationState.time = current;
                    animationState.enabled = true;
                    animation.Sample();
                    if (!isPlaying)
                    {
                        animationState.enabled = false;
                    }
                }
            }
        }

        public int playingFrameNo
        {
            get
            {
                return (int) Mathf.Round(playingFrameNoFloat);
            }
            set
            {
                playingFrameNoFloat = value;
            }
        }

        public float playingFrameNoFloat
        {
            get
            {
                if (!isAnmSyncing)
                {
                    return timelineManager.currentFrameNo;
                }

                var rate = motionSliderRate;
                var anmLength = anmEndFrameNo - anmStartFrameNo;
                return anmStartFrameNo + rate * anmLength;
            }
            set
            {
                var anmLength = anmEndFrameNo - anmStartFrameNo;
                if (anmLength > 0)
                {
                    var rate = Mathf.Clamp01((float) (value - anmStartFrameNo) / anmLength);
                    motionSliderRate = rate;
                }
            }
        }

        // アニメーションと同期しているか
        public bool isAnmSyncing
        {
            get
            {
                return timelineManager.IsValidData() && anmId == TimelineLayerBase.TimelineAnmId;
            }
        }

        // タイムラインアニメーションを再生中か
        public bool isAnmPlaying
        {
            get
            {
                return studioHack.isMotionPlaying && isAnmSyncing;
            }
        }

        private static FieldInfo fieldLimbControlList = null;

        public List<LimbControl> limbControlList
        {
            get
            {
                if (ikManager == null)
                {
                    return new List<LimbControl>();
                }
                if (fieldLimbControlList == null)
                {
                    fieldLimbControlList = typeof(IKManager).GetField("limb_control_list_", BindingFlags.NonPublic | BindingFlags.Instance);
                    PluginUtils.AssertNull(fieldLimbControlList != null, "fieldLimbControlList is null");
                }
                return (List<LimbControl>) fieldLimbControlList.GetValue(ikManager);
            }
        }

        private static TimelineManager timelineManager
        {
            get
            {
                return TimelineManager.instance;
            }
        }

        private static StudioHackBase studioHack
        {
            get
            {
                return MTE.studioHack;
            }
        }

        public MaidCache(int slotNo)
        {
            this.slotNo = slotNo;
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

        public void PositonCorrection(IKHoldType holdType)
        {
            var dragPoint = GetDragPoint(holdType);
            if (dragPoint != null)
            {
                dragPoint.PositonCorrection();
            }
        }

        public void Reset()
        {
            maid = null;
            annName = "";
            anmId = 0;
            animationState = null;
            cacheBoneData = null;
            ikManager = null;
        }

        public void ResetAnm()
        {
            annName = "";
            anmId = 0;
            animationState = null;
        }

        public void Update(Maid maid)
        {
            if (maid != this.maid)
            {
                OnMaidChanged(maid);
            }

            if (maid == null || animation == null)
            {
                return;
            }

            // アニメ名更新
            var anmName = maid.body0.LastAnimeFN;
            if (this.annName != anmName)
            {
                OnAnmChanged(anmName);
            }

            var animationState = this.animationState;
            if (animationState != null && animationState.enabled && animationState.length > 0f)
            {
                float value = animationState.time;
                if (animationState.length < animationState.time)
                {
                    if (animationState.wrapMode == WrapMode.ClampForever)
                    {
                        value = animationState.length;
                    }
                    else
                    {
                        value = animationState.time - animationState.length * (float)((int)(animationState.time / animationState.length));
                    }
                }
                _motionSliderRate = value / animationState.length;
            }
        }

        private void OnMaidChanged(Maid maid)
        {
            PluginUtils.LogDebug("Maid changed: " + (maid != null ? maid.name : "null"));

            Reset();

            this.maid = maid;
            if (maid == null)
            {
                return;
            }

            cacheBoneData = maid.gameObject.GetComponent<CacheBoneDataArray>();
            if (cacheBoneData == null)
            {
                cacheBoneData = maid.gameObject.AddComponent<CacheBoneDataArray>();
                cacheBoneData.CreateCache(maid.body0.GetBone("Bip01"));
            }
            ikManager = PoseEditWindow.GetMaidIKManager(maid);

            if (onMaidChanged != null)
            {
                onMaidChanged(slotNo, maid);
            }
        }

        private void OnAnmChanged(string anmName)
        {
            PluginUtils.LogDebug("Animation changed: " + anmName);

            ResetAnm();

            this.annName = anmName;
            if (string.IsNullOrEmpty(annName))
            {
                return;
            }

            animationState = animation[annName.ToLower()];
            long.TryParse(annName, out anmId);

            if (onAnmChanged != null)
            {
                onAnmChanged(slotNo, anmName);
            }
        }
    }
}