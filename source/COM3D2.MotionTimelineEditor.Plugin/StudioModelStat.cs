using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    using AttachPoint = PhotoTransTargetObject.AttachPoint;

    public enum StudioModelType
    {
        Mod,
        Prefab,
        Asset,
        MyRoom,
    }

    public class StudioModelStat
    {
        public OfficialObjectInfo info { get; private set; }
        public int group { get; private set; }
        public string name { get; private set; }
        public string displayName { get; private set; }
        public AttachPoint attachPoint { get; set; }
        public int attachMaidSlotNo { get; set; }
        public object obj { get; set; }
        public string pluginName { get; set; }
        public bool visible { get; set; }

        public BlendShapeController blendShapeController { get; private set; }
        public ModelBoneController modelBoneController { get; private set; }
        public ModelMaterialController modelMaterialController { get; private set; }

        public List<ModelBone> bones
        {
            get
            {
                if (modelBoneController != null)
                {
                    return modelBoneController.bones;
                }
                return new List<ModelBone>();
            }
        }

        public List<ModelBlendShape> blendShapes
        {
            get
            {
                if (blendShapeController != null)
                {
                    return blendShapeController.blendShapes;
                }
                return new List<ModelBlendShape>();
            }
        }

        public List<ModelMaterial> materials
        {
            get
            {
                if (modelMaterialController != null)
                {
                    return modelMaterialController.materials;
                }
                return new List<ModelMaterial>();
            }
        }

        private Transform _transform;
        public Transform transform
        {
            get
            {
                return _transform;
            }
            set
            {
                if (_transform == value)
                {
                    return;
                }

                _transform = value;
                CreateControllers();
            }
        }

        public StudioModelStat()
        {
        }

        public StudioModelStat(
            OfficialObjectInfo info,
            int group,
            Transform transform,
            AttachPoint attachPoint,
            int attachMaidSlotNo,
            object obj,
            string pluginName,
            bool visible)
        {
            Init(
                info,
                group,
                transform,
                attachPoint,
                attachMaidSlotNo,
                obj,
                pluginName,
                visible);
        }

        public void Init(
            OfficialObjectInfo info,
            int group,
            Transform transform,
            AttachPoint attachPoint,
            int attachMaidSlotNo,
            object obj,
            string pluginName,
            bool visible)
        {
            this.info = info;
            this.group = group;
            this.attachPoint = attachPoint;
            this.attachMaidSlotNo = attachMaidSlotNo;
            this.transform = transform;
            this.obj = obj;
            this.pluginName = pluginName;
            this.visible = visible;

            InitName();
        }

        public void InitName()
        {
            var groupSuffix = PluginUtils.GetGroupSuffix(group);
            this.name = info.fileName + groupSuffix;
            this.displayName = info.label + groupSuffix;
        }

        public void SetGroup(int group)
        {
            this.group = group;
            InitName();
        }

        public string GetNameByGroup(int group)
        {
            var groupSuffix = PluginUtils.GetGroupSuffix(group);
            return info.fileName + groupSuffix;
        }

        public void FromModel(StudioModelStat model)
        {
            info = model.info;
            name = model.name;
            displayName = model.displayName;
            group = model.group;
            attachPoint = model.attachPoint;
            attachMaidSlotNo = model.attachMaidSlotNo;
            obj = model.obj;
            pluginName = model.pluginName;
            visible = model.visible;

            transform = model.transform;
        }

        public void FixBlendValues()
        {
            if (blendShapeController != null)
            {
                blendShapeController.FixBlendValues();
            }
        }

        private void CreateControllers()
        {
            if (transform == null)
            {
                return;
            }

            modelBoneController = ModelBoneController.GetOrCreate(this);
            blendShapeController = BlendShapeLoader.LoadController(this);
            modelMaterialController = ModelMaterialController.GetOrCreate(this);
        }

        public ModelBone GetBone(int index)
        {
            return modelBoneController.GetBone(index);
        }

        public ModelMaterial GetMaterial(int index)
        {
            return modelMaterialController.GetMaterial(index);
        }

        public void ReplaceInfo(OfficialObjectInfo info)
        {
            this.info = info;
            InitName();
        }
    }
}