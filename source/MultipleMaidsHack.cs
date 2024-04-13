using System.IO;
using System.Reflection;
using CM3D2.MultipleMaids.Plugin;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class MultipleMaidsHack : MaidHackBase
    {
        private MultipleMaids multipleMaids = null;
        private IK _ik = new IK();

        private FieldInfo fieldMaidArray = null;
        private FieldInfo fieldSelectMaidIndex = null;
        private FieldInfo fieldIsLock = null;
        private FieldInfo fieldUnLockFlg = null;
        private FieldInfo fieldIsStop = null;
        private FieldInfo fieldIsBone = null;
        private FieldInfo fieldOkFlg = null;

        private Maid[] maidArray
        {
            get
            {
                return (Maid[])fieldMaidArray.GetValue(multipleMaids);
            }
        }

        private int selectMaidIndex
        {
            get
            {
                return (int)fieldSelectMaidIndex.GetValue(multipleMaids);
            }
        }

        private bool[] isLockArray
        {
            get
            {
                return (bool[])fieldIsLock.GetValue(multipleMaids);
            }
        }

        private bool isLock
        {
            get
            {
                var isLockArray = this.isLockArray;
                var selectMaidIndex = this.selectMaidIndex;
                if (selectMaidIndex >= 0 && selectMaidIndex < isLockArray.Length)
                {
                    return isLockArray[selectMaidIndex];
                }

                return false;
            }
            set
            {
                var isLockArray = this.isLockArray;
                var selectMaidIndex = this.selectMaidIndex;
                if (selectMaidIndex >= 0 && selectMaidIndex < isLockArray.Length)
                {
                    isLockArray[selectMaidIndex] = value;
                }
            }
        }

        private bool unLockFlg
        {
            get
            {
                return (bool)fieldUnLockFlg.GetValue(multipleMaids);
            }
            set
            {
                fieldUnLockFlg.SetValue(multipleMaids, value);
            }
        }

        private bool[] isStopArray
        {
            get
            {
                return (bool[])fieldIsStop.GetValue(multipleMaids);
            }
        }

        private bool isStop
        {
            get
            {
                var isStopArray = this.isStopArray;
                var selectMaidIndex = this.selectMaidIndex;
                if (selectMaidIndex >= 0 && selectMaidIndex < isStopArray.Length)
                {
                    return isStopArray[selectMaidIndex];
                }

                return false;
            }
            set
            {
                var isStopArray = this.isStopArray;
                var selectMaidIndex = this.selectMaidIndex;
                if (selectMaidIndex >= 0 && selectMaidIndex < isStopArray.Length)
                {
                    isStopArray[selectMaidIndex] = value;
                }

                if (_animationState != null)
                {
                    _animationState.enabled = !value;
                }
            }
        }

        private bool[] isBoneArray
        {
            get
            {
                return (bool[])fieldIsBone.GetValue(multipleMaids);
            }
        }

        private bool isBone
        {
            get
            {
                var isBoneArray = this.isBoneArray;
                var selectMaidIndex = this.selectMaidIndex;
                if (selectMaidIndex >= 0 && selectMaidIndex < isBoneArray.Length)
                {
                    return isBoneArray[selectMaidIndex];
                }

                return false;
            }
            set
            {
                var isBoneArray = this.isBoneArray;
                var selectMaidIndex = this.selectMaidIndex;
                if (selectMaidIndex >= 0 && selectMaidIndex < isBoneArray.Length)
                {
                    isBoneArray[selectMaidIndex] = value;
                }
            }
        }

        private bool okFlg
        {
            get
            {
                return (bool)fieldOkFlg.GetValue(multipleMaids);
            }
        }

        public override string outputAnmPath
        {
            get
            {
                var path = Path.GetFullPath(".\\") + "Mod\\MultipleMaidsPose\\";
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                return path;
            }
        }

        public override bool isIkBoxVisibleRoot
        {
            get
            {
                return false;
            }
            set
            {
                // do nothing
            }
        }

        public override bool isIkBoxVisibleBody
        {
            get
            {
                return false;
            }
            set
            {
                // do nothing
            }
        }

        public override bool isPoseEditing
        {
            get
            {
                return isLock;
            }
            set
            {
                if (value == isPoseEditing)
                {
                    return;
                }

                if (value && isMotionPlaying)
                {
                    isStop = true;
                }

                isLock = value;
                isBone = value;
                unLockFlg = value;
            }
        }

        public override bool isMotionPlaying
        {
            get
            {
                return !isStop;
            }
            set
            {
                if (value == isMotionPlaying)
                {
                    return;
                }

                if (value && isPoseEditing)
                {
                    isLock = false;
                    isBone = false;
                    unLockFlg = false;
                }

                isStop = !value;
            }
        }

        private Transform clavicleL
        {
            get
            {
                return cacheBoneData.GetBoneData(IKManager.BoneType.Clavicle_L).transform;
            }
        }

        private Transform upperArmL
        {
            get
            {
                return cacheBoneData.GetBoneData(IKManager.BoneType.UpperArm_L).transform;
            }
        } 

        private Transform forearmL
        {
            get
            {
                return cacheBoneData.GetBoneData(IKManager.BoneType.Forearm_L).transform;
            }
        }

        private Transform handL
        {
            get
            {
                return cacheBoneData.GetBoneData(IKManager.BoneType.Hand_L).transform;
            }
        }

        private Transform clavicleR
        {
            get
            {
                return cacheBoneData.GetBoneData(IKManager.BoneType.Clavicle_R).transform;
            }
        }

        private Transform upperArmR
        {
            get
            {
                return cacheBoneData.GetBoneData(IKManager.BoneType.UpperArm_R).transform;
            }
        }

        private Transform forearmR
        {
            get
            {
                return cacheBoneData.GetBoneData(IKManager.BoneType.Forearm_R).transform;
            }
        }

        private Transform handR
        {
            get
            {
                return cacheBoneData.GetBoneData(IKManager.BoneType.Hand_R).transform;
            }
        }

        private Transform hipL
        {
            get
            {
                return cacheBoneData.GetBoneData(IKManager.BoneType.Hip_L).transform;
            }
        }

        private Transform calfL
        {
            get
            {
                return cacheBoneData.GetBoneData(IKManager.BoneType.Calf_L).transform;
            }
        }

        private Transform thighL
        {
            get
            {
                return cacheBoneData.GetBoneData(IKManager.BoneType.Thigh_L).transform;
            }
        }

        private Transform footL
        {
            get
            {
                return cacheBoneData.GetBoneData(IKManager.BoneType.Foot_L).transform;
            }
        }

        private Transform hipR
        {
            get
            {
                return cacheBoneData.GetBoneData(IKManager.BoneType.Hip_R).transform;
            }
        }

        private Transform calfR
        {
            get
            {
                return cacheBoneData.GetBoneData(IKManager.BoneType.Calf_R).transform;
            }
        }

        private Transform thighR
        {
            get
            {
                return cacheBoneData.GetBoneData(IKManager.BoneType.Thigh_R).transform;
            }
        }

        private Transform footR
        {
            get
            {
                return cacheBoneData.GetBoneData(IKManager.BoneType.Foot_R).transform;
            }
        }

        private float _motionSliderRate = 0f;

        public override float motionSliderRate
        {
            get
            {
                return _motionSliderRate;
            }
            set
            {
                //Extensions.LogDebug("motionSliderRate update：" + value);
                _motionSliderRate = value;

                if (_animationState != null)
                {
                    var maxNum = _animationState.length;
                    var current = Mathf.Clamp01(value) * maxNum;
                    _animationState.time = current;
                    _animationState.enabled = true;
                    _animation.Sample();
                    if (isStop)
                    {
                        _animationState.enabled = false;
                    }
                }
            }
        }

        public override bool useMuneKeyL
        {
            get
            {
                if (_maid != null)
                {
                    return !_maid.body0.jbMuneL.enabled;
                }
                return false;
            }
            set
            {
                if (_maid != null)
                {
                    _maid.body0.jbMuneL.enabled = !value;
                    _maid.body0.MuneYureL(_maid.body0.jbMuneL.enabled ? 1 : 0);
                }
            }
        }

        public override bool useMuneKeyR
        {
            get
            {
                if (_maid != null)
                {
                    return !_maid.body0.jbMuneR.enabled;
                }
                return false;
            }
            set
            {
                if (_maid != null)
                {
                    _maid.body0.jbMuneR.enabled = !value;
                    _maid.body0.MuneYureR(_maid.body0.jbMuneR.enabled ? 1 : 0);
                }
            }
        }

        public override void Init()
        {
            Extensions.Log("MultipleMaidsHack初期化中...");
            {
                GameObject gameObject = GameObject.Find("UnityInjector");
                multipleMaids = gameObject.GetComponent<MultipleMaids>();
                Extensions.AssertNull(multipleMaids != null);
            }

            {
                BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.InvokeMethod;

                fieldMaidArray = typeof(MultipleMaids).GetField("maidArray", bindingAttr);
                Extensions.AssertNull(fieldMaidArray != null);

                fieldSelectMaidIndex = typeof(MultipleMaids).GetField("selectMaidIndex", bindingAttr);
                Extensions.AssertNull(fieldSelectMaidIndex != null);

                fieldIsLock = typeof(MultipleMaids).GetField("isLock", bindingAttr);
                Extensions.AssertNull(fieldIsLock != null);

                fieldUnLockFlg = typeof(MultipleMaids).GetField("unLockFlg", bindingAttr);
                Extensions.AssertNull(fieldUnLockFlg != null);

                fieldIsStop = typeof(MultipleMaids).GetField("isStop", bindingAttr);
                Extensions.AssertNull(fieldIsStop != null);

                fieldIsBone = typeof(MultipleMaids).GetField("isBone", bindingAttr);
                Extensions.AssertNull(fieldIsBone != null);

                fieldOkFlg = typeof(MultipleMaids).GetField("okFlg", bindingAttr);
                Extensions.AssertNull(fieldOkFlg != null);
            }
        }

        public override bool IsValid()
        {
            _errorMessage = "";

            if (!okFlg)
            {
                _errorMessage = "複数メイドを有効化してください";
                return false;
            }

            if (GetMaid() == null)
            {
                _errorMessage = "メイドを配置してください";
                return false;
            }

            return true;
        }

        protected override Maid GetMaid()
        {
            var maidArray = this.maidArray;
            var selectMaidIndex = this.selectMaidIndex;
            if (selectMaidIndex < 0 || selectMaidIndex >= maidArray.Length)
            {
                return null;
            }

            return maidArray[selectMaidIndex];
        }

        protected override void OnMaidChanged(Maid maid)
        {
            base.OnMaidChanged(maid);
            motionSliderRate = _motionSliderRate;
        }

        protected override void OnAnmChanged(string anmName)
        {
            base.OnAnmChanged(anmName);
            motionSliderRate = _motionSliderRate;
        }

        public override void Update()
        {
            base.Update();

            if (_maid == null)
            {
                return;
            }

            // 再生位置更新
            if (_animationState != null && _animationState.enabled && _animationState.length > 0f)
            {
                float value = _animationState.time;
                if (_animationState.length < _animationState.time)
                {
                    if (_animationState.wrapMode == WrapMode.ClampForever)
                    {
                        value = _animationState.length;
                    }
                    else
                    {
                        value = _animationState.time - _animationState.length * (float)((int)(_animationState.time / _animationState.length));
                    }
                }
                _motionSliderRate = value / _animationState.length;
            }
        }

        private Transform GetBoneTransform(IKHoldType holdType)
        {
            switch (holdType)
            {
                case IKHoldType.Arm_L_Joint:
                    return forearmL;
                case IKHoldType.Arm_L_Tip:
                    return handL;
                case IKHoldType.Arm_R_Joint:
                    return forearmR;
                case IKHoldType.Arm_R_Tip:
                    return handR;
                case IKHoldType.Foot_L_Joint:
                    return calfL;
                case IKHoldType.Foot_L_Tip:
                    return footL;
                case IKHoldType.Foot_R_Joint:
                    return calfR;
                case IKHoldType.Foot_R_Tip:
                    return footR;
                default:
                    return null;
            }
        }

        public override Vector3 GetIkPosition(IKHoldType holdType)
        {
            var transform = GetBoneTransform(holdType);
            if (transform != null)
            {
                return transform.position;
            }
            return Vector3.zero;
        }

        private void PorcIk(
            Transform hip,
            Transform knee,
            Transform ankle,
            Vector3 targetPosition)
        {
            _ik.Init(hip, knee, ankle, maid.body0);
			_ik.Porc(hip, knee, ankle, targetPosition, default(Vector3));
        }

        public override void UpdateIkPosition(IKHoldType holdType, Vector3 targetPosition)
        {
            switch (holdType)
            {
                case IKHoldType.Arm_L_Joint:
                    PorcIk(clavicleL, upperArmL, forearmL, targetPosition);
                    break;
                case IKHoldType.Arm_L_Tip:
                    PorcIk(upperArmL, forearmL, handL, targetPosition);
                    break;
                case IKHoldType.Arm_R_Joint:
                    PorcIk(clavicleR, upperArmR, forearmR, targetPosition);
                    break;
                case IKHoldType.Arm_R_Tip:
                    PorcIk(upperArmR, forearmR, handR, targetPosition);
                    break;
                case IKHoldType.Foot_L_Joint:
                    PorcIk(hipL, thighL, calfL, targetPosition);
                    break;
                case IKHoldType.Foot_L_Tip:
                    PorcIk(thighL, calfL, footL, targetPosition);
                    break;
                case IKHoldType.Foot_R_Joint:
                    PorcIk(hipR, thighR, calfR, targetPosition);
                    break;
                case IKHoldType.Foot_R_Tip:
                    PorcIk(thighR, calfR, footR, targetPosition);
                    break;
                default:
                    break;
            }
        }
    }
}