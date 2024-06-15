namespace COM3D2.MotionTimelineEditor.Plugin
{
    using System;
    using System.Collections.Generic;
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

    public abstract class ComboBoxCacheBase
    {
        public string label;
        public int currentIndex = 0;
        public Rect buttonRect;
        public Vector2 contentSize = new Vector2(100, 100);

        public abstract int prevIndex { get; }
        public abstract int nextIndex { get; }
        public abstract void DrawContent(GUIView view);
    }

    public class ComboBoxCache<T> : ComboBoxCacheBase
    {
        public List<T> items = new List<T>();
        public Func<T, int, string> getName;
        public Func<T, int, bool> getEnabled;
        public Action<T, int> onSelected;

        public override int prevIndex
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

        public override int nextIndex
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

        public override void DrawContent(GUIView view)
        {
            float width = this.contentSize.x;
            float height = this.contentSize.y;
            float windowWidth = view.viewRect.width;
            float windowHeight = view.viewRect.height;
            float buttonHeight =  this.buttonRect.height;

            var savedMergin = view.margin;
            var savedPadding = view.padding;

            view.margin = 0;
            view.padding = Vector2.zero;

            var windowRect = new Rect(0, 0, windowWidth, windowHeight);
            GUI.Box(windowRect, "", GUIView.gsMask);

            var savedPos = view.currentPos;
            var savedMaxPos = view.layoutMaxPos;

            var buttonRect = this.buttonRect;
            view.currentPos = buttonRect.position;
            view.currentPos.y += buttonRect.height;

            if (height > this.items.Count * buttonHeight)
            {
                height = this.items.Count * buttonHeight;
            }

            if (view.currentPos.y + height > windowHeight)
            {
                view.currentPos.y = buttonRect.position.y - height;
            }
            if (view.currentPos.y < 0)
            {
                var diff = -view.currentPos.y;
                height -= diff;
                view.currentPos.y = 0;
            }
            if (view.currentPos.x + width > windowWidth)
            {
                view.currentPos.x = windowWidth - width;
            }

            var selectedIndex = view.DrawListView(
                this.items,
                this.getName,
                this.getEnabled,
                width,
                height,
                this.currentIndex,
                buttonHeight);

            if (selectedIndex >= 0)
            {
                view.CancelFocusComboBox();
            }

            if (Event.current.type == EventType.MouseUp &&
                Event.current.button == 0)
            {
                view.CancelFocusComboBox();
            }

            view.currentPos = savedPos;
            view.layoutMaxPos = savedMaxPos;

            view.margin = savedMergin;
            view.padding = savedPadding;

            if (selectedIndex >= 0 && selectedIndex < this.items.Count)
            {
                this.currentIndex = selectedIndex;
                if (this.onSelected != null)
                {
                    this.onSelected(this.items[this.currentIndex], this.currentIndex);
                }
            }
        }
    }
}