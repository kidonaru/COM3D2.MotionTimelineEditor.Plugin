namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class ValueData
    {
        public float value = 0f;
        public TangentData inTangent = new TangentData();
        public TangentData outTangent = new TangentData();

        public int intValue
        {
            get
            {
                return (int)value;
            }
            set
            {
                this.value = value;
            }
        }

        public void FromValue(ValueData value)
        {
            this.value = value.value;
            inTangent.FromTangentData(value.inTangent);
            outTangent.FromTangentData(value.outTangent);
        }

        public bool Equals(ValueData other)
        {
            return value == other.value;
        }

        public override bool Equals(object obj)
        {
            ValueData other = obj as ValueData;
            return other != null && Equals(other);
        }

        public override int GetHashCode()
        {
            return value.GetHashCode()
                    ^ inTangent.GetHashCode()
                    ^ outTangent.GetHashCode();
        }
    }
}