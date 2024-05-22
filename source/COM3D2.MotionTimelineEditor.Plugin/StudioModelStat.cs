using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public enum StudioModelType
    {
        Mod,
        Prefab,
        Asset,
        MyRoom,
    }

    public class StudioModelBoneStat
    {
        public StudioModelStat parentModel { get; private set; }
        public Transform transform { get; private set; }

        public string name
        {
            get
            {
                return string.Format("{0}/{1}", parentModel.name, transform.name);
            }
        }

        public StudioModelBoneStat()
        {
        }

        public StudioModelBoneStat(
            StudioModelStat parentModel,
            Transform transform)
        {
            this.parentModel = parentModel;
            this.transform = transform;
        }
    }

    public class StudioModelStat
    {
        public OfficialObjectInfo info { get; private set; }
        public int group { get; private set; }
        public string name { get; private set; }
        public string displayName { get; private set; }
        public List<StudioModelBoneStat> bones { get; private set; }

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
                BuildBones();
            }
        }

        public StudioModelStat()
        {
            bones = new List<StudioModelBoneStat>();
        }

        public StudioModelStat(
            OfficialObjectInfo info,
            int group,
            Transform transform) : this()
        {
            Init(info, group, transform);
        }

        public void Init(
            OfficialObjectInfo info,
            int group,
            Transform transform)
        {
            this.info = info;
            this.group = group;
            this.transform = transform;

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

        public void FromModel(StudioModelStat model)
        {
            group = model.group;
            transform = model.transform;
            info = model.info;
            name = model.name;
            displayName = model.displayName;
        }

        public void BuildBones()
        {
            bones.Clear();

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
                    var bone = new StudioModelBoneStat(this, boneTrans);
                    bones.Add(bone);
                }
            }
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