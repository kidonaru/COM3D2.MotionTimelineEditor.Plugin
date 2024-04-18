using System.Xml.Serialization;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class BoneData
    {
        [XmlElement("Transform")]
        public TransformData transform;

        [XmlIgnore]
        public FrameData parentFrame = null;

        public string bonePath
        {
            get
            {
                return BoneUtils.ConvertBonePath(transform.name);
            }
        }

        public IKManager.BoneType boneType
        {
            get
            {
                return BoneUtils.GetBoneTypeByName(transform.name);
            }
        }

        public int frameNo
        {
            get
            {
                return parentFrame != null ? parentFrame.frameNo : -1;
            }
        }

        public BoneData()
        {
        }

        public BoneData(TransformData trans)
        {
            this.transform = trans;
        }

        public BoneData(Transform trans) : this(new TransformData(trans))
        {
        }

        /// <summary>
        /// 最短の補間経路に修正
        /// </summary>
        /// <param name="prevBone"></param>
        public void FixRotation(BoneData prevBone)
        {
            transform.FixRotation(prevBone.transform);
        }

        public BoneData DeepCopy()
        {
            var bone = new BoneData();
            bone.transform = transform.DeepCopy();
            return bone;
        }
    }
}