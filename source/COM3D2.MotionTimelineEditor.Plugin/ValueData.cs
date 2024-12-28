namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class ValueData
    {
        public double _value = 0f;
        public TangentData inTangent = new TangentData();
        public TangentData outTangent = new TangentData();

        public float value
        {
            get => (float)_value;
            set => this._value = value;
        }

        public int intValue
        {
            get => (int)_value;
            set => this._value = value;
        }

        public bool boolValue
        {
            get => _value != 0f;
            set => this._value = value == true ? 1f : 0f;
        }

        public void FromValue(ValueData _value)
        {
            this._value = _value._value;
            inTangent.FromTangentData(_value.inTangent);
            outTangent.FromTangentData(_value.outTangent);
        }

        public ValueData Clone()
        {
            ValueData _value = new ValueData();
            _value.FromValue(this);
            return _value;
        }

        public bool Equals(ValueData other)
        {
            return _value == other._value;
        }

        public override bool Equals(object obj)
        {
            ValueData other = obj as ValueData;
            return other != null && Equals(other);
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode()
                    ^ inTangent.GetHashCode()
                    ^ outTangent.GetHashCode();
        }
    }
}