using UnityEngine;
using CM3D2.PngPlacement.Plugin;
using System.Collections.Generic;
using System.Reflection;
using System;
using COM3D2.MotionTimelineEditor.Plugin;

namespace COM3D2.MotionTimelineEditor_PngPlacement.Plugin
{
    public enum PngAttachPoint
    {
        none,
        body,
        head,
        handL,
        handR,
        footL,
        footR,
        muneL,
        muneR,
        body2,
        mata,
        ude1L,
        ude2L,
        ude1R,
        ude2R,
        leg1L,
        leg2L,
        leg1R,
        leg2R
    }

    public class LoadDataWrapper
    {
        public bool enable { get; set; }
        public string name { get; set; }
        public Vector3 pos { get; set; }
        public Vector3 rotation { get; set; }
        public bool inversion { get; set; }
        public bool stoprotation { get; set; }
        public Vector3 stoprotationv { get; set; }
        public float scale { get; set; }
        public int scalemag { get; set; }
        public string shader { get; set; }
        public int rq { get; set; }
        public bool fixcamera { get; set; }
        public Vector3 fixpos { get; set; }
        public PngAttachPoint attach { get; set; }
        public bool attachrotation { get; set; }
        public byte brightness { get; set; }
        public int primitive { get; set; }
        public float scalez { get; set; }
        public bool primitivereferencex { get; set; }
        public bool squareuv { get; set; }
        public int maid { get; set; }
        public Color32 color { get; set; }
        public float apngspeed { get; set; }
        public bool apngisfixedspeed { get; set; }

        public LoadDataWrapper()
        {
            this.enable = true;
            this.name = string.Empty;
            this.scale = 1f;
            this.scalemag = 1;
            this.rq = 3200;
            this.brightness = byte.MaxValue;
            this.scalez = 1f;
            this.primitivereferencex = true;
            this.color = UnityEngine.Color.white;
            this.shader = string.Empty;
            this.apngspeed = 1f;
        }

        public LoadDataWrapper(TimelinePngObjectData objectData) : this()
        {
            this.name = objectData.imageName;
            this.primitive = objectData.primitive;
            this.shader = objectData.GetShaderName();
        }
    }

    public class LoadDataField : CustomFieldBase
    {
        public override Type assemblyType { get; } = typeof(PngPlacement);

        public Type loadDataType;

        public PropertyInfo enable;
        public PropertyInfo name;
        public PropertyInfo pos;
        public PropertyInfo rotation;
        public PropertyInfo inversion;
        public PropertyInfo stoprotation;
        public PropertyInfo stoprotationv;
        public PropertyInfo scale;
        public PropertyInfo scalemag;
        public PropertyInfo shader;
        public PropertyInfo rq;
        public PropertyInfo fixcamera;
        public PropertyInfo fixpos;
        public PropertyInfo attach;
        public PropertyInfo attachrotation;
        public PropertyInfo brightness;
        public PropertyInfo primitive;
        public PropertyInfo scalez;
        public PropertyInfo primitivereferencex;
        public PropertyInfo squareuv;
        public PropertyInfo maid;
        public PropertyInfo color;
        public PropertyInfo apngspeed;
        public PropertyInfo apngisfixedspeed;

        public override Dictionary<string, string> typeNames { get; } = new Dictionary<string, string>
        {
            { "loadDataType", "CM3D2.PngPlacement.Plugin.PngPlacement+PlacementMgr+LoadData" },
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
            defaultParentType = loadDataType;
            return base.PrepareLoadFields();
        }

        public LoadDataWrapper ConvertToWrapper(object obj)
        {
            LoadDataWrapper wrapper = new LoadDataWrapper();
            wrapper.enable = (bool)enable.GetValue(obj, null);
            wrapper.name = (string)name.GetValue(obj, null);
            wrapper.pos = (Vector3)pos.GetValue(obj, null);
            wrapper.rotation = (Vector3)rotation.GetValue(obj, null);
            wrapper.inversion = (bool)inversion.GetValue(obj, null);
            wrapper.stoprotation = (bool)stoprotation.GetValue(obj, null);
            wrapper.stoprotationv = (Vector3)stoprotationv.GetValue(obj, null);
            wrapper.scale = (float)scale.GetValue(obj, null);
            wrapper.scalemag = (int)scalemag.GetValue(obj, null);
            wrapper.shader = (string)shader.GetValue(obj, null);
            wrapper.rq = (int)rq.GetValue(obj, null);
            wrapper.fixcamera = (bool)fixcamera.GetValue(obj, null);
            wrapper.fixpos = (Vector3)fixpos.GetValue(obj, null);
            wrapper.attach = (PngAttachPoint)attach.GetValue(obj, null);
            wrapper.attachrotation = (bool)attachrotation.GetValue(obj, null);
            wrapper.brightness = (byte)brightness.GetValue(obj, null);
            wrapper.primitive = (int)primitive.GetValue(obj, null);
            wrapper.scalez = (float)scalez.GetValue(obj, null);
            wrapper.primitivereferencex = (bool)primitivereferencex.GetValue(obj, null);
            wrapper.squareuv = (bool)squareuv.GetValue(obj, null);
            wrapper.maid = (int)maid.GetValue(obj, null);
            wrapper.color = (Color32)color.GetValue(obj, null);
            wrapper.apngspeed = (float)apngspeed.GetValue(obj, null);
            wrapper.apngisfixedspeed = (bool)apngisfixedspeed.GetValue(obj, null);

            return wrapper;
        }

        public object ConvertToOriginal(LoadDataWrapper wrapper)
        {
            object obj = Activator.CreateInstance(loadDataType);
            enable.SetValue(obj, wrapper.enable, null);
            name.SetValue(obj, wrapper.name, null);
            pos.SetValue(obj, wrapper.pos, null);
            rotation.SetValue(obj, wrapper.rotation, null);
            inversion.SetValue(obj, wrapper.inversion, null);
            stoprotation.SetValue(obj, wrapper.stoprotation, null);
            stoprotationv.SetValue(obj, wrapper.stoprotationv, null);
            scale.SetValue(obj, wrapper.scale, null);
            scalemag.SetValue(obj, wrapper.scalemag, null);
            shader.SetValue(obj, wrapper.shader, null);
            rq.SetValue(obj, wrapper.rq, null);
            fixcamera.SetValue(obj, wrapper.fixcamera, null);
            fixpos.SetValue(obj, wrapper.fixpos, null);
            attach.SetValue(obj, wrapper.attach, null);
            attachrotation.SetValue(obj, wrapper.attachrotation, null);
            brightness.SetValue(obj, wrapper.brightness, null);
            primitive.SetValue(obj, wrapper.primitive, null);
            scalez.SetValue(obj, wrapper.scalez, null);
            primitivereferencex.SetValue(obj, wrapper.primitivereferencex, null);
            squareuv.SetValue(obj, wrapper.squareuv, null);
            maid.SetValue(obj, wrapper.maid, null);
            color.SetValue(obj, wrapper.color, null);
            apngspeed.SetValue(obj, wrapper.apngspeed, null);
            apngisfixedspeed.SetValue(obj, wrapper.apngisfixedspeed, null);

            return obj;
        }
    }
}