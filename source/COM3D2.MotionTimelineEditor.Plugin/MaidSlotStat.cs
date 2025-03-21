using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class MaidSlotStat : IModelStat
    {
        public string name { get; private set; }
        public string displayName { get; private set; }

        public TBodySkin bodySkin { get; private set; }
        public GameObject obj => bodySkin.obj;
        public Transform transform => bodySkin.obj_tr;

        public MPN mpn => bodySkin?.m_ParentMPN ?? MPN.null_mpn;
        public MaidProp prop => bodySkin.m_mp;

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

        public MaidSlotStat()
        {
        }

        public MaidSlotStat(TBodySkin bodySkin, string displayName)
        {
            this.bodySkin = bodySkin;
            this.name = bodySkin.Category;
            this.displayName = displayName;

            CreateControllers();
        }

        private void CreateControllers()
        {
            modelMaterialController = ModelMaterialController.GetOrCreate(this);
        }

        public ModelMaterial GetMaterial(int index)
        {
            if (modelMaterialController != null)
            {
                return modelMaterialController.GetMaterial(index);
            }
            return null;
        }
    }
}