using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class DepthOfFieldData
    {
        public bool enabled;
        public float focalLength;
        public float focalSize;
        public float aperture;
        public float maxBlurSize;
        public int maidSlotNo;
    }

    public class PostEffectManager
    {
        private static PostEffectManager _instance;
        public static PostEffectManager instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new PostEffectManager();
                }

                return _instance;
            }
        }

        private DepthOfFieldScatter _depthOfField = null;
        public DepthOfFieldScatter depthOfField
        {
            get
            {
                if (_depthOfField == null)
                {
                    _depthOfField = studioHack.depthOfField;
                }

                return _depthOfField;
            }
        }

        public int depthOfFieldMaidSlotId = -1;

        private static MaidManager maidManager
        {
            get
            {
                return MaidManager.instance;
            }
        }

        private static StudioHackBase studioHack
        {
            get
            {
                return StudioHackManager.studioHack;
            }
        }

        private static Config config
        {
            get
            {
                return ConfigManager.config;
            }
        }

        private PostEffectManager()
        {
        }

        public void Init()
        {
            TimelineManager.onRefresh += ResetCache;
        }

        public void OnPluginDisable()
        {
            ResetCache();
        }

        public void OnPluginEnable()
        {
        }

        private void ResetCache()
        {
            _depthOfField = null;
        }

        public DepthOfFieldData GetDepthOfFieldData()
        {
            DepthOfFieldData data = new DepthOfFieldData();
            data.enabled = depthOfField.enabled;
            data.focalLength = depthOfField.focalLength;
            data.focalSize = depthOfField.focalSize;
            data.aperture = depthOfField.aperture;
            data.maxBlurSize = depthOfField.maxBlurSize;
            data.maidSlotNo = depthOfFieldMaidSlotId;
            return data;
        }

        public void DisableDepthOfField()
        {
            depthOfField.enabled = false;
        }

        public void ApplyDepthOfField(DepthOfFieldData data)
        {
            depthOfField.enabled = data.enabled;
            depthOfField.focalLength = data.focalLength;
            depthOfField.focalSize = data.focalSize;
            depthOfField.aperture = data.aperture;
            depthOfField.maxBlurSize = data.maxBlurSize;
            depthOfFieldMaidSlotId = data.maidSlotNo;
            depthOfField.visualizeFocus = config.dofVisualizeFocus;
            depthOfField.highResolution = config.dofHighResolution;
            depthOfField.nearBlur = config.dofNearBlur;

            Transform focalTransform = null;
            if (data.maidSlotNo >= 0)
            {
                var maid = maidManager.GetMaid(data.maidSlotNo);
                if (maid != null)
                {
                    focalTransform = maid.body0.trsHead;
                }
            }
            depthOfField.focalTransform = focalTransform;

            studioHack.OnUpdateDepthOfField();
        }
    }
}