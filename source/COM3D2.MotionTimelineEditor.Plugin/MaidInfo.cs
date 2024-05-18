using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class MaidInfo
    {
        public Vector3 initEyesPosL = Vector3.zero;
        public Vector3 initEyesPosR = Vector3.zero;
        public Vector3 initEyesScaL = Vector3.one;
        public Vector3 initEyesScaR = Vector3.one;

        public MaidInfo(Maid maid, IKManager ikManager)
        {
            if (maid == null || maid.body0 == null || maid.body0.trsEyeL == null || maid.body0.trsEyeR == null)
            {
                PluginUtils.LogError("MaidInfo: MaidInfoの初期化に失敗しました。");
                return;
            }

            initEyesPosL = maid.body0.trsEyeL.localPosition;
            initEyesPosR = maid.body0.trsEyeR.localPosition;
            initEyesScaL = maid.body0.trsEyeL.localScale;
            initEyesScaR = maid.body0.trsEyeR.localScale;
        }

        private static Dictionary<Maid, MaidInfo> _maidInfoMap = new Dictionary<Maid, MaidInfo>();

        public static MaidInfo GetOrCreate(Maid maid, IKManager ikManager)
        {
            if (maid == null)
            {
                return null;
            }

            MaidInfo info;
            if (_maidInfoMap.TryGetValue(maid, out info))
            {
                return info;
            }

            info = new MaidInfo(maid, ikManager);

            _maidInfoMap[maid] = info;
            return info;
        }

        public static void Reset()
        {
            _maidInfoMap.Clear();
        }
    }
}