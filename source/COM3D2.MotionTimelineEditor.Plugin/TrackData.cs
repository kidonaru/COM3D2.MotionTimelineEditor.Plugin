using System.Xml.Serialization;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TrackXml
    {
        [XmlElement("Name")]
        public string name;
        [XmlElement("StartFrameNo")]
        public int startFrameNo;
        [XmlElement("EndFrameNo")]
        public int endFrameNo;

        public TrackXml()
        {
        }
    }

    public class TrackData
    {
        public string name;
        public int startFrameNo;
        public int endFrameNo;

        public TrackData()
        {
        }

        public TrackData(TrackXml xml)
        {
            FromXml(xml);
        }

        public void FromXml(TrackXml xml)
        {
            name = xml.name;
            startFrameNo = xml.startFrameNo;
            endFrameNo = xml.endFrameNo;
        }

        public TrackXml ToXml()
        {
            return new TrackXml
            {
                name = name,
                startFrameNo = startFrameNo,
                endFrameNo = endFrameNo,
            };
        }
    }
}