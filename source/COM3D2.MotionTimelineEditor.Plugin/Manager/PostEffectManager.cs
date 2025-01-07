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

    public class PostEffectManager : ManagerBase
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

        private PostEffectController _controller = null;
        public PostEffectController controller
        {
            get
            {
                if (_controller == null)
                {
                    _controller = mainCamera.GetOrAddComponent<PostEffectController>();
                }
                return _controller;
            }
        }

        public ColorParaffinEffectSettings paraffin => controller.context.paraffinSettings;
        public DistanceFogEffectSettings distanceFog => controller.context.fogSettings;
        public RimlightEffectSettings rimlight => controller.context.rimlightSettings;

        private static Camera mainCamera => PluginUtils.MainCamera;

        private PostEffectManager()
        {
        }

        public override void Init()
        {
        }

        public override void OnLoad()
        {
            DisableAllEffects();
            InitPostEffects();
        }

        public override void OnPluginDisable()
        {
            DisableAllEffects();
            ResetCache();
            ReleaseController();
        }

        public void InitPostEffects()
        {
            InitParrifinEffect();
            InitDistanceFogEffect();
            InitRimlightEffect();
        }

        private void InitParrifinEffect()
        {
            while (postEffectManager.GetParaffinCount() < timeline.paraffinCount)
            {
                postEffectManager.AddParaffinData();
            }
            while (postEffectManager.GetParaffinCount() > timeline.paraffinCount)
            {
                postEffectManager.RemoveParaffinData();
            }
        }

        private void InitDistanceFogEffect()
        {
            while (postEffectManager.GetDistanceFogCount() < timeline.distanceFogCount)
            {
                postEffectManager.AddDistanceFogData();
            }
            while (postEffectManager.GetDistanceFogCount() > timeline.distanceFogCount)
            {
                postEffectManager.RemoveDistanceFogData();
            }
        }

        private void InitRimlightEffect()
        {
            while (postEffectManager.GetRimlightCount() < timeline.rimlightCount)
            {
                postEffectManager.AddRimlightData();
            }
            while (postEffectManager.GetRimlightCount() > timeline.rimlightCount)
            {
                postEffectManager.RemoveRimlightData();
            }
        }

        private void ResetCache()
        {
            _depthOfField = null;
        }

        private void ReleaseController()
        {
            if (_controller != null)
            {
                Object.Destroy(_controller);
                _controller = null;
            }
        }

        public void DisableAllEffects()
        {
            depthOfField.enabled = false;
            paraffin.enabled = false;
            distanceFog.enabled = false;
            rimlight.enabled = false;
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
            return paraffin.GetDataCount();
        }
        
        public void AddParaffinData()
        {
            paraffin.AddData(new ColorParaffinData());
        }

        public void RemoveParaffinData()
        {
            paraffin.RemoveDataLast();
        }

        public ColorParaffinData GetParaffinData(int index)
        {
            return paraffin.GetData(index);
        }

        public void ApplyParaffin(int index, ColorParaffinData data)
        {
            if (data.enabled)
            {
                paraffin.enabled = true;
            }
            paraffin.SetData(index, data);
            paraffin.isDebugView = config.paraffinDebug;
        }

        public int GetDistanceFogCount()
        {
            return distanceFog.GetDataCount();
        }
        
        public void AddDistanceFogData()
        {
            distanceFog.AddData(new DistanceFogData());
        }

        public void RemoveDistanceFogData()
        {
            distanceFog.RemoveDataLast();
        }

        public DistanceFogData GetDistanceFogData(int index)
        {
            return distanceFog.GetData(index);
        }

        public void ApplyDistanceFog(int index, DistanceFogData data)
        {
            if (data.enabled)
            {
                distanceFog.enabled = true;
            }
            distanceFog.SetData(index, data);
            distanceFog.isDebugView = config.distanceFogDebug;
        }

        public int GetRimlightCount()
        {
            return rimlight.GetDataCount();
        }
        
        public void AddRimlightData()
        {
            rimlight.AddData(new RimlightData());
        }

        public void RemoveRimlightData()
        {
            rimlight.RemoveDataLast();
        }

        public RimlightData GetRimlightData(int index)
        {
            return rimlight.GetData(index);
        }

        public void ApplyRimlight(int index, RimlightData data)
        {
            if (data.enabled)
            {
                rimlight.enabled = true;
            }
            rimlight.SetData(index, data);
            rimlight.isDebugView = config.rimlightDebug;
        }
    }
}