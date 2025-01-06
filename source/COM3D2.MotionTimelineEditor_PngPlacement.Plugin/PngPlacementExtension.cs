using COM3D2.MotionTimelineEditor.Plugin;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor_PngPlacement.Plugin
{
    public static class PngPlacementExtension
    {
        private static PngPlacementManager manager => PngPlacementManager.instance;

        public static void SetEnable(this PngObjectDataWrapper pngObj, bool b)
        {
            manager.pngPlacement.SetEnable(pngObj, b);
        }

        public static void SetAPNGSpeed(this PngObjectDataWrapper pngObj, float f)
        {
            manager.pngPlacement.SetAPNGSpeed(pngObj, f);
        }

        public static void SetAPNGIsFixedSpeed(this PngObjectDataWrapper pngObj, bool b)
        {
            manager.pngPlacement.SetAPNGIsFixedSpeed(pngObj, b);
        }

        public static void SetScale(this PngObjectDataWrapper pngObj, float f)
        {
            manager.pngPlacement.SetScale(pngObj, f);
        }

        public static void SetScaleMag(this PngObjectDataWrapper pngObj, int i)
        {
            manager.pngPlacement.SetScaleMag(pngObj, i);
        }

        public static void SetScaleZ(this PngObjectDataWrapper pngObj, float f)
        {
            manager.pngPlacement.SetScaleZ(pngObj, f);
        }

        public static void SetRotation(this PngObjectDataWrapper pngObj, Vector3 v)
        {
            manager.pngPlacement.SetRotation(pngObj, v);
        }

        public static void SetStopRotation(this PngObjectDataWrapper pngObj, bool b)
        {
            manager.pngPlacement.SetStopRotation(pngObj, b);
        }

        public static void SetFixedCamera(this PngObjectDataWrapper pngObj, bool b)
        {
            manager.pngPlacement.SetFixedCamera(pngObj, b);
        }

        public static void SetInversion(this PngObjectDataWrapper pngObj, bool b)
        {
            manager.pngPlacement.SetInversion(pngObj, b);
        }

        public static void SetBrightness(this PngObjectDataWrapper pngObj, byte b)
        {
            manager.pngPlacement.SetBrightness(pngObj, b);
        }

        public static void SetColor(this PngObjectDataWrapper pngObj, Color c)
        {
            manager.pngPlacement.SetColor(pngObj, c);
        }

        public static void SetShaderName(this PngObjectDataWrapper pngObj, string s)
        {
            manager.pngPlacement.SetShaderName(pngObj, s);
        }

        public static void SetRenderQueue(this PngObjectDataWrapper pngObj, int i)
        {
            manager.pngPlacement.SetRenderQueue(pngObj, i);
        }

        public static void SetAttachPoint(this PngObjectDataWrapper pngObj, PngAttachPoint p, int iMaid)
        {
            manager.pngPlacement.SetAttachPoint(pngObj, p, iMaid);
        }

        public static void SetAttachRotation(this PngObjectDataWrapper pngObj, bool b)
        {
            manager.pngPlacement.SetAttachRotation(pngObj, b);
        }

        public static string GetShaderName(this TimelinePngObjectData objectData)
        {
            return manager.pngPlacement.GetShaderName(objectData.shaderDisplay);
        }

        public static void SetStopRotationVector(this PngObjectDataWrapper pngObj, Vector3 v)
        {
            manager.pngPlacement.SetStopRotationVector(pngObj, v);
        }
    }
}