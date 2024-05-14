using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    using MTE = MotionTimelineEditor;

    public abstract class StudioHackBase
    {
        public abstract int priority { get; }
        public abstract Maid selectedMaid { get; }
        public abstract List<Maid> allMaids { get; }
        public abstract List<StudioModelStat> modelList { get; }
        public abstract int selectedMaidSlotNo { get; }
        public abstract string outputAnmPath { get; }
        public abstract bool hasIkBoxVisible { get; }
        public abstract bool isIkBoxVisibleRoot { get; set; }
        public abstract bool isIkBoxVisibleBody { get; set; }
        public abstract bool isPoseEditing { get; set; }
        public abstract bool isMotionPlaying { get; set; }
        public abstract float motionSliderRate { set; }
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

        protected static StudioModelManager modelManager
        {
            get
            {
                return StudioModelManager.instance;
            }
        }

        public StudioHackBase()
        {
        }

        public virtual bool Init()
        {
            return true;
        }

        public virtual void ChangeMaid(Maid maid)
        {
            // do nothing
        }

        public virtual void OnChangedSceneLevel(Scene sceneName, LoadSceneMode SceneMode)
        {
            // do nothing
        }

        public virtual void OnSceneActive()
        {
            MaidCache.onMaidChanged += OnMaidChanged;
            MaidCache.onAnmChanged += OnAnmChanged;
        }

        public virtual void OnSceneDeactive()
        {
            MaidCache.onMaidChanged -= OnMaidChanged;
            MaidCache.onAnmChanged -= OnAnmChanged;
            MTE.instance.isEnable = false;
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

        public virtual void ClearPoseHistory()
        {
            // do nothing
        }

        public virtual void DeleteAllModels()
        {
            // do nothing
        }

        public virtual void DeleteModel(StudioModelStat model)
        {
            // do nothing
        }

        public virtual void CreateModel(StudioModelStat model)
        {
            // do nothing
        }

        private void DeleteBGObject()
		{
			BgMgr bgMgr = GameMain.Instance.BgMgr;
			UnityEngine.Object.Destroy(bgMgr.current_bg_object);
			bgMgr.DeleteBg();
		}

        public virtual void ChangeBackground(string bgName)
        {
            if (bgName != GameMain.Instance.BgMgr.GetBGName())
            {
                DeleteBGObject();
                GameMain.Instance.BgMgr.ChangeBg(bgName);
            }
        }

        public virtual void SetBackgroundVidible(bool visible)
        {
            var bgObject = GameMain.Instance.BgMgr.current_bg_object;
            if (bgObject != null)
            {
                bgObject.SetActive(visible);
            }
        }

        public virtual bool IsBackgroundVidible()
        {
            var bgObject = GameMain.Instance.BgMgr.current_bg_object;
            return bgObject != null && bgObject.activeSelf;
        }

        protected virtual void OnMaidChanged(int maidSlotNo, Maid maid)
        {
            // do nothing
        }

        protected virtual void OnAnmChanged(int maidSlotNo, string anmName)
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