namespace COM3D2.MotionTimelineEditor.Plugin
{
    using UnityEngine;

    public enum FloatFieldType
    {
        Float = 0,
        Int,
    }

    public class FloatFieldCache
    {
        public string label = null;
        public string text = "";

        private FloatFieldType _fieldType = FloatFieldType.Float;
        public FloatFieldType fieldType
        {
            get
            {
                return _fieldType;
            }
            set
            {
                if (value == _fieldType)
                {
                    return;
                }

                _fieldType = value;
                _value = float.NaN;
                text = "";
            }
        }

        public string format
        {
            get
            {
                return fieldType == FloatFieldType.Int ? "F0" : "F2";
            }
        }

        private float _value = float.NaN;
        public float value
        {
            get
            {
                return _value;
            }
        }

        public void UpdateValue(float value, bool updateText)
        {
            if (value == _value)
            {
                return;
            }

            _value = value;

            if (updateText)
            {
                text = float.IsNaN(_value) ? "" : value.ToString(format);
            }
        }

        public void UpdateValue(float value)
        {
            UpdateValue(value, true);
        }

        public FloatFieldCache()
        {
        }
    }

    public class ColorFieldCache
    {
        public string label = null;
        public string text = "";
        public bool hasAlpha = false;

        private Color _color = Color.white;
        public Color color
        {
            get
            {
                return _color;
            }
        }

        public void UpdateColor(Color color, bool updateText)
        {
            if (color == _color && text.Length > 0)
            {
                return;
            }

            _color = color;

            if (updateText)
            {
                if (hasAlpha)
                {
                    text = color.ToHexRGBA();
                }
                else
                {
                    text = color.ToHexRGB();
                }
            }
        }

        public ColorFieldCache()
        {
        }

        public ColorFieldCache(string label, bool hasAlpha)
        {
            this.label = label;
            this.hasAlpha = hasAlpha;
        }
    }
}