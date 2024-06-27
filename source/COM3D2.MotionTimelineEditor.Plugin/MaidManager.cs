using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class MaidManager
    {
        public List<MaidCache> maidCaches = new List<MaidCache>();
        public int maidSlotNo { get; private set; }
        private string _errorMessage = "";

        public static event UnityAction<int> onMaidSlotNoChanged;

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

        public MaidCache maidCache
        {
            get
            {
                if (maidSlotNo < 0 || maidSlotNo >= maidCaches.Count)
                {
                    return null;
                }
                return maidCaches[maidSlotNo];
            }
        }

        public Maid maid
        {
            get
            {
                if (maidCache != null)
                {
                    return maidCache.maid;
                }
                return null;
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
                if (maidCache != null)
                {
                    return maidCache.animation;
                }
                return null;
            }
        }

        public AnimationState animationState
        {
            get
            {
                if (maidCache != null)
                {
                    return maidCache.animationState;
                }
                return null;
            }
        }

        public string annName
        {
            get
            {
                if (maidCache != null)
                {
                    return maidCache.annName;
                }
                return "";
            }
        }

        public float anmSpeed
        {
            get
            {
                if (maidCache != null)
                {
                    return maidCache.anmSpeed;
                }
                return 0;
            }
        }

        public IKManager ikManager
        {
            get
            {
                if (maidCache != null)
                {
                    return maidCache.ikManager;
                }
                return null;
            }
        }

        public CacheBoneDataArray cacheBoneData
        {
            get
            {
                if (maidCache != null)
                {
                    return maidCache.cacheBoneData;
                }
                return null;
            }
        }

        private static StudioHackBase studioHack
        {
            get
            {
                return StudioHackManager.studioHack;
            }
        }

        private MaidManager()
        {
            SceneManager.sceneLoaded += OnChangedSceneLevel;
            TimelineManager.onRefresh += OnRefresh;
        }

        public Maid GetMaid(int slotNo)
        {
            if (slotNo < 0 || slotNo >= studioHack.allMaids.Count)
            {
                return null;
            }
            return studioHack.allMaids[slotNo];
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

            var maid = studioHack.selectedMaid;
            if (maid == null)
            {
                _errorMessage = "メイドを配置してください";
                return false;
            }

            var maids = studioHack.allMaids;
            foreach (var m in maids)
            {
                if (m.body0 == null || m.body0.m_Bones == null ||
                    m.body0.trsEyeL == null || m.body0.trsEyeR == null ||
                    m.IsAllProcPropBusy)
                {
                    _errorMessage = "メイド生成中です";
                    return false;
                }
            }

            return true;
        }

        public void Reset()
        {
            foreach (var cache in maidCaches)
            {
                cache.Reset();
            }
        }

        public void ResetAnm()
        {
            foreach (var cache in maidCaches)
            {
                cache.ResetAnm();
            }
        }

        public void Update()
        {
            if (!IsValid())
            {
                Reset();
                return;
            }

            var maids = studioHack.allMaids;
            for (int i = maidCaches.Count; i < maids.Count; i++)
            {
                maidCaches.Add(new MaidCache(i));
            }
            if (maidCaches.Count > maids.Count)
            {
                maidCaches.RemoveRange(maids.Count, maidCaches.Count - maids.Count);
            }

            for (int i = 0; i < maids.Count; i++)
            {
                var maid = maids[i];
                var cache = maidCaches[i];
                cache.Update(maid);
            }

            var selectedMaidSlotNo = studioHack.selectedMaidSlotNo;
            if (selectedMaidSlotNo != maidSlotNo)
            {
                maidSlotNo = selectedMaidSlotNo;

                if (onMaidSlotNoChanged != null)
                {
                    onMaidSlotNoChanged(maidSlotNo);
                }
            }
        }

        public void OnPluginDisable()
        {
            foreach (var cache in maidCaches)
            {
                cache.ResetEyes();
            }

            Reset();
        }

        public void OnPluginEnable()
        {
        }

        public Vector3 GetIkPosition(IKHoldType holdType)
        {
            var cache = this.maidCache;
            if (cache != null)
            {
                return cache.GetIkPosition(holdType);
            }
            return Vector3.zero;
        }

        public void UpdateIkPosition(IKHoldType holdType, Vector3 targetPosition)
        {
            var cache = this.maidCache;
            if (cache != null)
            {
                cache.UpdateIkPosition(holdType, targetPosition);
            }
        }

        public void AdjustFootGrounding(IKHoldType holdType)
        {
            var cache = this.maidCache;
            if (cache != null)
            {
                cache.AdjustFootGrounding(holdType);
            }
        }

        public void PositonCorrection(IKHoldType holdType)
        {
            var cache = this.maidCache;
            if (cache != null)
            {
                cache.PositonCorrection(holdType);
            }
        }

        public MaidCache GetMaidCache(int slotNo)
        {
            if (slotNo < 0 || slotNo >= maidCaches.Count)
            {
                return null;
            }
            return maidCaches[slotNo];
        }

        public MaidCache GetMaidCache(Maid maid)
        {
            foreach (var cache in maidCaches)
            {
                if (cache.maid == maid)
                {
                    return cache;
                }
            }
            return null;
        }

        public void SetMotionPlayingAll(bool isPlaying)
        {
            foreach (var cache in maidCaches)
            {
                var animationState = cache.animationState;
                if (animationState != null)
                {
                    animationState.enabled = isPlaying;
                }
            }
        }

        public void SetPlayingFrameNoAll(int frameNo)
        {
            foreach (var cache in maidCaches)
            {
                cache.playingFrameNo = frameNo;
            }
        }

        public void SetAnmSpeedAll(float speed)
        {
            foreach (var cache in maidCaches)
            {
                cache.anmSpeed = speed;
            }
        }

        public void UpdateMuneYure()
        {
            foreach (var cache in maidCaches)
            {
                cache.UpdateMuneYure();
            }
        }

        public void UpdateHeadLook()
        {
            foreach (var cache in maidCaches)
            {
                cache.UpdateHeadLook();
            }
        }

        public void ChangeMaid(Maid maid)
        {
            if (maid == null || maid == studioHack.selectedMaid)
            {
                return;
            }

            PluginUtils.LogDebug("ChangeMaid: " + maid.name);

            var isMotionPlaying = studioHack.isMotionPlaying;
            studioHack.isPoseEditing = false;
            studioHack.ChangeMaid(maid);
            studioHack.isMotionPlaying = isMotionPlaying;
        }

        public void OnMotionUpdated(Maid maid)
        {
            var cache = GetMaidCache(maid);
            if (cache != null)
            {
                cache.ResetAnm();
                cache.Update(maid);
            }
        }

        private void OnChangedSceneLevel(Scene sceneName, LoadSceneMode SceneMode)
        {
            Reset();
            MaidInfo.Reset();
        }

        private void OnRefresh()
        {
            ResetAnm();
            Update();
        }
    }
}