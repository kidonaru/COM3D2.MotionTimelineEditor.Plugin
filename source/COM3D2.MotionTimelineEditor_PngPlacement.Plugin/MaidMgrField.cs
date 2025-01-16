using System.Collections.Generic;
using System.Reflection;
using CM3D2.PngPlacement.Plugin;
using COM3D2.MotionTimelineEditor;
using System;

namespace COM3D2.MotionTimelineEditor_PngPlacement.Plugin
{
    public class MaidMgrField : CustomFieldBase
    {
        public override Type assemblyType { get; set; } = typeof(PngPlacement);

        public Type maidMgrType;

        public PropertyInfo bChangeState;

        public MethodInfo Find;
        public MethodInfo OnUpdateMaid;
        public MethodInfo RequestUpdate;

        public override Dictionary<string, string> typeNames { get; } = new Dictionary<string, string>
        {
            { "maidMgrType", "CM3D2.PngPlacement.Plugin.PngPlacement+MaidMgr" },
        };

        public override bool PrepareLoadFields()
        {
            defaultParentType = maidMgrType;
            return base.PrepareLoadFields();
        }
    }
}