using System.Collections.Generic;
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

        public static DepthOfFieldData Lerp(
            DepthOfFieldData a,
            DepthOfFieldData b,
            float t)
        {
            return new DepthOfFieldData
            {
                enabled = a.enabled,
                focalLength = Mathf.Lerp(a.focalLength, b.focalLength, t),
                focalSize = Mathf.Lerp(a.focalSize, b.focalSize, t),
                aperture = Mathf.Lerp(a.aperture, b.aperture, t),
                maxBlurSize = Mathf.Lerp(a.maxBlurSize, b.maxBlurSize, t),
                maidSlotNo = a.maidSlotNo,
            };
        }
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

        private ColorParaffinEffect _paraffin = null;
        public ColorParaffinEffect paraffin
        {
            get
            {
                if (_paraffin == null)
                {
                    _paraffin = mainCamera.GetComponent<ColorParaffinEffect>();
                    if (_paraffin == null)
                    {
                        _paraffin = mainCamera.gameObject.AddComponent<ColorParaffinEffect>();
                        _paraffin.enabled = false;
                    }
                }

                return _paraffin;
            }
        }

        private static MaidManager maidManager => MaidManager.instance;

        private static StudioHackBase studioHack => StudioHackManager.studioHack;

        private static Config config => ConfigManager.config;

        private static CameraMain mainCamera
        {
            get
            {
                return GameMain.Instance.MainCamera;
            }
        }

        private static TimelineData timeline
        {
            get
            {
                return TimelineManager.instance.timeline;
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
            _paraffin = null;
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

        public int GetParaffinCount()
        {
            return paraffin.GetParaffinCount();
        }
        
        public void AddParaffinData(ParaffinData data)
        {
            paraffin.AddParaffinData(data);
        }

        public void RemoveParaffinData()
        {
            paraffin.RemoveLastParaffinData();
        }

        public ParaffinData GetParaffinData(int index)
        {
            return paraffin.GetParaffinData(index);
        }

        public void DisableParaffin()
        {
            paraffin.enabled = false;
        }

        public void ApplyParaffin(int index, ParaffinData data)
        {
            if (data.enabled)
            {
                paraffin.enabled = true;
            }
            paraffin.SetParaffinData(index, data);
            paraffin.isDebug = config.paraffinDebug;
        }
    }
}