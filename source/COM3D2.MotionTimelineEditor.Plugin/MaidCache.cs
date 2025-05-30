using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        public MaidPropCache maidPropCache = null;
        public Dictionary<IKHoldType, IKHoldEntity> ikHoldEntities = new Dictionary<IKHoldType, IKHoldEntity>(6);
        public List<MaidSlotStat> slotStats = new List<MaidSlotStat>(32);
        public Dictionary<TBody.SlotID, MaidSlotStat> slotStatMap = new Dictionary<TBody.SlotID, MaidSlotStat>(32);
        public Dictionary<string, ModelMaterial> materialMap = new Dictionary<string, ModelMaterial>(32);
        public List<string> materialNames = new List<string>(32);
        public List<AnimationLayerInfo> animationLayerInfos = new List<AnimationLayerInfo>(10);

        public static readonly int MinLayerIndex = 2;
        public static readonly int MaxLayerIndex = 8;

        public bool isGroundingFootL = false; // 左足の接地を有効
        public bool isGroundingFootR = false; // 右足の接地を有効
        public float floorHeight = 0f; // 接地面の高さ
        public float footBaseOffset = 0.05f; // 足の位置のオフセット
        public float footStretchHeight = 0.1f; // 足を伸ばす高さ
        public float footStretchAngle = 45f; // 足を伸ばしたときの角度
        public float footGroundAngle = 90f; // 接地時の足の角度

        public static event UnityAction<int, Maid> onMaidChanged;
        public static event UnityAction<int, string> onAnmChanged;

        public static readonly Dictionary<string, IKHoldType> ikHoldTypeMap = Enum.GetValues(typeof(IKHoldType))
                .Cast<IKHoldType>()
                .Where(t => t != IKHoldType.Max)
                .ToDictionary(t => t.ToString(), t => t);

        public Animation animation
        {
            get => maid != null ? maid.GetAnimation() : null;
        }

        public float anmSpeed
        {
            get => animationState != null ? animationState.speed : 0;
            set
            {
                if (animationState != null)
                {
                    animationState.speed = value;
                }
            }
        }

        public float _motionSliderRate = 0f;
        public float motionSliderRate
        {
            get => _motionSliderRate;
            set
            {
                //MTEUtils.LogDebug("Update motionSliderRate slot={0} rate={1}", slotNo, value);
                _motionSliderRate = value;

                if (animationState != null)
                {
                    var isAnmEnabled = animationState.enabled;
                    var maxNum = animationState.length;
                    var current = Mathf.Clamp01(value) * maxNum;
                    animationState.time = current;

                    if (!isAnmEnabled)
                    {
                        animationState.enabled = true;
                        animation.Sample();
                        animationState.enabled = false;
                    }
                }
            }
        }

        public int playingFrameNo
        {
            get => (int) Mathf.Round(playingFrameNoFloat);
            set => playingFrameNoFloat = value;
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
            get => timelineManager.IsValidData() && anmId == TimelineLayerBase.TimelineAnmId;
        }

        // アニメーションが有効か
        public bool isAnmEnabled
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

        // タイムラインアニメーションを再生中か
        public bool isAnmPlaying
        {
            get => isAnmEnabled && isAnmSyncing && anmSpeed > 0f;
            set
            {
                if (value)
                {
                    anmSpeed = timelineManager.anmSpeed;
                    isAnmEnabled = true;
                }
                else
                {
                    anmSpeed = 0f;
                }
            }
        }

        public bool useHeadKey
        {
            get => maid.body0.trsLookTarget == null;
            set => maid.body0.trsLookTarget = value ? null : PluginUtils.MainCamera.transform;
        }
        
        public Transform trsEyeL
        {
            get => maid != null ? maid.body0.trsEyeL : null;
        }

        public Transform trsEyeR
        {
            get => maid != null ? maid.body0.trsEyeR : null;
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
            get => _lookAtTargetType;
            set
            {
                _lookAtTargetType = value;
                UpdateLookAtTarget();
            }
        }

        private int _lookAtTargetIndex;
        public int lookAtTargetIndex
        {
            get => _lookAtTargetIndex;
            set
            {
                _lookAtTargetIndex = value;
                UpdateLookAtTarget();
            }
        }

        private MaidPointType _lookAtMaidPointType;
        public MaidPointType lookAtMaidPointType
        {
            get => _lookAtMaidPointType;
            set
            {
                _lookAtMaidPointType = value;
                UpdateLookAtTarget();
            }
        }

        public Vector3 eyeEulerAngle;

        public string oneShotVoiceName = string.Empty;
        public float oneShotVoiceStartTime = 0f;
        public float oneShotVoiceLength = 0f;
        public float voiceFadeTime = 0.1f;
        public float voicePitch = 1f;
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
                    MTEUtils.AssertNull(fieldLimbControlList != null, "fieldLimbControlList is null");
                }
                return (List<LimbControl>) fieldLimbControlList.GetValue(ikManager);
            }
        }

        public string fullName
        {
            get => maid != null ? maid.status.fullNameJpStyle : "";
        }

        private static TimelineManager timelineManager => TimelineManager.instance;
        private static TimelineData timeline => timelineManager.timeline;
        private static StudioHackBase studioHack => StudioHackManager.instance.studioHack;
        private static PartsEditHackManager partsEditHackManager => PartsEditHackManager.instance;
        private static MaidManager maidManager => MaidManager.instance;
        private static StudioModelManager modelManager => StudioModelManager.instance;

        public MaidCache(int slotNo)
        {
            this.slotNo = slotNo;

            for (int i = 0; i <= MaxLayerIndex; i++)
            {
                animationLayerInfos.Add(new AnimationLayerInfo(i));
            }
        }

        public void ResetIkHoldEntities()
        {
            ikHoldEntities.Clear();
        }

        public void ResetGrounding()
        {
            isGroundingFootL = false;
            isGroundingFootR = false;
        }

        public IKHoldEntity GetIKHoldEntity(IKHoldType holdType)
        {
            if (holdType == IKHoldType.Max)
            {
                return null;
            }

            IKHoldEntity entity;
            if (!ikHoldEntities.TryGetValue(holdType, out entity))
            {
                entity = new IKHoldEntity(holdType, this);
                ikHoldEntities[holdType] = entity;
            }
            return entity;
        }

        public static IKHoldType GetIKHoldType(string holdName)
        {
            IKHoldType holdType;
            if (ikHoldTypeMap.TryGetValue(holdName, out holdType))
            {
                return holdType;
            }
            return IKHoldType.Max;
        }

        public IKHoldEntity GetIKHoldEntity(string holdName)
        {
            var holdType = GetIKHoldType(holdName);
            return GetIKHoldEntity(holdType);
        }

        public FABRIK GetIkFabrik(IKHoldType holdType)
        {
            return GetIKHoldEntity(holdType).fabrik;
        }

        public IKDragPoint GetDragPoint(IKHoldType holdType)
        {
            return GetIKHoldEntity(holdType).dragPoint;
        }

        public WorldTransformAxis GetAxisObj(IKHoldType holdType)
        {
            return GetDragPoint(holdType).axis_obj;
        }

        public Vector3 GetIkPosition(IKHoldType holdType)
        {
            return GetIKHoldEntity(holdType).position;
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

        public Vector3 GetInitialPosition(IKManager.BoneType boneType)
        {
            return info.GetInitialPosition(boneType);
        }

        public void Reset()
        {
            maid = null;
            info = null;
            cacheBoneData = null;
            ikManager = null;
            extendBoneCache = null;
            maidPropCache = null;
            ikHoldEntities.Clear();
            _blendShapeCache.Clear();
            slotStats.Clear();
            slotStatMap.Clear();
            materialMap.Clear();
            materialNames.Clear();
            lookAtTargetType = LookAtTargetType.None;
            lookAtTargetIndex = 0;
            lookAtMaidPointType = MaidPointType.Head;
            eyeEulerAngle = Vector3.zero;
            oneShotVoiceName = string.Empty;
            oneShotVoiceStartTime = 0f;
            oneShotVoiceLength = 0f;
            voiceFadeTime = 0.1f;
            voicePitch = 1f;
            loopVoiceName = string.Empty;

            ResetAnm();
        }

        public void ResetAnm()
        {
            annName = "";
            anmId = 0;
            animationState = null;

            foreach (var info in animationLayerInfos)
            {
                info.Reset();
            }
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

        public void LateUpdate()
        {
            if (maid == null)
            {
                return;
            }

            foreach (var ikHoldEntity in ikHoldEntities.Values)
            {
                ikHoldEntity.LateUpdate();
            }
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
            var bone = ikManager.GetBone(boneType);
            return bone != null ? bone.transform : null;
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

        public MaidSlotStat GetSlotStat(TBody.SlotID slotId)
        {
            return slotStatMap.GetOrDefault(slotId);
        }

        public ModelMaterial GetMaterial(string name)
        {
            ModelMaterial material;
            if (materialMap.TryGetValue(name, out material))
            {
                return material;
            }
            return null;
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

            var ikHoldEntity = GetIKHoldEntity(boneName);
            if (ikHoldEntity != null)
            {
                return ikHoldEntity.transform;
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
                    return PluginUtils.MainCamera.transform;
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

        public TBodySkin GetSlot(TBody.SlotID slotId)
        {
            return maid != null ? maid.body0.goSlot[(int)slotId] : null;
        }

        public TBodySkin GetSlot(DressSlotID slotId)
        {
            if (DressUtils.IsShiftSlotId(slotId))
            {
                return null;
            }
            var bodySlotId = DressUtils.GetBodySlotId(slotId);
            return GetSlot(bodySlotId);
        }

        public MaidSlotStat GetSlotStat(DressSlotID slotId)
        {
            if (DressUtils.IsShiftSlotId(slotId))
            {
                return null;
            }
            var bodySlotId = DressUtils.GetBodySlotId(slotId);
            return GetSlotStat(bodySlotId);
        }

        public void PlayOneShotVoice()
        {
            PlayVoice(
                oneShotVoiceName,
                oneShotVoiceStartTime,
                oneShotVoiceLength,
                false);
            UpdateVoice();
        }

        private string _playingVoiceName = "";
        private float _voiceStopTime = 0f;

        public void PlayVoice(
            string fileName,
            float startTime,
            float length,
            bool isLoop)
        {
            if (maid == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(fileName))
            {
                maid.AudioMan.Stop(voiceFadeTime);
                return;
            }

            var fixedFileName = EndsWith(fileName, ".ogg");
            maid.AudioMan.LoadPlay(fixedFileName, voiceFadeTime, false, isLoop);

            if (maid.AudioMan.audiosource != null)
            {
                maid.AudioMan.audiosource.pitch = voicePitch;

                if (startTime > 0f)
                {
                    maid.AudioMan.audiosource.time = startTime;
                }
            }

            _playingVoiceName = fixedFileName;
            _voiceStopTime = length > 0f ? startTime + length : 0f;
        }

        public void UpdateVoice()
        {
            if (maid == null)
            {
                return;
            }

            if (_voiceStopTime > 0f)
            {
                if (maid.AudioMan.FileName != _playingVoiceName)
                {
                    _voiceStopTime = 0f;
                }
                else if (maid.AudioMan.isPlay() && maid.AudioMan.audiosource.time >= _voiceStopTime)
                {
                    maid.AudioMan.Stop(voiceFadeTime);
                    _voiceStopTime = 0f;
                }
            }

            if (!string.IsNullOrEmpty(loopVoiceName))
            {
                if (!maid.AudioMan.isPlay())
                {
                    PlayVoice(loopVoiceName, 0f, 0f, true);
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
            MTEUtils.LogDebug("Maid changed: " + (maid != null ? maid.name : "null"));

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

            maidPropCache = maid.gameObject.GetComponent<MaidPropCache>();
            if (maidPropCache == null)
            {
                maidPropCache = maid.gameObject.AddComponent<MaidPropCache>();
                maidPropCache.Init(maid);
            }

            foreach (var pair in DressUtils.DressSlotJpNameMap)
            {
                var dressSlotId = pair.Key;
                var slotJpName = pair.Value;
                if (DressUtils.IsShiftSlotId(dressSlotId))
                {
                    continue;
                }

                var slotId = DressUtils.GetBodySlotId(dressSlotId);
                var slot = GetSlot(dressSlotId);
                if (slot == null)
                {
                    continue;
                }

                var stat = new MaidSlotStat(slot, slotJpName);

                slotStats.Add(stat);
                slotStatMap[slotId] = stat;
            }

            UpdateMaterials();

            onMaidChanged?.Invoke(slotNo, maid);
        }

        public void UpdateMaterials()
        {
            materialMap.Clear();
            materialNames.Clear();

            foreach (var stat in slotStats)
            {
                foreach (var material in stat.materials)
                {
                    materialMap[material.name] = material;
                    materialNames.Add(material.name);
                }
            }
        }

        public AnimationLayerInfo GetAnimationLayerInfo(int layer)
        {
            return animationLayerInfos.GetOrDefault(layer);
        }

        public void ApplyAnimationLayerInfo(AnimationLayerInfo info, float t)
        {
            animationLayerInfos[info.layer] = info;

            if (animation == null)
            {
                return;
            }

            if (info.state == null || info.state.name != info.anmTag)
            {
                if (string.IsNullOrEmpty(info.anmName))
                {
                    if (info.state != null)
                    {
                        info.state.enabled = false;
                        info.state = null;
                    }
                }
                else if (GameUty.IsExistFile(info.anmName))
                {
                    info.state = maid.body0.CrossFadeLayer(
                        info.anmName,
                        GameUty.FileSystem,
                        info.layer,
                        false,
                        info.loop,
                        false,
                        0f,
                        info.weight);
                }
                else
                {
                    // マイポーズを検索
                    var path = MTEUtils.CombinePaths(PhotoModePoseSave.folder_path, info.anmName);
                    if (File.Exists(path))
                    {
                        info.state = maid.body0.CrossFadeLayerByFullPath(
                            path,
                            info.layer,
                            false,
                            info.loop,
                            false,
                            0f,
                            info.weight);
                    }
                }
            }

            if (info.state == null)
            {
                return;
            }

            info.state.wrapMode = info.loop ? WrapMode.Loop : WrapMode.Once;
            info.state.weight = info.weight;

            info.state.time = t;
            info.state.speed = 0f;
        }

        private void OnAnmChanged(string anmName)
        {
            MTEUtils.LogDebug("Animation changed: " + anmName);

            ResetAnm();

            this.annName = anmName;
            if (string.IsNullOrEmpty(annName))
            {
                return;
            }

            animationState = animation[annName.ToLower()];
            long.TryParse(annName, out anmId);

            onAnmChanged?.Invoke(slotNo, anmName);
        }

        public void OnPoseEditUpdated()
        {
            foreach (var ikHoldEntity in ikHoldEntities.Values)
            {
                if (!ikHoldEntity.isAnime)
                {
                    ikHoldEntity.ResetTargetPosition();
                }
            }
        }
    }
}