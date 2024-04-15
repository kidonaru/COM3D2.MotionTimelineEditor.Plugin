using UnityEngine;
using UnityEngine.SceneManagement;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    using MTE = MotionTimelineEditor;

    public abstract class StudioHackBase
    {
        public abstract Maid activeMaid { get; }
        public abstract string outputAnmPath { get; }
        public abstract bool hasIkBoxVisible { get; }
        public abstract bool isIkBoxVisibleRoot { get; set; }
        public abstract bool isIkBoxVisibleBody { get; set; }
        public abstract bool isPoseEditing { get; set; }
        public abstract bool isMotionPlaying { get; set; }
        public abstract float motionSliderRate { get; set; }
        public abstract bool useMuneKeyL { get; set; }
        public abstract bool useMuneKeyR { get; set; }

        protected string _errorMessage = "";
        public string errorMessage
        {
            get
            {
                return _errorMessage;
            }
        }

        private bool _isSceneActive = false;
        public bool isSceneActive
        {
            get
            {
                return _isSceneActive;
            }
            set
            {
                if (_isSceneActive == value)
                {
                    return;
                }
                _isSceneActive = value;

                if (_isSceneActive)
                {
                    OnSceneActive();
                }
                else
                {
                    OnSceneDeactive();
                }
            }
        }

        protected static MaidManager maidManager
        {
            get
            {
                return MaidManager.instance;
            }
        }

        protected static Maid maid
        {
            get
            {
                return maidManager.maid;
            }
        }

        protected static CacheBoneDataArray cacheBoneData
        {
            get
            {
                return maidManager.cacheBoneData;
            }
        }

        protected static Animation animation
        {
            get
            {
                return maidManager.animation;
            }
        }

        protected static AnimationState animationState
        {
            get
            {
                return maidManager.animationState;
            }
        }

        public StudioHackBase()
        {
            maidManager.onMaidChanged += OnMaidChanged;
            maidManager.onAnmChanged += OnAnmChanged;
        }

        public virtual void OnChangedSceneLevel(Scene sceneName, LoadSceneMode SceneMode)
        {
            // do nothing
        }

        public virtual void OnSceneActive()
        {
            // do nothing
        }

        public virtual void OnSceneDeactive()
        {
            MTE.instance.isShowWnd = false;
        }

        public virtual bool IsValid()
        {
            _errorMessage = "";
            return true;
        }

        public virtual void Update()
        {
            // do nothing
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

        protected virtual void OnMaidChanged(Maid maid)
        {
            // do nothing
        }

        protected virtual void OnAnmChanged(string anmName)
        {
            // do nothing
        }

        public virtual void OnMotionUpdated(Maid maid)
        {
            // do nothing
        }

        public virtual void OnUpdateMyPose(string anmPath, bool isExist)
        {
            // do nothing
        }
    }
}