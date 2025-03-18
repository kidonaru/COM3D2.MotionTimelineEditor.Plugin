using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public interface INPRShaderHack
    {
        bool Init();
        void Reload();
        void UpdateMaterial(GameObject gameObject, string menuFileName);
    }
}