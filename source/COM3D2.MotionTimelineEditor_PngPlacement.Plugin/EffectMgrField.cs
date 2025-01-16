using System.Collections.Generic;
using System.Reflection;
using CM3D2.PngPlacement.Plugin;
using COM3D2.MotionTimelineEditor;
using System;

namespace COM3D2.MotionTimelineEditor_PngPlacement.Plugin
{
    public class EffectMgrField : CustomFieldBase
    {
        public override Type assemblyType { get; set; } = typeof(PngPlacement);

        public Type effectMgrType;

        public MethodInfo LoadAllSets;

        public override Dictionary<string, string> typeNames { get; } = new Dictionary<string, string>
        {
            { "effectMgrType", "CM3D2.PngPlacement.Plugin.PngPlacement+EffectMgr" },
        };

        public override bool PrepareLoadFields()
        {
            defaultParentType = effectMgrType;
            return base.PrepareLoadFields();
        }
    }
}