using System;
using System.IO;
using System.Reflection;
using CM3D2.MultipleMaids.Plugin;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class MultipleMaidsHack : StudioHackBase
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

                if (animationState != null)
                {
                    animationState.enabled = !value;
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

        public override Maid activeMaid
        {
            get
            {
                var maidArray = this.maidArray;
                var selectMaidIndex = this.selectMaidIndex;
                if (selectMaidIndex < 0 || selectMaidIndex >= maidArray.Length)
                {
                    return null;
                }

                return maidArray[selectMaidIndex];
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

        public override bool hasIkBoxVisible
        {
            get
            {
                return false;
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

                if (animationState != null)
                {
                    var maxNum = animationState.length;
                    var current = Mathf.Clamp01(value) * maxNum;
                    animationState.time = current;
                    animationState.enabled = true;
                    animation.Sample();
                    if (isStop)
                    {
                        animationState.enabled = false;
                    }
                }
            }
        }

        public override bool useMuneKeyL
        {
            get
            {
                if (maid != null)
                {
                    return !maid.body0.jbMuneL.enabled;
                }
                return false;
            }
            set
            {
                if (maid != null)
                {
                    maid.body0.jbMuneL.enabled = !value;
                    maid.body0.MuneYureL(maid.body0.jbMuneL.enabled ? 1 : 0);
                }
            }
        }

        public override bool useMuneKeyR
        {
            get
            {
                if (maid != null)
                {
                    return !maid.body0.jbMuneR.enabled;
                }
                return false;
            }
            set
            {
                if (maid != null)
                {
                    maid.body0.jbMuneR.enabled = !value;
                    maid.body0.MuneYureR(maid.body0.jbMuneR.enabled ? 1 : 0);
                }
            }
        }

        public MultipleMaidsHack()
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

        public override void OnChangedSceneLevel(Scene sceneName, LoadSceneMode sceneMode)
        {
            base.OnChangedSceneLevel(sceneName, sceneMode);
            isSceneActive = sceneName.name == "SceneEdit" || sceneName.name == "SceneDaily";
        }

        public override bool IsValid()
        {
            if (!base.IsValid())
            {
                return false;
            }

            if (!okFlg)
            {
                _errorMessage = "複数メイドを有効化してください";
                return false;
            }

            return true;
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

            if (maid == null)
            {
                return;
            }

            // 再生位置更新
            var animationState = StudioHackBase.animationState;
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
    }
}