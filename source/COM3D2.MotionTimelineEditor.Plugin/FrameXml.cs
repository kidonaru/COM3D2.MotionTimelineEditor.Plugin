using System.Collections.Generic;
using System.Xml.Serialization;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class FrameXml
    {
        [XmlElement("FrameNo")]
        public int frameNo = 0;

        [XmlElement("Bone")]
        public List<BoneXml> bones;

        public FrameXml()
        {
        }
    }
}