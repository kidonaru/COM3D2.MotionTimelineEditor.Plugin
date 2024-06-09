using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class ExtendBoneCache
    {
        public Maid maid;
        public Transform root;
        public List<string> slotNames;

        public class Entity
        {
            public string slotName;
            public string boneName;
            public string extendBoneName;
            public string bonePath;
            public Transform transform;
            public Vector3 initialPosition;
            public Quaternion initialRotation;
        }

        public Dictionary<string, Entity> entities = new Dictionary<string, Entity>(64);

        public ExtendBoneCache(Maid maid, Transform root)
        {
            this.maid = maid;
            this.root = root;

            Init();
        }

        public void Init()
        {
            entities.Clear();

            var slotNameHash = new HashSet<string>();

            foreach (var bodySkin in maid.body0.goSlot)
            {
                if (bodySkin == null || bodySkin.obj == null)
                {
                    continue;
                }

                var skinMesh = bodySkin.obj.GetComponentInChildren<SkinnedMeshRenderer>();
                if (skinMesh == null || skinMesh.bones == null)
                {
                    continue;
                }

                foreach (Transform bone in skinMesh.bones)
                {
                    if (bone == null)
                    {
                        continue;
                    }

                    var boneName = bone.name;
                    if (!BoneUtils.IsDefaultBoneName(boneName) &&
                        BoneUtils.IsVisibleBoneName(boneName))
                    {
                        var slotName = bodySkin.Category;
                        AddEntity(slotName, boneName, bone);
                        slotNameHash.Add(slotName);
                    }
                }
            }

            slotNames = new List<string>(slotNameHash);
        }

        public void AddEntity(string slotName, string boneName, Transform transform)
        {
            var extendBoneName = slotName + "/" + boneName;
            entities[extendBoneName] = new Entity
            {
                slotName = slotName,
                boneName = boneName,
                extendBoneName = extendBoneName,
                bonePath = transform.GetFullPath(this.root),
                transform = transform,
                initialPosition = transform.localPosition,
                initialRotation = transform.localRotation,
            };
        }

        public Entity GetEntity(string extendBoneName)
        {
            Entity entity;
            if (entities.TryGetValue(extendBoneName, out entity))
            {
                return entity;
            }
            return null;
        }
    }
}