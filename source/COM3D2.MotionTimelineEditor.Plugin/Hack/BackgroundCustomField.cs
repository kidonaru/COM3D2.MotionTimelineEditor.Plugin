using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class BackgroundCustomField : CustomFieldBase
    {
        public override Type assemblyType { get; set; }

        public Type BackgroundCustomType;

        public MethodInfo createCategory;
        public MethodInfo createObjectCategory;

        public override Dictionary<string, string> typeNames { get; } = new Dictionary<string, string>
        {
            { "BackgroundCustomType", "COM3D2.BackgroundCustom.Plugin.BackgroundCustom" },
        };

        public override bool LoadAssembly()
        {
            var assemblyPath = Path.GetFullPath(MTEUtils.CombinePaths(
                "Sybaris", "UnityInjector", "COM3D2.BackgroundCustom.Plugin.dll"));
            if (File.Exists(assemblyPath))
            {
                assembly = Assembly.LoadFile(assemblyPath);
            }

            MTEUtils.AssertNull(assembly != null, "BackgroundCustom.Plugin" + " not found");
            return assembly != null;
        }

        public override bool PrepareLoadFields()
        {
            defaultParentType = BackgroundCustomType;
            return base.PrepareLoadFields();
        }
    }
}