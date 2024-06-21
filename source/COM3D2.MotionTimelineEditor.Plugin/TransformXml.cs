using System.Xml.Serialization;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TransformXml
    {
        [XmlElement("Name")]
        public string name;

        [XmlElement("Value")]
        public float[] values = new float[0];

        [XmlIgnore]
        private float[] _inTangents = null;

        [XmlArray("InTangents")]
        [XmlArrayItem("Value")]
        public float[] inTangents
        {
            get
            {
                return _inTangents;
            }
            set
            {
                _inTangents = value;
            }
        }

        [XmlIgnore]
        private float[] _outTangents = null;

        [XmlArray("OutTangents")]
        [XmlArrayItem("Value")]
        public float[] outTangents
        {
            get
            {
                return _outTangents;
            }
            set
            {
                _outTangents = value;
            }
        }

        [XmlElement("InSmoothBit")]
        public int inSmoothBit = 0;

        [XmlElement("OutSmoothBit")]
        public int outSmoothBit = 0;

        [XmlIgnore]
        private string[] _strValues = null;

        [XmlArray("StrValues")]
        [XmlArrayItem("Value")]
        public string[] strValues
        {
            get
            {
                return _strValues;
            }
            set
            {
                _strValues = value;
            }
        }

        public TransformXml()
        {
        }
    }
}