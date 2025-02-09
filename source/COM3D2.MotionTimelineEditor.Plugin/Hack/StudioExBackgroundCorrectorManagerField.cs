using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class StudioExBackgroundCorrectorManagerField : CustomFieldBase
    {
        public override Type assemblyType { get; set; }

        public Type StudioExBackgroundCorrectorManagerType;

        public FieldInfo flgDoUpdateObjectIndexTable;

        public override Dictionary<string, string> typeNames { get; } = new Dictionary<string, string>
        {
            { "StudioExBackgroundCorrectorManagerType", "COM3D2.StudioExBackgroundCorrector.Managed.StudioExBackgroundCorrectorManager" },
        };

        public override bool LoadAssembly()
        {
            var assemblyPath = Path.GetFullPath(MTEUtils.CombinePaths(
                "Sybaris", "COM3D2.StudioExBackgroundCorrector.Managed.dll"));
            if (File.Exists(assemblyPath))
            {
                assembly = Assembly.LoadFile(assemblyPath);
            }

            //MTEUtils.AssertNull(assembly != null, "StudioExBackgroundCorrector.Managed.dll" + " not found");
            return assembly != null;
        }

        public override bool PrepareLoadFields()
        {
            defaultParentType = StudioExBackgroundCorrectorManagerType;
            return base.PrepareLoadFields();
        }
    }

    public class StudioExBackgroundCorrectorManagerWrapper
    {
        private StudioExBackgroundCorrectorManagerField _field = new StudioExBackgroundCorrectorManagerField();

        public bool initialized = false;

        public bool flgDoUpdateObjectIndexTable
        {
            get
            {
                if (IsValid())
                {
                    return (bool) _field.flgDoUpdateObjectIndexTable.GetValue(null);
                }

                return false;
            }
            set
            {
                if (IsValid())
                {
                    _field.flgDoUpdateObjectIndexTable.SetValue(null, value);
                }
            }
        }

        public bool Init()
        {
            if (!_field.Init())
            {
                return false;
            }

            initialized = true;
            return true;
        }

        public bool IsValid()
        {
            return initialized;
        }
    }
}