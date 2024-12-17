using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class MaidFollowLight : MonoBehaviour
    {
        public Light light;
        public Transform targetTransform;
        public int maidSlotNo = -1;
        public Vector3 offset = Vector3.zero;

        protected static MaidManager maidManager => MaidManager.instance;

        public MaidCache maidCache
        {
            get
            {
                return maidManager.GetMaidCache(maidSlotNo);
            }
        }

        public Maid maid
        {
            get
            {
                if (maidCache != null)
                {
                    return maidCache.maid;
                }
                return null;
            }
        }

        private static StudioHackBase studioHack => StudioHackManager.studioHack;

        public bool isFollow
        {
            get
            {
                return maid != null;
            }
        }

        private void LateUpdate()
        {
            if (studioHack == null)
            {
                return;
            }

            if (isFollow && light != null && maid != null && targetTransform != null)
            {
                targetTransform.position = maid.body0.Pelvis.position + offset;
            }
        }
    }

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

        private MaidFollowLight _followLight = null;
        public MaidFollowLight followLight
        {
            get
            {
                if (_followLight == null)
                {
                    _followLight = transform.GetOrAddComponent<MaidFollowLight>();
                }

                _followLight.light = light;
                _followLight.targetTransform = transform;
                return _followLight;
            }
        }

        public Vector3 position
        {
            get
            {
                if (followLight.isFollow)
                {
                    return followLight.offset;
                }
                else
                {
                    return transform.localPosition;
                }
            }
            set
            {
                if (followLight.isFollow)
                {
                    followLight.offset = value;
                }
                else
                {
                    transform.localPosition = value;
                }
            }
        }

        public Vector3 eulerAngles
        {
            get
            {
                return transform.localEulerAngles;
            }
            set
            {
                transform.localEulerAngles = value;
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
            this.index = index;
            this.transform = transform;
            this.obj = obj;
        }

        public StudioLightStat(LightType type, bool visible, int index)
        {
            this.type = type;
            this.visible = visible;
            this.index = index;
        }

        public void InitName()
        {
            var groupSuffix = PluginUtils.GetGroupSuffix(index);
            this.name = "Light" + groupSuffix;
            this.displayName = LightTypeNames[type] + groupSuffix;
        }

        public void FromStat(StudioLightStat stat)
        {
            type = stat.type;
            visible = stat.visible;
            light = stat.light;
            transform = stat.transform;
            obj = stat.obj;
            index = stat.index;

            _followLight = null;
        }

        public StudioLightStat Clone()
        {
            var stat = new StudioLightStat();
            stat.FromStat(this);
            return stat;
        }
    }
}