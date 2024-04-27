using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TimelineLayerXml
    {
        [XmlElement("ClassName")]
        public string className;

        [XmlElement("SlotNo")]
        public int slotNo;

        [XmlElement("Frame")]
        public List<FrameXml> keyFrames = new List<FrameXml>();

        public TimelineLayerXml()
        {
        }
    }
}