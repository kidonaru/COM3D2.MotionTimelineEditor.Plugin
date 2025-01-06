using UnityEngine;
using CM3D2.PngPlacement.Plugin;
using System.Collections.Generic;
using System.Reflection;
using System;
using COM3D2.MotionTimelineEditor.Plugin;

namespace COM3D2.MotionTimelineEditor_PngPlacement.Plugin
{
    public class BasicObjectWrapper
    {
        public GameObject parentObject { get; set; }
        public object dragMgr { get; set; }
        public bool enable { get; set; }
        public int maid { get; set; }
        public Texture2D texThumb_ { get; set; }
        public string guid { get; set; }

        public BasicObjectWrapper()
        {
            this.guid = Guid.NewGuid().ToString("N");
        }
    }

    public class PngObjectDataWrapper : BasicObjectWrapper
    {
        public int index { get; set; }
        public int group { get; private set; }
        public string displayName { get; private set; }
        public object original { get; set; }

        public GameObject pngObject { get; set; }
        public Material pngMaterial { get; set; }
        public PngPlacement.PNG.APNG apng { get; set; }
        public PngPlacement.PNG.APNGAnmData apngAnm { get; set; }
        public Vector3 rotation { get; set; }
        public bool inversion { get; set; }
        public bool stopRotation { get; set; }
        public Vector3 stopRotationVector { get; set; }
        public float scale { get; set; }
        public int scaleMag { get; set; }
        public string shaderDisplay { get; set; }
        public int renderQueue { get; set; }
        public bool fixedCamera { get; set; }
        public Vector3 fixedPos { get; set; }
        public PngAttachPoint attach { get; set; }
        public bool attachRotation { get; set; }
        public byte brightness { get; set; }
        public Color32 color { get; set; }
        public int primitive { get; set; }
        public float scaleZ { get; set; }
        public bool primitiveReferenceX { get; set; }
        public bool squareUV { get; set; }

        public Transform transform
        {
            get
            {
                if (parentObject != null)
                {
                    return parentObject.transform;
                }
                return null;
            }
        }

        public string imageName
        {
            get
            {
                if (parentObject != null)
                {
                    return parentObject.name;
                }
                return string.Empty;
            }
        }

        public PngObjectDataWrapper()
        {
            this.shaderDisplay = string.Empty;
            base.guid = Guid.NewGuid().ToString("N");
        }

        public void SetGroup(int group)
        {
            this.group = group;
            UpdateName();
        }

        public void UpdateName()
        {
            var groupSuffix = PluginUtils.GetGroupSuffix(group);
            this.displayName = imageName + groupSuffix;
        }

        public void CopyFrom(PngObjectDataWrapper source)
        {
            this.index = source.index;
            this.group = source.group;
            this.displayName = source.displayName;
            this.original = source.original;

            this.parentObject = source.parentObject;
            this.dragMgr = source.dragMgr;
            this.enable = source.enable;
            this.maid = source.maid;
            this.texThumb_ = source.texThumb_;
            this.guid = source.guid;

            this.pngObject = source.pngObject;
            this.pngMaterial = source.pngMaterial;
            this.apng = source.apng;
            this.apngAnm = source.apngAnm;
            this.rotation = source.rotation;
            this.inversion = source.inversion;
            this.stopRotation = source.stopRotation;
            this.stopRotationVector = source.stopRotationVector;
            this.scale = source.scale;
            this.scaleMag = source.scaleMag;
            this.shaderDisplay = source.shaderDisplay;
            this.renderQueue = source.renderQueue;
            this.fixedCamera = source.fixedCamera;
            this.fixedPos = source.fixedPos;
            this.attach = source.attach;
            this.attachRotation = source.attachRotation;
            this.brightness = source.brightness;
            this.color = source.color;
            this.primitive = source.primitive;
            this.scaleZ = source.scaleZ;
            this.primitiveReferenceX = source.primitiveReferenceX;
            this.squareUV = source.squareUV;
        }
    }

    public class PngObjectDataField : CustomFieldBase
    {
        public override Type assemblyType { get; } = typeof(PngPlacement);

        public Type pngObjectDataType;

        // BasicObject
        public PropertyInfo parentObject;
        public PropertyInfo dragMgr;
        public PropertyInfo enable;
        public PropertyInfo maid;
        public PropertyInfo texThumb_;
        public PropertyInfo guid;

        // PngObjectData
        public PropertyInfo pngObject;
        public PropertyInfo pngMaterial;
        public PropertyInfo apng;
        public PropertyInfo apngAnm;
        public PropertyInfo rotation;
        public PropertyInfo inversion;
        public PropertyInfo stopRotation;
        public PropertyInfo stopRotationVector;
        public PropertyInfo scale;
        public PropertyInfo scaleMag;
        public PropertyInfo shaderDisplay;
        public PropertyInfo renderQueue;
        public PropertyInfo fixedCamera;
        public PropertyInfo fixedPos;
        public PropertyInfo attach;
        public PropertyInfo attachRotation;
        public PropertyInfo brightness;
        public PropertyInfo color;
        public PropertyInfo primitive;
        public PropertyInfo scaleZ;
        public PropertyInfo primitiveReferenceX;
        public PropertyInfo squareUV;

