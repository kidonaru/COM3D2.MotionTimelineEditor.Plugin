using System.Collections.Generic;
using System.Reflection;
using CM3D2.PngPlacement.Plugin;
using COM3D2.MotionTimelineEditor;
using UnityEngine;
using System.IO;
using System;

namespace COM3D2.MotionTimelineEditor_PngPlacement.Plugin
{
    public class PngPlacementField : CustomFieldBase
    {
        public override Type assemblyType { get; set; } = typeof(PngPlacement);

        public Type basicObjectType;

        public FieldInfo pm;
        public FieldInfo em;
        public FieldInfo im;
        public FieldInfo mm;
        public FieldInfo isShowNowSceen;

        public override Dictionary<string, string> typeNames { get; } = new Dictionary<string, string>
        {
            { "basicObjectType", "CM3D2.PngPlacement.Plugin.PngPlacement+BasicObject" },
        };

        public override Type defaultParentType { get; set; } = typeof(PngPlacement);

        public override bool PrepareLoadFields()
        {
            return base.PrepareLoadFields();
        }
    }
}