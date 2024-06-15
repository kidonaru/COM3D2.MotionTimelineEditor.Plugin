namespace COM3D2.MotionTimelineEditor.Plugin
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public class FloatFieldCache
    {
        public string label = null;
        public string text = "";

        private string _format = "F2";
        public string format
        {
            get
            {
                return _format;
            }
            set
            {
                if (value == _format)
                {
                    return;
                }

                _format = value;
                _value = float.NaN;
                text = "";
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

    public class ComboBoxCache<T>
    {
        public string label;
        public List<T> items = new List<T>();
        public Func<T, int, string> getName;
        public Func<T, int, bool> getEnabled;
        public Action<T, int> onSelected;
        public int currentIndex = 0;
        public bool focused = false;
        public Vector2 scrollPosition;
        public Rect buttonRect;

        public int prevIndex
        {
            get
            {
                var prevIndex = currentIndex - 1;
                if (prevIndex < 0)
                {
                    prevIndex = items.Count - 1;
                }
                return prevIndex;
            }
        }

        public int nextIndex
        {
            get
            {
                var nextIndex = currentIndex + 1;
                if (nextIndex >= items.Count)
                {
                    nextIndex = 0;
                }
                return nextIndex;
            }
        }

        public T currentItem
        {
            get
            {
                if (currentIndex >= 0 && currentIndex < items.Count)
                {
                    return items[currentIndex];
                }
                return default(T);
            }
        }
    }
}