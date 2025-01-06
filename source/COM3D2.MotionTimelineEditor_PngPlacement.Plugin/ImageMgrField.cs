using System.Collections.Generic;
using System.Reflection;
using CM3D2.PngPlacement.Plugin;
using System;

namespace COM3D2.MotionTimelineEditor_PngPlacement.Plugin
{
    public class ImageMgrField : CustomFieldBase
    {
        public override Type assemblyType { get; } = typeof(PngPlacement);

        public Type imageMgrType;

        public PropertyInfo iPngSel;
        public PropertyInfo listImage;
        public PropertyInfo listImageFilename;
        public MethodInfo _Init;

        public override Dictionary<string, string> typeNames { get; } = new Dictionary<string, string>
        {
            { "imageMgrType", "CM3D2.PngPlacement.Plugin.PngPlacement+ImageMgr" },
        };

        public override Type defaultParentType { get; set; } = null;

        public override Dictionary<string, Type> parentTypes { get; } = new Dictionary<string, System.Type>
        {
        };

        public override Dictionary<string, string> overrideFieldName { get; } = new Dictionary<string, string>
        {
            { "_Init", "Init" },
        };

        public override Dictionary<string, Type[]> methodParameters { get; } = new Dictionary<string, System.Type[]>
        {
            { "_Init", new Type[] {} },
        };

        public override bool PrepareLoadFields()
        {
            defaultParentType = imageMgrType;
            return base.PrepareLoadFields();
        }
    }
}