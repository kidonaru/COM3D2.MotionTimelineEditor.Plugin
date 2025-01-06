using System.Collections.Generic;
using System.Reflection;
using CM3D2.PngPlacement.Plugin;
using System;

namespace COM3D2.MotionTimelineEditor_PngPlacement.Plugin
{
    public class PlacementMgrField : CustomFieldBase
    {
        public override Type assemblyType { get; } = typeof(PngPlacement);

        public Type placementMgrType;
        public Type loadDataType;
        public Type pngObjectDataType;
        public Type attachPointType;

        public PropertyInfo iCurrentObject;
        public PropertyInfo listObjectData;
        public PropertyInfo iPrimitive;
        public PropertyInfo sShaderDispName;

        public MethodInfo Create;
        public MethodInfo CreateByLoadData;
        public MethodInfo DeleteAll;
        public MethodInfo LoadAllSets;

        public MethodInfo SetEnable;
        public MethodInfo SetAPNGSpeed;
        public MethodInfo SetAPNGIsFixedSpeed;
        public MethodInfo SetScale;
        public MethodInfo SetScaleMag;
        public MethodInfo SetScaleZ;
        public MethodInfo SetRotation;
        public MethodInfo SetStopRotation;
        public MethodInfo SetFixedCamera;
        public MethodInfo SetInversion;
        public MethodInfo SetBrightness;
        public MethodInfo SetColor;
        public MethodInfo SetShaderName;
        public MethodInfo SetRenderQueue;
        public MethodInfo SetAttachPoint;
        public MethodInfo SetAttachRotation;

        public override Dictionary<string, string> typeNames { get; } = new Dictionary<string, string>
        {
            { "placementMgrType", "CM3D2.PngPlacement.Plugin.PngPlacement+PlacementMgr" },
            { "loadDataType", "CM3D2.PngPlacement.Plugin.PngPlacement+PlacementMgr+LoadData" },
            { "pngObjectDataType", "CM3D2.PngPlacement.Plugin.PngPlacement+PlacementMgr+PngObjectData" },
            { "attachPointType", "CM3D2.PngPlacement.Plugin.PngPlacement+AttachPoint" },
        };

        public override Type defaultParentType { get; set; } = null;

        public override Dictionary<string, Type> parentTypes { get; } = new Dictionary<string, System.Type>
        {
        };

        public override Dictionary<string, string> overrideFieldName { get; } = new Dictionary<string, string>
        {
            { "CreateByLoadData", "Create" },
        };

        public override Dictionary<string, Type[]> methodParameters { get; } = new Dictionary<string, System.Type[]>
        {
            { "SetAPNGSpeed", new Type[] { typeof(int), typeof(float) } },
            { "SetAPNGIsFixedSpeed", new Type[] { typeof(int), typeof(bool) } },
            { "SetAttachPoint", new Type[] { typeof(int), typeof(int) } }
        };

        public override bool PrepareLoadFields()
        {
            defaultParentType = placementMgrType;
            methodParameters["Create"] = new Type[] {};
            methodParameters["CreateByLoadData"] = new Type[] { loadDataType };
            return base.PrepareLoadFields();
        }
    }
}