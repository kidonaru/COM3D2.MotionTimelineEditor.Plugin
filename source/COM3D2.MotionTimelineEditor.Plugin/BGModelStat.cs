using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class BGModelStat : IModelStat
    {
        public string sourceName { get; private set; }
        public int group { get; private set; }
        public BGModelInfo info { get; private set; }

        public string name { get; private set; }
        public string displayName { get; private set; }

        public GameObject sourceObj => info?.gameObject;
        public GameObject obj => sourceObj;

        public Transform transform
        {
            get => (obj != null) ? obj.transform : null;
            set {}
        }

        public bool visible
        {
            get => obj != null && obj.activeSelf;
            set
            {
                if (value == visible)
                {
                    return;
                }
                if (obj != null)
                {
                    obj.SetActive(value);
                }
            }
        }

        public ModelMaterialController modelMaterialController { get; private set; }

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

        public BGModelStat()
        {
        }

        public BGModelStat(string sourceName, int group, BGModelInfo info)
        {
            Init(sourceName, group, info);
        }

        public void Init(string sourceName, int group, BGModelInfo info)
        {
            this.sourceName = sourceName;
            this.group = group;
            this.info = info;
            InitName();
            CreateControllers();

            if (sourceObj == null)
            {
                MTEUtils.LogDebug($"背景モデルが見つかりません: {sourceName}");
                return;
            }
        }

        public void InitName()
        {
            var groupSuffix = PluginUtils.GetGroupSuffix(group);
            this.name = sourceName + groupSuffix;
            this.displayName = BoneUtils.ConvertToBoneName(name);
        }

        private void CreateControllers()
        {
            if (transform == null)
            {
                return;
            }

            modelMaterialController = ModelMaterialController.GetOrCreate(this);
        }

        public void Destroy()
        {
            if (obj != null)
            {
                obj.SetActive(info.initialVisible);
                obj.transform.localPosition = info.initialPosition;
                obj.transform.localRotation = info.initialRotation;
                obj.transform.localScale = info.initialScale; 
            }
        }

        public ModelMaterial GetMaterial(int index)
        {
            return modelMaterialController.GetMaterial(index);
        }
    }
}