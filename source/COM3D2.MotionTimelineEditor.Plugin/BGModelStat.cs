using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class BGModelStat
    {
        public string sourceName { get; private set; }
        public int group { get; private set; }
        public BGModelInfo info { get; private set; }

        public GameObject obj { get; private set; }
        public Transform transform { get; private set; }
        public string name { get; private set; }
        public string displayName { get; private set; }

        public GameObject sourceObj => info?.gameObject;

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

            if (sourceObj == null)
            {
                PluginUtils.Log($"背景モデルが見つかりません: {sourceName}");
                return;
            }

            if (group == 0)
            {
                this.obj = sourceObj;
                this.transform = sourceObj.transform;
            }
            else
            {
                this.obj = Object.Instantiate(sourceObj);
                this.transform = obj.transform;
                this.transform.SetParent(sourceObj.transform.parent, true);
                this.obj.name = name;
            }
        }

        public void InitName()
        {
            var groupSuffix = PluginUtils.GetGroupSuffix(group);
            this.name = sourceName + groupSuffix;
            this.displayName = BoneUtils.ConvertToBoneName(name);
        }

        public void Destroy()
        {
            if (obj != null)
            {
                if (obj == sourceObj)
                {
                    obj.SetActive(info.initialVisible);
                    obj.transform.localPosition = info.initialPosition;
                    obj.transform.localRotation = info.initialRotation;
                    obj.transform.localScale = info.initialScale; 
                }
                else
                {
                    Object.Destroy(obj);
                }
            }

            obj = null;
            transform = null;
        }
    }
}