using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class ExtendBoneCache : MonoBehaviour
    {
        public Maid maid;
        public Transform root;
        public List<string> slotNames = new List<string>();
        public HashSet<string> yureSlotNames = new HashSet<string>();

        public class Entity
        {
            public int slotNo;
            public string slotName;
            public string boneName;
            public string extendBoneName;
            public string bonePath;
            public Transform transform;
            public Vector3 initialPosition;
            public Quaternion initialRotation;
        }

        public Dictionary<string, Entity> entities = new Dictionary<string, Entity>(64);

        private static PartsEditHackManager partsEditHackManager
        {
            get
            {
                return PartsEditHackManager.instance;
            }
        }

        public ExtendBoneCache()
        {
        }

        public void Init(Maid maid, Transform root)
        {
            this.maid = maid;
            this.root = root;

            Refresh();
        }

        public void Refresh()
        {
            entities.Clear();
            slotNames.Clear();

            var slotNameHash = new HashSet<string>();

            var slotCount = maid.body0.goSlot.Count;
            for (var slotNo = 0; slotNo < slotCount; slotNo++)
            {
                var bodySkin = maid.body0.GetSlot(slotNo);
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
                        AddEntity(slotNo, slotName, boneName, bone);
                        slotNameHash.Add(slotName);
                    }
                }
            }

            slotNames = new List<string>(slotNameHash);

            foreach (var slotName in slotNames)
            {
                if (partsEditHackManager.GetYureAble(maid, slotName))
                {
                    yureSlotNames.Add(slotName);
                }
            }
        }

        public void AddEntity(int slotNo, string slotName, string boneName, Transform transform)
        {
            var extendBoneName = slotName + "/" + boneName;
            entities[extendBoneName] = new Entity
            {
                slotNo = slotNo,
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

        public bool IsYureSlot(string slotName)
        {
            return yureSlotNames.Contains(slotName);
        }
    }
}