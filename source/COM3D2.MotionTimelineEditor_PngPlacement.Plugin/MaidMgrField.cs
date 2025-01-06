using System.Collections.Generic;
using System.Reflection;
using CM3D2.PngPlacement.Plugin;
using System;

namespace COM3D2.MotionTimelineEditor_PngPlacement.Plugin
{
    public class MaidMgrField : CustomFieldBase
    {
        public override Type assemblyType { get; } = typeof(PngPlacement);

        public Type maidMgrType;

        public PropertyInfo bChangeState;

        public MethodInfo Find;
        public MethodInfo OnUpdateMaid;
        public MethodInfo RequestUpdate;

        public override Dictionary<string, string> typeNames { get; } = new Dictionary<string, string>
        {
            { "maidMgrType", "CM3D2.PngPlacement.Plugin.PngPlacement+MaidMgr" },
        };

        public override Type defaultParentType { get; set; } = null;

        public override Dictionary<string, Type> parentTypes { get; } = new Dictionary<string, System.Type>
        {
        };

        public override Dictionary<string, string> overrideFieldName { get; } = new Dictionary<string, string>
        {
        };

        public override Dictionary<string, Type[]> methodParameters { get; } = new Dictionary<string, System.Type[]>
        {
        };

        public override bool PrepareLoadFields()
        {
            defaultParentType = maidMgrType;
            return base.PrepareLoadFields();
        }
    }
}