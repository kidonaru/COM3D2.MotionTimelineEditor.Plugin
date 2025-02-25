using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class ModelBone
    {
        public ModelBoneController controller;
        public Transform transform;
        public Vector3 initialPosition;
        public Quaternion initialRotation;
        public Vector3 initialScale;

        public StudioModelStat model => controller.model;

        public string name
        {
            get => string.Format("{0}/{1}", model.name, transform != null ? transform.name : "");
        }

        public Vector3 initialEulerAngles
        {
            get => initialRotation.eulerAngles;
        }
    }

    public class ModelBoneController : MonoBehaviour
    {
        public StudioModelStat model;

        private SkinnedMeshRenderer _meshRenderer = null;
        public SkinnedMeshRenderer meshRenderer
        {
            get
            {
                if (_meshRenderer == null)
                {
                    _meshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
                }
                return _meshRenderer;
            }
        }

        public List<ModelBone> bones = new List<ModelBone>();

        public static ModelBoneController GetOrCreate(StudioModelStat model)
        {
            var transform = model.transform;
            var go = transform.gameObject;

            var controller = go.GetComponent<ModelBoneController>();
            if (controller == null)
            {
                controller = go.AddComponent<ModelBoneController>();
                controller.Init();
            }

            controller.model = model;
            return controller;
        }

        public void Init()
        {
            bones.Clear();

            if (meshRenderer == null || meshRenderer.bones == null)
            {
                return;
            }

            foreach (var bone in meshRenderer.bones)
            {
                bones.Add(new ModelBone
                {
                    controller = this,
                    transform = bone,
                    initialPosition = bone.localPosition,
                    initialRotation = bone.localRotation,
                    initialScale = bone.localScale,
                });
            }
        }

        public ModelBone GetBone(int index)
        {
            if (index < 0 || index >= bones.Count)
            {
                return null;
            }

            return bones[index];
        }
    }
}