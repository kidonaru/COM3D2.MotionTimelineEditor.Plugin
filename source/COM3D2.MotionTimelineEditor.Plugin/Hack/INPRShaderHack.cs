using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public interface INPRShaderHack
    {
        bool Init();
        void UpdateMaterial(GameObject gameObject);
    }
}