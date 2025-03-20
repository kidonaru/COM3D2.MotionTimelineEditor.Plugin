using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class MaidPropCache : MonoBehaviour
    {
        public Maid maid;
        public Dictionary<MaidPartType, string> initialPropNames = new Dictionary<MaidPartType, string>(64);

        public void Init(Maid maid)
        {
            this.maid = maid;
            UpdateInitialProp();
        }

        public void UpdateInitialProp()
        {
            initialPropNames.Clear();

            foreach (var maidPartType in MaidPartUtils.equippableMaidPartTypes)
            {
                var mpn = maidPartType.ToMPN();
                var prop = maid.GetProp(mpn);
                if (prop == null)
                {
                    continue;
                }

                initialPropNames[maidPartType] = prop.strFileName;
            }
        }

        public void ApplyInitialProp()
        {
            foreach (var pair in initialPropNames)
            {
                var maidPartType = pair.Key;
                var initialPropName = pair.Value;
                var mpn = maidPartType.ToMPN();
                var prop = maid.GetProp(mpn);

                if (prop != null && prop.strFileName != initialPropName)
                {
                    var rid = initialPropName.GetHashCode();
                    maid.SetProp(mpn, initialPropName, rid);
                }
            }

            maid.AllProcPropSeqStart();
        }

        public string GetInitialPropName(MaidPartType maidPartType)
        {
            return initialPropNames.GetOrDefault(maidPartType);
        }
    }
}