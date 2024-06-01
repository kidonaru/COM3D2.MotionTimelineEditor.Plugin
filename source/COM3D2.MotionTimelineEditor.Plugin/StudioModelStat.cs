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

    public class StudioModelBone
    {
        public StudioModelStat parentModel { get; private set; }
        public Transform transform { get; private set; }

        public string name
        {
            get
            {
                return string.Format("{0}/{1}", parentModel.name, transform != null ? transform.name : "");
            }
        }

        public StudioModelBone()
        {
        }

        public StudioModelBone(
            StudioModelStat parentModel,
            Transform transform)
        {
            this.parentModel = parentModel;
            this.transform = transform;
        }
    }

    public class StudioModelBlendShape
    {
        public StudioModelStat parentModel { get; private set; }
        public string shapeKeyName { get; private set; }
        public int shapeKeyIndex { get; set; }

        public BlendShapeController blendShapeController
        {
            get
            {
                return parentModel.blendShapeController;
            }
        }

        public string name
        {
            get
            {
                return string.Format("{0}/{1}", parentModel.name, shapeKeyName);
            }
        }

        public float weight
        {
            get
            {
                if (shapeKeyIndex >= 0 && shapeKeyIndex < blendShapeController.blendShapeCount)
                {
                    return blendShapeController.GetBlendShapeWeight(shapeKeyIndex);
                }
                PluginUtils.LogWarning("BlendShapeIndex out of range: {0}", shapeKeyIndex);
                return 0f;
            }
            set
            {
                if (shapeKeyIndex >= 0 && shapeKeyIndex < blendShapeController.blendShapeCount)
                {
                    blendShapeController.SetBlendShapeWeight(shapeKeyIndex, value);
                    return;
                }
                PluginUtils.LogWarning("BlendShapeIndex out of range: {0}", shapeKeyIndex);
            }
        }

        public StudioModelBlendShape()
        {
        }

        public StudioModelBlendShape(
            StudioModelStat parentModel,
            string shapeKeyName)
        {
            this.parentModel = parentModel;
            this.shapeKeyName = shapeKeyName;
            this.shapeKeyIndex = blendShapeController.GetBlendShapeIndex(shapeKeyName);
        }
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
        public List<StudioModelBone> bones { get; private set; }
        public List<StudioModelBlendShape> blendShapes { get; private set; }
        public BlendShapeController blendShapeController { get; private set; }

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
                BuildBonesAndBlendShapes();
            }
        }

        public StudioModelStat()
        {
            bones = new List<StudioModelBone>();
            blendShapes = new List<StudioModelBlendShape>();
        }

        public StudioModelStat(
            OfficialObjectInfo info,
            int group,
            Transform transform,
            AttachPoint attachPoint,
            int attachMaidSlotNo,
            object obj,
            string pluginName) : this()
        {
            Init(info, group, transform, attachPoint, attachMaidSlotNo, obj, pluginName);
        }

        public void Init(
            OfficialObjectInfo info,
            int group,
            Transform transform,
            AttachPoint attachPoint,
            int attachMaidSlotNo,
            object obj,
            string pluginName)
        {
            this.info = info;
            this.group = group;
            this.attachPoint = attachPoint;
            this.attachMaidSlotNo = attachMaidSlotNo;
            this.transform = transform;
            this.obj = obj;
            this.pluginName = pluginName;

            InitName();
        }

        public void InitName()
        {
            var groupSuffix = StudioModelManager.GetGroupSuffix(group);
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
            var groupSuffix = StudioModelManager.GetGroupSuffix(group);
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

            transform = model.transform;
        }

        public void FixBlendValues()
        {
            if (blendShapeController != null)
            {
                blendShapeController.FixBlendValues();
            }
        }

        private void BuildBonesAndBlendShapes()
        {
            bones.Clear();
            blendShapes.Clear();

            if (transform == null)
            {
                return;
            }

            var meshRenderer = transform.GetComponentInChildren<SkinnedMeshRenderer>();
            if (meshRenderer == null)
            {
                return;
            }

            foreach (var boneTrans in meshRenderer.bones)
            {
                if (IsVisibleBone(boneTrans))
                {
                    var bone = new StudioModelBone(this, boneTrans);
                    bones.Add(bone);
                }
            }

            blendShapeController = BlendShapeLoader.LoadController(this);
            if (blendShapeController == null)
            {
                return;
            }

            var blendShapeCount = blendShapeController.blendShapeCount;
            for (int i = 0; i < blendShapeCount; i++)
            {
                var shapeKeyName = blendShapeController.GetBlendShapeName(i);
                var blendShape = new StudioModelBlendShape(this, shapeKeyName);
                blendShapes.Add(blendShape);
            }
        }

        public StudioModelBone GetBone(int index)
        {
            if (index < 0 || index >= bones.Count)
            {
                return null;
            }

            return bones[index];
        }

        private bool IsVisibleBone(Transform trans)
        {
            if (trans == null)
            {
                return false;
            }

            return !trans.name.EndsWith("_nub") && !trans.name.EndsWith("_SCL_");
        }
    }
}