using System;
using System.Collections.Generic;
using System.Reflection;
using RootMotion.FinalIK;
using UnityEngine;
using UnityEngine.Events;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    using AttachPoint = PhotoTransTargetObject.AttachPoint;

    public enum MaidPointType
    {
        Head,
        Chest,
        Crotch,
        Hip,
        Bip01,
    }

    public enum LookAtTargetType
    {
        None,
        Camera,
        Maid,
        Model,
    }

    public partial class MaidCache
    {
        public int slotNo = 0;
        public Maid maid = null;
        public MaidInfo info = null;
        public string annName = "";
        public long anmId = 0;
        public AnimationState animationState = null;
        public int anmStartFrameNo = 0;
        public int anmEndFrameNo = 0;
        public CacheBoneDataArray cacheBoneData;
        public IKManager ikManager = null;
        public ExtendBoneCache extendBoneCache = null;

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

        public bool useHeadKey
        {
            get
            {
                return maid.body0.trsLookTarget == null;
            }
            set
            {
                maid.body0.trsLookTarget = value ? null : GameMain.Instance.MainCamera.transform;
            }
        }
        
        public Transform trsEyeL
        {
            get
            {
                return maid != null ? maid.body0.trsEyeL : null;
            }
        }

        public Transform trsEyeR
        {
            get
            {
                return maid != null ? maid.body0.trsEyeR : null;
            }
        }

        public Vector3 eyesPosL
        {
            get
            {
                if (maid != null)
                {
                    var initPos = info.initEyesPosL;
                    return trsEyeL.localPosition - initPos;
                }
                return Vector3.zero;
            }
            set
            {
                if (maid != null)
                {
                    var initPos = info.initEyesPosL;
                    trsEyeL.localPosition = value + initPos;
                }
            }
        }

        public Vector3 eyesPosR
        {
            get
            {
                if (maid != null)
                {
                    var initPos = info.initEyesPosR;
                    return trsEyeR.localPosition - initPos;
                }
                return Vector3.zero;
            }
            set
            {
                if (maid != null)
                {
                    var initPos = info.initEyesPosR;
                    trsEyeR.localPosition = value + initPos;
                }
            }
        }

        public Vector3 eyesScaL
        {
            get
            {
                if (maid != null)
                {
                    var initSca = info.initEyesScaL;
                    return trsEyeL.localScale - initSca;
                }
                return Vector3.zero;
            }
            set
            {
                if (maid != null)
                {
                    var initSca = info.initEyesScaL;
                    trsEyeL.localScale = value + initSca;
                }
            }
        }

        public Vector3 eyesScaR
        {
            get
            {
                if (maid != null)
                {
                    var initSca = info.initEyesScaR;
                    return trsEyeR.localScale - initSca;
                }
                return Vector3.zero;
            }
            set
            {
                if (maid != null)
                {
                    var initSca = info.initEyesScaR;
                    trsEyeR.localScale = value + initSca;
                }
            }
        }

        private LookAtTargetType _lookAtTargetType;
        public LookAtTargetType lookAtTargetType
        {
            get
            {
                return _lookAtTargetType;
            }
            set
            {
                _lookAtTargetType = value;
                UpdateLookAtTarget();
            }
        }

        private int _lookAtTargetIndex;
        public int lookAtTargetIndex
        {
            get
            {
                return _lookAtTargetIndex;
            }
            set
            {
                _lookAtTargetIndex = value;
                UpdateLookAtTarget();
            }
        }

        private MaidPointType _lookAtMaidPointType;
        public MaidPointType lookAtMaidPointType
        {
            get
            {
                return _lookAtMaidPointType;
            }
            set
            {
                _lookAtMaidPointType = value;
                UpdateLookAtTarget();
            }
        }

        public Vector3 eyeEulerAngle;

        public string oneTimeVoiceName = string.Empty;
        public string loopVoiceName = string.Empty;

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

        public string fullName
        {
            get
            {
                return maid != null ? maid.status.fullNameJpStyle : "";
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

        private static StudioHackBase studioHack
        {
            get
            {
                return StudioHackManager.studioHack;
            }
        }

        private static PartsEditHackManager partsEditHackManager
        {
            get
            {
                return PartsEditHackManager.instance;
            }
        }

        private static MaidManager maidManager
        {
            get
            {
                return MaidManager.instance;
            }
        }

        private static StudioModelManager modelManager
        {
            get
            {
                return StudioModelManager.instance;
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
            }
        }

        public void PositonCorrection(IKHoldType holdType)
        {
            var boneType = holdType.ConvertBoneType();
            var bone = ikManager.GetBone(boneType);
            var pos = GetInitialEditPosition(boneType);
            bone.transform.localPosition = pos;
        }

        private Dictionary<IKManager.BoneType, Vector3> _initialEditPositions = new Dictionary<IKManager.BoneType, Vector3>();

        public void SetInitialEditPosition(IKManager.BoneType boneType, Vector3 pos)
        {
            _initialEditPositions[boneType] = pos;
        }

        public Vector3 GetInitialEditPosition(IKManager.BoneType boneType)
        {
            Vector3 pos;
            if (_initialEditPositions.TryGetValue(boneType, out pos))
            {
                return pos;
            }
            return Vector3.zero;
        }

        public void SaveInitialEditPosition()
        {
            var boneTypes = new List<IKManager.BoneType>
            {
                IKManager.BoneType.UpperArm_L,
                IKManager.BoneType.Forearm_L,
                IKManager.BoneType.Thigh_L,
                IKManager.BoneType.Calf_L,

                IKManager.BoneType.UpperArm_R,
                IKManager.BoneType.Forearm_R,
                IKManager.BoneType.Thigh_R,
                IKManager.BoneType.Calf_R,
            };

            foreach (var boneType in boneTypes)
            {
                var bone = ikManager.GetBone(boneType);
                if (bone != null)
                {
                    SetInitialEditPosition(boneType, bone.transform.localPosition);
                }
            }
        }

        public void Reset()
        {
            maid = null;
            info = null;
            annName = "";
            anmId = 0;
            animationState = null;
            cacheBoneData = null;
            ikManager = null;
            extendBoneCache = null;
            _blendShapeCache.Clear();
            lookAtTargetType = LookAtTargetType.None;
            lookAtTargetIndex = 0;
            lookAtMaidPointType = MaidPointType.Head;
            eyeEulerAngle = Vector3.zero;
            oneTimeVoiceName = string.Empty;
            loopVoiceName = string.Empty;
        }

        public void ResetAnm()
        {
            annName = "";
            anmId = 0;
            animationState = null;
        }

        public void ResetEyes()
        {
            eyesPosL = Vector3.zero;
            eyesPosR = Vector3.zero;
            eyesScaL = Vector3.zero;
            eyesScaR = Vector3.zero;
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

            UpdateEyeEulerAngle();
            UpdateVoice();
        }

        public void PlayAnm(long id, byte[] anmData)
        {
            if (anmData == null)
            {
                return;
            }
            if (maid == null)
            {
                return;
            }

            GameMain.Instance.ScriptMgr.StopMotionScript();

            UpdateMuneYure();
            UpdateHeadLook();

            maid.body0.CrossFade(id.ToString(), anmData, false, false, false, 0f, 1f);
            maid.SetAutoTwistAll(true);

            var animation = maid.GetAnimation();
            if (animation != null)
            {
                animation.wrapMode = timeline.isLoopAnm ? WrapMode.Loop : WrapMode.ClampForever;
            }
        }

        public void UpdateMuneYure()
        {
            if (maid != null && !maid.boMAN)
            {
                maid.body0.MuneYureL((float)((!timeline.useMuneKeyL) ? 1 : 0));
                maid.body0.MuneYureR((float)((!timeline.useMuneKeyR) ? 1 : 0));
                maid.body0.jbMuneL.enabled = !timeline.useMuneKeyL;
                maid.body0.jbMuneR.enabled = !timeline.useMuneKeyR;
            }
        }

        public void UpdateHeadLook()
        {
            if (maid == null || timeline == null)
            {
                return;
            }

            maid.EyeToCamera(timeline.eyeMoveType, 0f);
            maid.LockHeadAndEye(false);

            if (timeline.useHeadKey)
            {
                var trsEyeL = maid.body0.trsEyeL;
                var trsEyeR = maid.body0.trsEyeR;
                trsEyeL.localRotation = maid.body0.quaDefEyeL;
                trsEyeR.localRotation = maid.body0.quaDefEyeR;
            }

            UpdateLookAtTarget();
        }

        public void UpdateLookAtTarget()
        {
            if (maid == null || timeline == null || !timeline.useHeadKey)
            {
                return;
            }

            var lookAtTarget = GetLookAtTarget();
            maid.LockHeadAndEye(lookAtTarget == null);
            maid.body0.trsLookTarget = lookAtTarget;
        }

        public void UpdateEyeEulerAngle()
        {
            if (maid == null || timeline == null || !timeline.useHeadKey)
            {
                return;
            }

            var lookAtTarget = GetLookAtTarget();
            if (lookAtTarget == null)
            {
                maid.body0.SetEyeEulerAngle(eyeEulerAngle);

                this.trsEyeL.localRotation = maid.body0.quaDefEyeL * Quaternion.Euler(0f, -eyeEulerAngle.x * 0.2f + maid.body0.m_editYorime, -eyeEulerAngle.z * 0.1f);
                this.trsEyeR.localRotation = maid.body0.quaDefEyeR * Quaternion.Euler(0f, eyeEulerAngle.x * 0.2f + maid.body0.m_editYorime, eyeEulerAngle.z * 0.1f);
            }
            else
            {
                eyeEulerAngle = maid.body0.GetEyeEulerAngle();
            }
        }

        public Transform GetAttachPointTransform(AttachPoint point)
        {
            if (ikManager == null || point == AttachPoint.Null)
            {
                return null;
            }

            var boneType = BoneUtils.GetBoneType(point);
            return ikManager.GetBone(boneType).transform;
        }

        private Dictionary<string, MaidBlendShape> _blendShapeCache =
                new Dictionary<string, MaidBlendShape>();

        public MaidBlendShape GetBlendShape(string shapeKey)
        {
            if (!_blendShapeCache.ContainsKey(shapeKey))
            {
                var blendShape = GetBlendShapeInternal(shapeKey);
                _blendShapeCache[shapeKey] = blendShape;
            }
            return _blendShapeCache[shapeKey];
        }

        private MaidBlendShape GetBlendShapeInternal(string shapeKey)
        {
            if (maid == null || maid.body0 == null)
            {
                return null;
            }

            var blendShape = new MaidBlendShape();
            
            foreach (var slot in maid.body0.goSlot)
            {
                if (slot.morph == null || slot.morph.hash.Count == 0)
                {
                    continue;
                }

                if (slot.morph.Contains(shapeKey))
                {
                    blendShape.entities.Add(new MaidBlendShape.Entity
                    {
                        morph = slot.morph,
                        shapeKeyIndex = (int) slot.morph.hash[shapeKey],
                    });
                }
            }

            return blendShape;
        }

        public void SetBlendShapeValue(string shapeKey, float value)
        {
            var blendShape = GetBlendShape(shapeKey);
            if (blendShape != null)
            {
                blendShape.weight = value;
            }
        }

        public float GetBlendShapeValue(string shapeKey)
        {
            var blendShape = GetBlendShape(shapeKey);
            if (blendShape != null)
            {
                return blendShape.weight;
            }
            return 0f;
        }

        public void FixBlendValues(IEnumerable<string> shapeKeys)
        {
            if (maid == null || maid.body0 == null)
            {
                return;
            }

            var morphs = new HashSet<TMorph>();

            foreach (var shapeKey in shapeKeys)
            {
                var blendShape = GetBlendShape(shapeKey);
                foreach (var entity in blendShape.entities)
                {
                    morphs.Add(entity.morph);
                }
            }

            foreach (var morph in morphs)
            {
                if (morph != null)
                {
                    morph.FixBlendValues();
                }
            }
        }

        public string GetBonePath(string boneName)
        {
            var bonePath = BoneUtils.ConvertToBonePath(boneName);
            if (!string.IsNullOrEmpty(bonePath))
            {
                return bonePath;
            }

            if (extendBoneCache != null)
            {
                var entity = extendBoneCache.GetEntity(boneName);
                if (entity != null)
                {
                    return entity.bonePath;
                }
            }
            return "";
        }

        public Transform GetBoneTransform(string boneName)
        {
            if (BoneUtils.IsDefaultBoneName(boneName))
            {
                var bonePath = BoneUtils.ConvertToBonePath(boneName);
                var boneData = cacheBoneData.GetBoneData(bonePath);
                if (boneData != null)
                {
                    return boneData.transform;
                }
                return null;
            }

            if (extendBoneCache != null)
            {
                var entity = extendBoneCache.GetEntity(boneName);
                if (entity != null)
                {
                    return entity.transform;
                }
            }

            return null;
        }

        public Vector3 GetInitialPosition(string boneName)
        {
            if (BoneUtils.IsDefaultBoneName(boneName))
            {
                var boneType = BoneUtils.GetBoneTypeByName(boneName);
                return BoneUtils.GetInitialPosition(boneType);
            }

            if (extendBoneCache != null)
            {
                var entity = extendBoneCache.GetEntity(boneName);
                if (entity != null)
                {
                    return entity.initialPosition;
                }
            }

            return Vector3.zero;
        }

        public Vector3 GetInitialEulerAngles(string boneName)
        {
            if (BoneUtils.IsDefaultBoneName(boneName))
            {
                var boneType = BoneUtils.GetBoneTypeByName(boneName);
                return BoneUtils.GetInitialEulerAngles(boneType);
            }

            if (extendBoneCache != null)
            {
                var entity = extendBoneCache.GetEntity(boneName);
                if (entity != null)
                {
                    return entity.initialRotation.eulerAngles;
                }
            }

            return Vector3.zero;
        }

        public bool IsYureSlot(string slotName)
        {
            return extendBoneCache != null && extendBoneCache.IsYureSlot(slotName);
        }

        public bool GetYureState(string slotName)
        {
            return partsEditHackManager.GetYureState(maid, slotName);
        }

        public void SetYureState(string slotName, bool state)
        {
            partsEditHackManager.SetYureState(maid, slotName, state);
        }

        public Transform GetPointTransform(MaidPointType type)
        {
            if (maid == null || maid.body0 == null)
            {
                return null;
            }

            Transform result = null;
            switch (type)
            {
                case MaidPointType.Head:
                    result = maid.body0.trsHead;
                    break;
                case MaidPointType.Chest:
                    result = maid.body0.Spine1a;
                    break;
                case MaidPointType.Crotch:
                    result = maid.body0.Pelvis;
                    break;
                case MaidPointType.Hip:
                    result = maid.body0.Hip_R;
                    break;
                case MaidPointType.Bip01:
                    result = maid.body0.trBip;
                    break;
            }

            return result;
        }

        public static readonly List<string> MaidPointTypeNames = new List<string>
        {
            "顔", "胸", "股", "尻", "中心",
        };

        public static string GetMaidPointTypeName(MaidPointType type)
        {
            return MaidPointTypeNames[(int) type];
        }

        public Transform GetLookAtTarget()
        {
            var maid = this.maid;
            if (maid == null)
            {
                return null;
            }

            switch (lookAtTargetType)
            {
                case LookAtTargetType.Camera:
                    return GameMain.Instance.MainCamera.transform;
                case LookAtTargetType.Maid:
                {
                    var maidCache = maidManager.GetMaidCache(lookAtTargetIndex);
                    if (maidCache != null)
                    {
                        return maidCache.GetPointTransform(lookAtMaidPointType);
                    }
                    break;
                }
                case LookAtTargetType.Model:
                {
                    var model = modelManager.GetModel(lookAtTargetIndex);
                    if (model != null)
                    {
                        return model.transform;
                    }
                    break;
                }
            }
            return null;
        }

        public void PlayOneTimeVoice()
        {
            PlayVoice(oneTimeVoiceName, 0.1f, false);
            UpdateVoice();
        }

        public void PlayVoice(string fileName, float fadeTime, bool isLoop)
        {
            if (maid == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(fileName))
            {
                maid.AudioMan.Stop();
                return;
            }

            var fixedFileName = EndsWith(fileName, ".ogg");
            maid.AudioMan.LoadPlay(fixedFileName, fadeTime, false, isLoop);
        }

        public void UpdateVoice()
        {
            if (maid == null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(loopVoiceName))
            {
                if (!maid.AudioMan.isPlay())
                {
                    PlayVoice(loopVoiceName, 0.1f, true);
                }
            }
        }

        public static string EndsWith(string value, string extension)
        {
            if (!string.IsNullOrEmpty(value) && !value.EndsWith(extension, StringComparison.CurrentCultureIgnoreCase))
            {
                value += extension;
            }
            return value;
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

            info = MaidInfo.GetOrCreate(maid, ikManager);

            extendBoneCache = maid.gameObject.GetComponent<ExtendBoneCache>();
            if (extendBoneCache == null)
            {
                var anmRoot = cacheBoneData.GetBoneData("Bip01").transform.parent;
                extendBoneCache = maid.gameObject.AddComponent<ExtendBoneCache>();
                extendBoneCache.Init(maid, anmRoot);
            }

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