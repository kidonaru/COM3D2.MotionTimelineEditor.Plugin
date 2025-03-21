using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{

    public class ModelMaterialController : MonoBehaviour
    {
        public IModelStat model;

        private Renderer _renderer = null;
        public Renderer renderer
        {
            get
            {
                if (_renderer == null)
                {
                    _renderer = GetComponentInChildren<Renderer>();
                }
                return _renderer;
            }
        }

        private List<ModelMaterial> _materials = new List<ModelMaterial>();
        public List<ModelMaterial> materials
        {
            get
            {
                if (renderer != null && renderer.sharedMaterials != null)
                {
                    var baseMaterials = renderer.sharedMaterials;
                    for (int i = 0; i < baseMaterials.Length; i++)
                    {
                        var baseMaterial = baseMaterials[i];
                        if (baseMaterial == null)
                        {
                            continue;
                        }

                        var material = i < _materials.Count ? _materials[i] : null;
                        if (material == null)
                        {
                            _materials.Add(new ModelMaterial(this, baseMaterial));
                        }
                        else if (material.material != baseMaterial)
                        {
                            material.UpdateMaterial(baseMaterial);
                        }
                    }

                    if (_materials.Count > baseMaterials.Length)
                    {
                        _materials.RemoveRange(baseMaterials.Length, _materials.Count - baseMaterials.Length);
                    }
                }

                return _materials;
            }
        }

        public static ModelMaterialController GetOrCreate(IModelStat model)
        {
            if (model == null || model.transform == null)
            {
                return null;
            }

            var transform = model.transform;
            var go = transform.gameObject;

            var controller = go.GetOrAddComponent<ModelMaterialController>();
            controller.model = model;
            return controller;
        }

        public ModelMaterial GetMaterial(int index)
        {
            if (index < 0 || index >= materials.Count)
            {
                return null;
            }

            return materials[index];
        }
    }
}