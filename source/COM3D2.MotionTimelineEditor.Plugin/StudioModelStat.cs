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

    public class StudioModelStat
    {
        public OfficialObjectInfo info { get; private set; }
        public int group { get; private set; }
        public Transform transform { get; private set; }
        public string name { get; private set; }
        public string displayName { get; private set; }

        public StudioModelStat()
        {
        }

        public StudioModelStat(
            OfficialObjectInfo info,
            int group,
            Transform transform)
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
    }
}