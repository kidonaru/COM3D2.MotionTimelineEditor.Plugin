using System.Collections.Generic;
using System.Reflection;
using CM3D2.PngPlacement.Plugin;
using COM3D2.MotionTimelineEditor;
using System;

namespace COM3D2.MotionTimelineEditor_PngPlacement.Plugin
{
    public class ShaderMgrField : CustomFieldBase
    {
        public override Type assemblyType { get; set; } = typeof(PngPlacement);

        public Type shaderMgrType;

        public PropertyInfo displays;

        public MethodInfo GetName;

        public override Dictionary<string, string> typeNames { get; } = new Dictionary<string, string>
        {
            { "shaderMgrType", "CM3D2.PngPlacement.Plugin.PngPlacement+ShaderMgr" }, 
        };

        public override bool PrepareLoadFields()
        {
            defaultParentType = shaderMgrType;
            return base.PrepareLoadFields();
        }
    }
}