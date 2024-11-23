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
        public bool useHSV = false;

        private Color _color = Color.white;
        public Color color
        {
            get
            {
                return _color;
            }
        }

        private Vector4 _hsv = Vector4.zero;
        public Vector4 hsv
        {
            get
            {
                return _hsv;
            }
        }

        private Color _defaultColor = Color.white;
        public Color defaultColor
        {
            get
            {
                return _color;
            }
        }

        private Vector4 _defaultHSV = Color.white.ToHSVA();
        public Vector4 defaultHSV
        {
            get
            {
                return _defaultHSV;
            }
        }

        public void UpdateColor(Color color, bool updateText)
        {
            if (color == _color && text.Length > 0)
            {
                return;
            }

            _color = color;
            _hsv = color.ToHSVA();

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

        public void UpdateHSV(Vector4 hsv, bool updateText)
        {
            if (hsv == this._hsv && text.Length > 0)
            {
                return;
            }

            _hsv = hsv;
            _color = hsv.FromHSVA();

            if (updateText)
            {
                text = _color.ToHexRGB();
            }
        }

        public void UpdateDefaultColor(Color defaultColor)
        {
            if (defaultColor == _defaultColor)
            {
                return;
            }

            _defaultColor = defaultColor;
            _defaultHSV = defaultColor.ToHSVA();
        }

        public void ResetColor()
        {
            if (_color == _defaultColor)
            {
                return;
            }

            _color = _defaultColor;
            _hsv = _defaultHSV;
            text = _color.ToHexRGB();
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