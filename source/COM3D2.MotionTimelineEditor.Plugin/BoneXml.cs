using System.Xml.Serialization;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class BoneXml
    {
        [XmlElement("Transform")]
        public TransformXml transform;

        public BoneXml()
        {
        }
    }
}