using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using COM3D2.MotionTimelineEditor;
using COM3D2.MotionTimelineEditor.Plugin;

namespace COM3D2.MotionTimelineEditor_PartsEdit.Plugin
{
    public class PartsEditField : CustomFieldBase
    {
        public override System.Type assemblyType { get; set; } = null;
 
        public Type yureUtilType = null;
        public Type settingType = null;
        public Type commonUIDataType = null;

        public FieldInfo boneDisplay = null;
        public FieldInfo gizmoType = null;
        public FieldInfo targetSelectMode = null;

        public MethodInfo GetYureAble = null;
        public MethodInfo GetYureState = null;
        public MethodInfo SetYureState = null;
        public MethodInfo SetMaid = null;
        public MethodInfo SetSlot = null;
        public MethodInfo SetObject = null;
        public MethodInfo SetBone = null;

        public override Dictionary<string, string> typeNames { get; } = new Dictionary<string, string>
        {
            { "yureUtilType", "YureUtil" },
            { "settingType", "CM3D2.PartsEdit.Plugin.Setting" },
            { "commonUIDataType", "CM3D2.PartsEdit.Plugin.CommonUIData" },
        };

        private readonly static string[] _assemblyFileNames = new string[]
        {
            "COM3D2.PartsEdit.Plugin.dll",
            "COM3D2.PartsEditWithStudio.Plugin.dll",
        };

        public override bool LoadAssembly()
        {
            foreach (var fileName in _assemblyFileNames)
            {
                var assemblyPath = Path.GetFullPath(MTEUtils.CombinePaths(
                    "Sybaris", "UnityInjector", fileName));
                if (File.Exists(assemblyPath))
                {
                    assembly = Assembly.LoadFile(assemblyPath);
                    break;
                }
            }

            MTEUtils.AssertNull(assembly != null, "PartsEdit.Plugin" + " not found");
            return assembly != null;
        }

        public override bool PrepareLoadFields()
        {
            parentTypes["boneDisplay"] = settingType;
            parentTypes["gizmoType"] = settingType;
            parentTypes["targetSelectMode"] = settingType;
            parentTypes["GetYureAble"] = yureUtilType;
            parentTypes["GetYureState"] = yureUtilType;
            parentTypes["SetYureState"] = yureUtilType;
            parentTypes["SetMaid"] = commonUIDataType;
            parentTypes["SetSlot"] = commonUIDataType;
            parentTypes["SetObject"] = commonUIDataType;
            parentTypes["SetBone"] = commonUIDataType;

            return base.PrepareLoadFields();
        }
    }
}