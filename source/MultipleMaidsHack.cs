using System.IO;
using System.Reflection;
using CM3D2.MultipleMaids.Plugin;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class MultipleMaidsHack : MaidHackBase
    {
        private MultipleMaids multipleMaids = null;

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
            PluginUtils.Log("MultipleMaidsHack初期化中...");
            {
                GameObject gameObject = GameObject.Find("UnityInjector");
                multipleMaids = gameObject.GetComponent<MultipleMaids>();
                PluginUtils.AssertNull(multipleMaids != null);
            }

            {
                BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.InvokeMethod;

                fieldMaidArray = typeof(MultipleMaids).GetField("maidArray", bindingAttr);
                PluginUtils.AssertNull(fieldMaidArray != null);

                fieldSelectMaidIndex = typeof(MultipleMaids).GetField("selectMaidIndex", bindingAttr);
                PluginUtils.AssertNull(fieldSelectMaidIndex != null);

                fieldIsLock = typeof(MultipleMaids).GetField("isLock", bindingAttr);
                PluginUtils.AssertNull(fieldIsLock != null);

                fieldUnLockFlg = typeof(MultipleMaids).GetField("unLockFlg", bindingAttr);
                PluginUtils.AssertNull(fieldUnLockFlg != null);

                fieldIsStop = typeof(MultipleMaids).GetField("isStop", bindingAttr);
                PluginUtils.AssertNull(fieldIsStop != null);

                fieldIsBone = typeof(MultipleMaids).GetField("isBone", bindingAttr);
                PluginUtils.AssertNull(fieldIsBone != null);

                fieldOkFlg = typeof(MultipleMaids).GetField("okFlg", bindingAttr);
                PluginUtils.AssertNull(fieldOkFlg != null);
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
    }
}