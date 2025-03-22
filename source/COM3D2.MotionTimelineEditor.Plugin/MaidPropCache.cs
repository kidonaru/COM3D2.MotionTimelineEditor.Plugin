using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class MaidPropInfo
    {
        public MPN mpn;
        public string propName;
        public int rid;

        public MaidPropInfo(MPN mpn, string propName, int rid)
        {
            this.mpn = mpn;
            this.propName = propName;
            this.rid = rid;
        }
    }

    public class MaidPropCache : MonoBehaviour
    {
        public Maid maid;
        public Dictionary<MaidPartType, MaidPropInfo> initialPropInfoMap = new Dictionary<MaidPartType, MaidPropInfo>(64);
        public readonly MaidPropInfo defaultPropInfo = new MaidPropInfo(MPN.null_mpn, "", 0);

        public void Init(Maid maid)
        {
            this.maid = maid;
            UpdateInitialProp();
        }

        public void UpdateInitialProp()
        {
            foreach (var maidPartType in MaidPartUtils.equippableMaidPartTypes)
            {
                var mpn = maidPartType.ToMPN();
                var prop = maid.GetProp(mpn);
                if (prop == null)
                {
                    continue;
                }

                if (initialPropInfoMap.TryGetValue(maidPartType, out var initialPropInfo))
                {
                    initialPropInfo.mpn = mpn;
                    initialPropInfo.propName = prop.strFileName;
                    initialPropInfo.rid = prop.nFileNameRID;
                }
                else
                {
                    initialPropInfoMap[maidPartType] = new MaidPropInfo(mpn, prop.strFileName, prop.nFileNameRID);
                }
            }
        }

        public void ApplyInitialProp()
        {
            foreach (var info in initialPropInfoMap.Values)
            {
                var mpn = info.mpn;
                var prop = maid.GetProp(mpn);

                if (prop != null && prop.strFileName != info.propName)
                {
                    maid.SetProp(mpn, info.propName, info.rid);
                }
            }

            maid.AllProcPropSeqStart();
        }

        public MaidPropInfo GetInitialPropInfo(MaidPartType maidPartType)
        {
            return initialPropInfoMap.GetOrDefault(maidPartType, defaultPropInfo);
        }
    }
}