using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class StudioLightStat
    {
        private LightType _type = LightType.Directional;
        public LightType type
        {
            get
            {
                return _type;
            }
            set
            {
                if (_type == value)
                {
                    return;
                }

                _type = value;
                InitName();
            }
        }

        public int maidSlotNo = -1;
        public bool visible = true;
        public Light light = null;
        public Transform transform = null;
        public object obj = null;
        private int _index = 0;
        public int index
        {
            get
            {
                return _index;
            }
            set
            {
                if (_index == value)
                {
                    return;
                }

                _index = value;
                InitName();
            }
        }

        public string name = "Light";
        public string displayName = "通常";

        public int typeOrder
        {
            get
            {
                switch (type)
                {
                    case LightType.Directional:
                        return 0;
                    case LightType.Spot:
                        return 1;
                    case LightType.Point:
                        return 2;
                    default:
                        return 3;
                }
            }
        }

        public static readonly Vector3 DefaultPosition = new Vector3(0f, 1.9f, 0.4f);

		public static readonly Vector3 DefaultRotation = new Vector3(40f, 180f, 0f);

        public static readonly Dictionary<LightType, string> LightTypeNames = new Dictionary<LightType, string>
        {
            { LightType.Directional, "通常" },
            { LightType.Point, "ポイント" },
            { LightType.Spot, "スポット" },
            { LightType.Area, "エリア" },
        };

        public StudioLightStat()
        {
        }

        public StudioLightStat(Light light, Transform transform, object obj, int index)
        {
            this.type = light.type;
            this.visible = light.enabled;
            this.light = light;
            this.transform = transform;
            this.obj = obj;
            this.index = index;
        }

        public void InitName()
        {
            var groupSuffix = StudioModelManager.GetGroupSuffix(index);
            this.name = "Light" + groupSuffix;
            this.displayName = LightTypeNames[type] + groupSuffix;
        }

        public string GetNameByGroup(int group)
        {
            var groupSuffix = StudioModelManager.GetGroupSuffix(group);
            return "Light" + groupSuffix;
        }

        public void FromStat(StudioLightStat stat)
        {
            type = stat.type;
            maidSlotNo = stat.maidSlotNo;
            visible = stat.visible;
            light = stat.light;
            transform = stat.transform;
            obj = stat.obj;
            index = stat.index;
        }
    }
}