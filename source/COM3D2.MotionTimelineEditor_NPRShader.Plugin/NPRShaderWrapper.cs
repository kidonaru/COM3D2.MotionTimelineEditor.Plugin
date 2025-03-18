using UnityEngine;
using COM3D2.MotionTimelineEditor;
using COM3D2.NPRShader.Plugin;
using System;

namespace COM3D2.MotionTimelineEditor_NPRShader.Plugin
{
    public class NPRShaderWrapper
    {
        public NPRShader.Plugin.NPRShader npr; 
        public NPRShaderField field = new NPRShaderField();

        public object envView
        {
            get => field.envView.GetValue(npr);
        }

        public bool bUpdateCubeMapRequest
        {
            get => (bool)field.bUpdateCubeMapRequest.GetValue(envView);
            set => field.bUpdateCubeMapRequest.SetValue(envView, value);
        }

        public ReflectionProbeController probe
        {
            get => (ReflectionProbeController)field.probe.GetValue(envView);
            set => field.probe.SetValue(envView, value);
        }

        private bool initialized = false;

        public bool Init()
        {
            {
                GameObject gameObject = GameObject.Find("UnityInjector");
                npr = gameObject.GetComponent<NPRShader.Plugin.NPRShader>();
                MTEUtils.AssertNull(npr != null, "NPRShader is null");
            }

            if (!field.Init())
            {
                return false;
            }

            initialized = true;
            return true;
        }

        public void UpdateCubeMap()
        {
            if (!initialized) return;
            field.UpdateCubeMap.Invoke(envView, null);
        }

        public void Reload()
        {
            try
            {
                if (!initialized) return;
                bUpdateCubeMapRequest = true;
                UpdateCubeMap();
                bUpdateCubeMapRequest = false;
                probe.RenderProbe();
            }
            catch (Exception ex)
            {
                MTEUtils.LogException(ex);
            }
        }
    }
}