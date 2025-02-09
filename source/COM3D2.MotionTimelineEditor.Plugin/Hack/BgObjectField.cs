using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class BgObjectWrapper
    {
        public object original { get; set; }

        public string create_time { get; set; }
        public GameObject game_object { get; set; }
        public PhotoBGObjectData data { get; set; }

        public BgObjectWrapper()
        {
        }
    }

    public class BgObjectField : CustomFieldBase
    {
        public override System.Type assemblyType { get; set; } = typeof(CreateBGObjectSubWindow);
        public Type BgObjectType;

        public FieldInfo create_time;
        public FieldInfo game_object;
        public FieldInfo data;

        public override Dictionary<string, string> typeNames { get; } = new Dictionary<string, string>
        {
            { "BgObjectType", "CreateBGObjectSubWindow+BgObject" },
        };

        public override bool PrepareLoadFields()
        {
            defaultParentType = BgObjectType;
            return base.PrepareLoadFields();
        }

        public BgObjectWrapper ConvertToWrapper(object obj)
        {
            var wrapper = new BgObjectWrapper();
            wrapper.original = obj;

            wrapper.create_time = (string)create_time.GetValue(obj);
            wrapper.game_object = (GameObject)game_object.GetValue(obj);
            wrapper.data = (PhotoBGObjectData)data.GetValue(obj);

            return wrapper;
        }

        public object ConvertToOriginal(BgObjectWrapper wrapper)
        {
            object obj = Activator.CreateInstance(BgObjectType);
            create_time.SetValue(obj, wrapper.create_time);
            game_object.SetValue(obj, wrapper.game_object);
            data.SetValue(obj, wrapper.data);

            return obj;
        }
    }
}