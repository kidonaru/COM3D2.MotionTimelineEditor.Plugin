using System;
using System.Collections.Generic;
using System.Reflection;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class StudioField : CustomFieldBase
    {
        public override Type assemblyType { get; set; } = typeof(PoseEditWindow);

        public FieldInfo data_dic_ = null;
        public FieldInfo bone_dic_ = null;
        public FieldInfo ikbox_visible_dic_ = null;
        public FieldInfo create_obj_list_ = null;

        public MethodInfo SetSelectMaid = null;
        public MethodInfo AddObject = null;
        public MethodInfo InstantiateLight = null;

        public override Dictionary<string, Type> parentTypes { get; } = new Dictionary<string, System.Type>
        {
            { "data_dic_", typeof(WindowPartsBoneCheckBox) },
            { "bone_dic_", typeof(IKManager) },
            { "ikbox_visible_dic_", typeof(PoseEditWindow) },
            { "create_obj_list_", typeof(CreateBGObjectSubWindow) },
            { "SetSelectMaid", typeof(PlacementWindow) },
            { "AddObject", typeof(CreateBGObjectSubWindow) },
            { "InstantiateLight", typeof(LightWindow) },
        };
    }
}