using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class MaidInfo
    {
        private IKManager ikManager;

        public Vector3 initEyesPosL = Vector3.zero;
        public Vector3 initEyesPosR = Vector3.zero;
        public Vector3 initEyesScaL = Vector3.one;
        public Vector3 initEyesScaR = Vector3.one;
        private Dictionary<IKManager.BoneType, Vector3> _initialPositions = new Dictionary<IKManager.BoneType, Vector3>();

        public MaidInfo(Maid maid, IKManager ikManager)
        {
            if (maid == null || maid.body0 == null || maid.body0.trsEyeL == null || maid.body0.trsEyeR == null || ikManager == null)
            {
                PluginUtils.LogError("MaidInfo: MaidInfoの初期化に失敗しました。");
                return;
            }

            this.ikManager = ikManager;
            initEyesPosL = maid.body0.trsEyeL.localPosition;
            initEyesPosR = maid.body0.trsEyeR.localPosition;
            initEyesScaL = maid.body0.trsEyeL.localScale;
            initEyesScaR = maid.body0.trsEyeR.localScale;

            SaveInitialPosition();
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

        public Vector3 GetInitialPosition(IKManager.BoneType boneType)
        {
            Vector3 pos;
            if (_initialPositions.TryGetValue(boneType, out pos))
            {
                return pos;
            }
            return Vector3.zero;
        }

        public void SaveInitialPosition()
        {
            var boneTypes = new List<IKManager.BoneType>
            {
                IKManager.BoneType.UpperArm_L,
                IKManager.BoneType.Forearm_L,
                IKManager.BoneType.Thigh_L,
                IKManager.BoneType.Calf_L,
                IKManager.BoneType.UpperArm_R,
                IKManager.BoneType.Forearm_R,
                IKManager.BoneType.Thigh_R,
                IKManager.BoneType.Calf_R,
            };

            foreach (var boneType in boneTypes)
            {
                var bone = ikManager.GetBone(boneType);
                if (bone != null)
                {
                    _initialPositions[boneType] = bone.transform.localPosition;
                }
            }
        }
    }
}