using System.Collections.Generic;
using System.Reflection;
using COM3D2.MotionTimelineEditor;
using UnityEngine;
using System.IO;
using System;

namespace COM3D2.MotionTimelineEditor_NPRShader.Plugin
{
    public class NPRShaderField : CustomFieldBase
    {
        public override Type assemblyType { get; set; } = typeof(NPRShader.Plugin.NPRShader);

        public Type EnvironmentWindowType;

        public FieldInfo envView;
        public FieldInfo bUpdateCubeMapRequest;
        public FieldInfo probe;

        public MethodInfo UpdateCubeMap;

        public override Type defaultParentType { get; set; } = typeof(NPRShader.Plugin.NPRShader);

        public override Dictionary<string, string> typeNames { get; } = new Dictionary<string, string>
        {
            { "EnvironmentWindowType", "COM3D2.NPRShader.Plugin.EnvironmentWindow" },
        };

        public override bool PrepareLoadFields()
        {
            parentTypes["bUpdateCubeMapRequest"] = EnvironmentWindowType;
            parentTypes["probe"] = EnvironmentWindowType;
            parentTypes["UpdateCubeMap"] = EnvironmentWindowType;
            return base.PrepareLoadFields();
        }
    }
}