using System.Collections.Generic;
using System.Reflection;
using COM3D2.MotionTimelineEditor.Plugin;
using MeidoPhotoStudio.Plugin;

namespace COM3D2.MotionTimelineEditor_MeidoPhotoStudio.Plugin
{
    using MPS = MeidoPhotoStudio.Plugin.MeidoPhotoStudio;
    using CameraManager = MeidoPhotoStudio.Plugin.CameraManager;

    public class MeidoPhotoStudioField
    {
        public FieldInfo active;
        public FieldInfo meidoManager;
        public FieldInfo windowManager;
        public FieldInfo propManager;
        public FieldInfo lightManager;
        public FieldInfo cameraManager;
        public FieldInfo effectManager;
        public FieldInfo maidIKPane;
        public FieldInfo maidDressingPane;
        public FieldInfo ikToggle;
        public FieldInfo releaseIKToggle;
        public FieldInfo boneIKToggle;
        public FieldInfo propList;
        public FieldInfo lightList;
        public FieldInfo subCamera;
        public FieldInfo dragPoints;

        private Dictionary<string, System.Type> _ownerTypes = new Dictionary<string, System.Type>
        {
            { "active", typeof(MPS) },
            { "meidoManager", typeof(MPS) },
            { "windowManager", typeof(MPS) },
            { "propManager", typeof(MPS) },
            { "lightManager", typeof(MPS) },
            { "cameraManager", typeof(MPS) },
            { "effectManager", typeof(MPS) },
            { "maidIKPane", typeof(PoseWindowPane) },
            { "maidDressingPane", typeof(PoseWindowPane) },
            { "ikToggle", typeof(MaidIKPane) },
            { "releaseIKToggle", typeof(MaidIKPane) },
            { "boneIKToggle", typeof(MaidIKPane) },
            { "propList", typeof(PropManager) },
            { "lightList", typeof(LightManager) },
            { "subCamera", typeof(CameraManager) },
            { "dragPoints", typeof(MeidoDragPointManager) },
        };

        public bool Init()
        {
            var bindingAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.InvokeMethod;

            foreach (var fieldInfo in typeof(MeidoPhotoStudioField).GetFields())
            {
                var fieldName = fieldInfo.Name;
                var parentType = _ownerTypes[fieldName];
                var targetField = parentType.GetField(fieldName, bindingAttr);
                PluginUtils.AssertNull(targetField != null, "field " + fieldName + " is null");
                fieldInfo.SetValue(this, targetField);

                if (targetField == null)
                {
                    return false;
                }
            }

            return true;
        }
    }
}