        public override Dictionary<string, string> typeNames { get; } = new Dictionary<string, string>
        {
            { "pngObjectDataType", "CM3D2.PngPlacement.Plugin.PngPlacement+PlacementMgr+PngObjectData" },
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
            defaultParentType = pngObjectDataType;
            return base.PrepareLoadFields();
        }

        public PngObjectDataWrapper ConvertToWrapper(object obj, int index)
        {
            var wrapper = new PngObjectDataWrapper();
            wrapper.index = index;
            wrapper.original = obj;

            wrapper.parentObject = (GameObject)parentObject.GetValue(obj, null);
            wrapper.dragMgr = dragMgr.GetValue(obj, null);
            wrapper.enable = (bool)enable.GetValue(obj, null);
            wrapper.maid = (int)maid.GetValue(obj, null);
            wrapper.texThumb_ = (Texture2D)texThumb_.GetValue(obj, null);
            wrapper.guid = (string)guid.GetValue(obj, null);

            wrapper.pngObject = (GameObject)pngObject.GetValue(obj, null);
            wrapper.pngMaterial = (Material)pngMaterial.GetValue(obj, null);
            wrapper.apng = (PngPlacement.PNG.APNG)apng.GetValue(obj, null);
            wrapper.apngAnm = (PngPlacement.PNG.APNGAnmData)apngAnm.GetValue(obj, null);
            wrapper.rotation = (Vector3)rotation.GetValue(obj, null);
            wrapper.inversion = (bool)inversion.GetValue(obj, null);
            wrapper.stopRotation = (bool)stopRotation.GetValue(obj, null);
            wrapper.stopRotationVector = (Vector3)stopRotationVector.GetValue(obj, null);
            wrapper.scale = (float)scale.GetValue(obj, null);
            wrapper.scaleMag = (int)scaleMag.GetValue(obj, null);
            wrapper.shaderDisplay = (string)shaderDisplay.GetValue(obj, null);
            wrapper.renderQueue = (int)renderQueue.GetValue(obj, null);
            wrapper.fixedCamera = (bool)fixedCamera.GetValue(obj, null);
            wrapper.fixedPos = (Vector3)fixedPos.GetValue(obj, null);
            wrapper.attach = (PngAttachPoint)attach.GetValue(obj, null);
            wrapper.attachRotation = (bool)attachRotation.GetValue(obj, null);
            wrapper.brightness = (byte)brightness.GetValue(obj, null);
            wrapper.color = (Color32)color.GetValue(obj, null);
            wrapper.primitive = (int)primitive.GetValue(obj, null);
            wrapper.scaleZ = (float)scaleZ.GetValue(obj, null);
            wrapper.primitiveReferenceX = (bool)primitiveReferenceX.GetValue(obj, null);
            wrapper.squareUV = (bool)squareUV.GetValue(obj, null);

            return wrapper;
        }

        public object ConvertToOriginal(PngObjectDataWrapper wrapper)
        {
            object obj = Activator.CreateInstance(pngObjectDataType);
            parentObject.SetValue(obj, wrapper.parentObject, null);
            dragMgr.SetValue(obj, wrapper.dragMgr, null);
            enable.SetValue(obj, wrapper.enable, null);
            maid.SetValue(obj, wrapper.maid, null);
            texThumb_.SetValue(obj, wrapper.texThumb_, null);
            guid.SetValue(obj, wrapper.guid, null);

            pngObject.SetValue(obj, wrapper.pngObject, null);
            pngMaterial.SetValue(obj, wrapper.pngMaterial, null);
            apng.SetValue(obj, wrapper.apng, null);
            apngAnm.SetValue(obj, wrapper.apngAnm, null);
            rotation.SetValue(obj, wrapper.rotation, null);
            inversion.SetValue(obj, wrapper.inversion, null);
            stopRotation.SetValue(obj, wrapper.stopRotation, null);
            stopRotationVector.SetValue(obj, wrapper.stopRotationVector, null);
            scale.SetValue(obj, wrapper.scale, null);
            scaleMag.SetValue(obj, wrapper.scaleMag, null);
            shaderDisplay.SetValue(obj, wrapper.shaderDisplay, null);
            renderQueue.SetValue(obj, wrapper.renderQueue, null);
            fixedCamera.SetValue(obj, wrapper.fixedCamera, null);
            fixedPos.SetValue(obj, wrapper.fixedPos, null);
            attach.SetValue(obj, wrapper.attach, null);
            attachRotation.SetValue(obj, wrapper.attachRotation, null);
            brightness.SetValue(obj, wrapper.brightness, null);
            color.SetValue(obj, wrapper.color, null);
            primitive.SetValue(obj, wrapper.primitive, null);
            scaleZ.SetValue(obj, wrapper.scaleZ, null);
            primitiveReferenceX.SetValue(obj, wrapper.primitiveReferenceX, null);
            squareUV.SetValue(obj, wrapper.squareUV, null);

            return obj;
        }
    }
}