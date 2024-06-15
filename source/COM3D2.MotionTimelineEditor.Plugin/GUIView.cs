using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public interface ITileViewContent
    {
        string name { get; }
        Texture2D thum { get; }
        bool isDir { get; }
        List<ITileViewContent> children { get; }
    }

    public class GUISubView : GUIView
    {
        public GUIView parent;

        public override int repeatButtonLastPressFrame
        {
            get
            {
                return parent.repeatButtonLastPressFrame;
            }
            set
            {
                parent.repeatButtonLastPressFrame = value;
            }
        }

        public override float repeatButtonStartTime
        {
            get
            {
                return parent.repeatButtonStartTime;
            }
            set
            {
                parent.repeatButtonStartTime = value;
            }
        }

        public override float repeatButtonLastInvokeTime
        {
            get
            {
                return parent.repeatButtonLastInvokeTime;
            }
            set
            {
                parent.repeatButtonLastInvokeTime = value;
            }
        }

        public GUISubView(GUIView parent, float x, float y, float width, float height)
            : base(x, y, width, height)
        {
            this.parent = parent;
            SetEnabled(parent.guiEnabled);
        }

        public GUISubView(GUIView parent, Rect viewRect)
            : base(viewRect)
        {
            this.parent = parent;
            SetEnabled(parent.guiEnabled);
        }

        public override FloatFieldCache GetFieldCache(string label, string format)
        {
            return parent.GetFieldCache(label, format);
        }

        public override TransformCache GetTransformCache(Transform transform)
        {
            return parent.GetTransformCache(transform);
        }
    }

    public class GUIView
    {
        public Vector2 currentPos;
        private LayoutDirection layoutDirection;
        public Vector2 padding;

        private Rect _viewRect;
        public Rect viewRect
        {
            get
            {
                if (isScrollViewEnabled)
                {
                    return scrollViewContentRect;
                }
                return _viewRect;
            }
        }

        public Rect scrollViewContentRect { get; private set; }
        public Rect scrollViewRect { get; private set; }
        public Vector2 scrollPosition { get; private set; }
        public bool isScrollViewEnabled { get; private set; }
        public float labelWidth { get; private set; }
        public float layoutMaxX { get; private set; }
        public float layoutMaxY { get; private set; }
        public float margin { get; set; }
        public Color defaultColor = Color.white;
        public bool guiEnabled = true;

        public virtual int repeatButtonLastPressFrame { get; set; }
        public virtual float repeatButtonStartTime { get; set; }
        public virtual float repeatButtonLastInvokeTime { get; set; }

        private List<FloatFieldCache> _fieldCaches = new List<FloatFieldCache>();
        private int _fieldCacheIndex = 0;

        private List<TransformCache> _transformCaches = new List<TransformCache>();
        private int _transformCacheIndex = 0;

        public static GUIStyle gsLabel = new GUIStyle("label")
        {
            fontSize = 12,
            alignment = TextAnchor.MiddleLeft,
            wordWrap = false,
        };
        public static GUIStyle gsButton = new GUIStyle("button")
        {
            fontSize = 12,
            alignment = TextAnchor.MiddleCenter
        };
        public static GUIStyle gsSelectedButton = new GUIStyle("box")
        {
            fontSize = 12,
            alignment = TextAnchor.MiddleCenter,
        };
        public static GUIStyle gsToggle = new GUIStyle("toggle")
        {
            fontSize = 12,
            alignment = TextAnchor.MiddleLeft,
        };
        public static GUIStyle gsTextField = new GUIStyle("textField")
        {
            fontSize = 12,
            alignment = TextAnchor.MiddleLeft
        };
        public static GUIStyle gsTile = new GUIStyle("button")
        {
            normal = {
                background = CreateColorTexture(new Color(0, 0, 0, 0.5f))
            },
            hover = {
                background = CreateColorTexture(new Color(0.75f, 0.75f, 0.75f, 0.5f))
            },
            active = {
                background = CreateColorTexture(new Color(0.5f, 0.5f, 0.5f, 0.5f))
            }
        };
        public static GUIStyle gsTileLabel = new GUIStyle("button")
        {
            normal = {
                background = CreateColorTexture(new Color(0, 0, 0, 0))
            },
        };
        public static GUIStyle gsMask = new GUIStyle("box")
        {
            fontSize = 12,
            alignment = TextAnchor.MiddleCenter,
            normal = {
                background = CreateColorTexture(new Color(0, 0, 0, 0.5f))
            }
        };
        public static GUIStyle gsBox = new GUIStyle("box")
        {
            fontSize = 12,
            alignment = TextAnchor.MiddleCenter
        };

        public static Vector2 defaultPadding = new Vector2(10, 10);
        public static float defaultMargin = 5;
        public static Texture2D texDummy = new Texture2D(1, 1);
        public static Texture2D texWhite = CreateColorTexture(Color.white);

        private static Config config
        {
            get
            {
                return ConfigManager.config;
            }
        }

        public enum LayoutDirection
        {
            Vertical,
            Horizontal,
            Free,
        }

        public GUIView(float x, float y, float width, float height)
        {
            Init(new Rect(x, y, width, height));
        }

        public GUIView(Rect viewRect)
        {
            Init(viewRect);
        }

        public void Init(float x, float y, float width, float height)
        {
            Init(new Rect(x, y, width, height));
        }
        
        public void Init(Rect viewRect)
        {
            this._viewRect = viewRect;
            this.labelWidth = 100;
            this.padding = defaultPadding;
            this.margin = defaultMargin;

            ResetLayout();
        }

        public void ResetLayout()
        {
            this.layoutDirection = LayoutDirection.Vertical;
            this.currentPos.x = 0;
            this.currentPos.y = 0;
            this.layoutMaxX = 0;
            this.layoutMaxY = 0;

            //PluginUtils.LogDebug("ResetLayout frame={0} _fieldCacheIndex={1} _transformCacheIndex={2}",
            //    Time.frameCount, _fieldCacheIndex, _transformCacheIndex);

            this._fieldCacheIndex = 0;
            this._transformCacheIndex = 0;

            EndEnabled();
        }

        public void BeginLayout(LayoutDirection direction)
        {
            this.layoutDirection = direction;
        }

        public void EndLayout()
        {
            this.currentPos.x = 0;
            this.currentPos.y = this.layoutMaxY;
            this.layoutDirection = LayoutDirection.Vertical;
        }

        public Vector2 BeginScrollView(
            float width,
            float height,
            Rect contentRect,
            Vector2 scrollPosition,
            GUIStyle horizontalScrollbar,
            GUIStyle verticalScrollbar)
        {
            scrollViewRect = GetDrawRect(width, height);
            var ret = GUI.BeginScrollView(
                scrollViewRect,
                scrollPosition,
                contentRect,
                horizontalScrollbar,
                verticalScrollbar);
            this.scrollViewContentRect = contentRect;
            this.scrollPosition = scrollPosition;
            this.isScrollViewEnabled = true;
            this.currentPos = Vector2.zero;
            return ret;
        }

        public Vector2 BeginScrollView(
            float width,
            float height,
            Rect contentRect,
            Vector2 scrollPosition,
            bool alwaysShowHorizontal,
            bool alwaysShowVertical)
        {
            var savedPadding = padding;
            padding = Vector2.zero;
            scrollViewRect = GetDrawRect(width, height);
            padding = savedPadding;

            var ret = GUI.BeginScrollView(
                scrollViewRect,
                scrollPosition,
                contentRect,
                alwaysShowHorizontal,
                alwaysShowVertical);
            this.scrollViewContentRect = contentRect;
            this.scrollPosition = scrollPosition;
            this.isScrollViewEnabled = true;
            this.currentPos = Vector2.zero;
            return ret;
        }

        public void EndScrollView()
        {
            GUI.EndScrollView();
            this.isScrollViewEnabled = false;
            this.scrollViewRect = Rect.zero;
            this.scrollPosition = Vector2.zero;

            currentPos = scrollViewRect.position;
            NextElement(scrollViewRect);
        }

        public void NextElement(Rect drawRect)
        {
            if (this.layoutDirection == LayoutDirection.Vertical)
            {
                this.currentPos.x = 0;
                this.currentPos.y += drawRect.height + margin;
                this.layoutMaxY = Math.Max(this.layoutMaxY, this.currentPos.y);
            }
            if (this.layoutDirection == LayoutDirection.Horizontal)
            {
                this.currentPos.x += drawRect.width + margin;
                this.layoutMaxX = Math.Max(this.layoutMaxX, this.currentPos.x);
                this.layoutMaxY = Math.Max(this.layoutMaxY, this.currentPos.y + drawRect.height + margin);
            }
        }

        public void BeginColor(Color color)
        {
            if (color != defaultColor)
            {
                GUI.color = color;
            }
        }

        public void EndColor()
        {
            if (GUI.color != defaultColor)
            {
                GUI.color = defaultColor;
            }
        }

        public void SetEnabled(bool enabled)
        {
            this.guiEnabled = enabled;
            EndEnabled();
        }

        public void BeginEnabled(bool enabled)
        {
            if (enabled) return;

            if (enabled != guiEnabled)
            {
                GUI.enabled = enabled;
            }
        }

        public void EndEnabled()
        {
            if (GUI.enabled != guiEnabled)
            {
                GUI.enabled = guiEnabled;
            }
        }

        public Rect GetDrawRect(float x, float y, float width, float height)
        {
            x += this.viewRect.x + padding.x;
            y += this.viewRect.y + padding.y;
            if (width < 0) width = this.viewRect.width - currentPos.x - this.padding.x * 2;
            if (height < 0) height = this.viewRect.height - currentPos.y - this.padding.y * 2;
            return new Rect(x, y, width, height);
        }

        public Rect GetDrawRect(float width, float height)
        {
            return GetDrawRect(this.currentPos.x, this.currentPos.y, width, height);
        }

        public bool DrawTextureButton(
            Texture2D texture,
            float width,
            float height,
            Color color)
        {
            var drawRect = GetDrawRect(width, height);
            BeginColor(color);
            bool result = GUI.Button(drawRect, "", gsButton);
            DrawTileThumb(texture, 0, 0, drawRect.width, drawRect.height);
            EndColor();
            NextElement(drawRect);
            return result;
        }

        public bool DrawButton(string text, float width, float height)
        {
            var drawRect = GetDrawRect(width, height);
            var result = GUI.Button(drawRect, text, gsButton);
            this.NextElement(drawRect);
            return result;
        }

        public bool DrawButton(string text, float width, float height, bool enabled)
        {
            BeginEnabled(enabled);
            var result = DrawButton(text, width, height);
            EndEnabled();
            return result;
        }

        public bool DrawButton(string text, float width, float height, bool enabled, Color color)
        {
            BeginColor(color);
            var result = DrawButton(text, width, height, enabled);
            EndColor();
            return result;
        }

        public bool DrawRepeatButton(string text, float width, float height)
        {
            var drawRect = GetDrawRect(width, height);
            var isPressed = GUI.RepeatButton(drawRect, text, gsButton);
            this.NextElement(drawRect);

            bool result = false;
            if (isPressed)
            {
                var frameNo = Time.frameCount;
                var currentTime = Time.realtimeSinceStartup;

                if (repeatButtonLastPressFrame < frameNo - 1)
                {
                    PluginUtils.LogDebug("DrawRepeatButton: first press frame={0} lastPressFrame={1}",
                        frameNo, repeatButtonLastPressFrame);
                    repeatButtonStartTime = currentTime;
                    repeatButtonLastInvokeTime = currentTime;
                    result = true;
                }

                repeatButtonLastPressFrame = frameNo;

                if (currentTime > repeatButtonStartTime + config.keyRepeatTimeFirst &&
                    currentTime > repeatButtonLastInvokeTime + config.keyRepeatTime)
                {
                    PluginUtils.LogDebug("DrawRepeatButton: repeat frame={0} lastInvokeTime={1}",
                        frameNo, repeatButtonLastInvokeTime);
                    repeatButtonLastInvokeTime = currentTime;
                    result = true;
                }
            }

            return result;
        }

        public bool DrawToggle(string label, bool value, float width, float height, bool enabled)
        {
            var drawRect = GetDrawRect(width, height);
            BeginEnabled(enabled);
            BeginColor(value ? Color.green : Color.white);
            bool newValue = GUI.Toggle(drawRect, value, label, gsToggle);
            EndColor();
            EndEnabled();
            this.NextElement(drawRect);
            return newValue;
        }

        public bool DrawToggle(string label, bool value, float width, float height)
        {
            return DrawToggle(label, value, width, height, true);
        }

        public void DrawLabel(
            string text,
            float width,
            float height,
            Color textColor,
            GUIStyle style,
            Action onClickAction)
        {
            var drawRect = GetDrawRect(width, height);
            BeginColor(textColor);
            GUI.Label(drawRect, text, style ?? gsLabel);
            EndColor();
            this.NextElement(drawRect);

            if (onClickAction != null
                && drawRect.Contains(Event.current.mousePosition)
                && Event.current.type == EventType.MouseDown
                && Event.current.button == 0)
            {
                onClickAction();
            }
        }

        public void DrawLabel(string text, float width, float height, Color textColor, GUIStyle style)
        {
            DrawLabel(text, width, height, textColor, style, null);
        }

        public void DrawLabel(string text, float width, float height, Color textColor)
        {
            DrawLabel(text, width, height, textColor, null, null);
        }

        public void DrawLabel(string text, float width, float height)
        {
            DrawLabel(text, width, height, Color.white, null, null);
        }

        public string DrawTextField(string label, string text, float width, float height)
        {
            if (!string.IsNullOrEmpty(label))
            {
                var labelRect = GetDrawRect(labelWidth, height);
                GUI.Label(labelRect, label, gsLabel);
                currentPos.x += labelWidth + margin;
                width -= labelWidth + margin;
            }

            var drawRect = GetDrawRect(width, height);
            text = GUI.TextField(drawRect, text, gsTextField);
            this.NextElement(drawRect);

            return text;
        }

        public string DrawTextField(string text, float width, float height)
        {
            return DrawTextField(null, text, width, height);
        }

        public int DrawIntField(string label, int value, float width, float height)
        {
            var text = value.ToString();
            var newText = DrawTextField(label, text, width, height);
            if (newText != text)
            {
                int.TryParse(newText, out value);
            }
            return value;
        }

        public int DrawIntField(int value, float width, float height)
        {
            return DrawIntField(null, value, width, height);
        }

        public float DrawFloatFieldCache(
            FloatFieldCache fieldCache,
            float width,
            float height)
        {
            return DrawFloatFieldCache(fieldCache.label, fieldCache, width, height);
        }

        public float DrawFloatFieldCache(
            string label,
            FloatFieldCache fieldCache,
            float width,
            float height)
        {
            var newText = DrawTextField(label, fieldCache.text, width, height);
            if (newText != fieldCache.text)
            {
                fieldCache.text = newText;

                float newValue;
                if (float.TryParse(newText, out newValue))
                {
                    fieldCache.UpdateValue(newValue, false);
                }
            }
            return fieldCache.value;
        }

        public float DrawFloatFieldCache(
            string label,
            FloatFieldCache fieldCache,
            float minValue,
            float maxValue,
            float width,
            float height)
        {
            var newText = DrawTextField(label, fieldCache.text, width, height);
            if (newText != fieldCache.text)
            {
                fieldCache.text = newText;

                float newValue;
                if (float.TryParse(newText, out newValue))
                {
                    newValue = Mathf.Clamp(newValue, minValue, maxValue);
                    fieldCache.UpdateValue(newValue, false);
                }
            }
            return fieldCache.value;
        }

        public Color DrawColorFieldCache(
            string label,
            ColorFieldCache fieldCache,
            float width,
            float height)
        {
            var newText = DrawTextField(label, fieldCache.text, width, height);
            if (newText != fieldCache.text)
            {
                fieldCache.text = newText;

                Color newColor;
                if (ColorUtility.TryParseHtmlString(newText, out newColor))
                {
                    fieldCache.UpdateColor(newColor, false);
                }
            }
            return fieldCache.color;
        }

        public float DrawFloatField(string label, float value, float width, float height)
        {
            var text = value.ToString("F2");
            var newText = DrawTextField(label, text, width, height);
            if (newText != text)
            {
                float.TryParse(newText, out value);
            }
            return value;
        }

        public float DrawFloatField(float value, float width, float height)
        {
            return DrawFloatField(null, value, width, height);
        }

        public float DrawSelectFloatField(
            float value, float diffValue, float width, float height)
        {
            var startPos = currentPos;
            var baseDrawRect = GetDrawRect(width, height);

            {
                var buttonRect = GetDrawRect(20, height);
                if (GUI.Button(buttonRect, "<", gsButton))
                {
                    value -= diffValue;
                }
                currentPos.x += 20 + margin;
            }

            var text = value.ToString("F2");
            var drawRect = GetDrawRect(width - (20 + margin) * 2, height);
            var newText = GUI.TextField(drawRect, text, gsTextField);
            if (newText != text)
            {
                float.TryParse(newText, out value);
            }

            currentPos.x += width - (20 + margin) * 2;

            {
                var buttonRect = GetDrawRect(20, height);
                if (GUI.Button(buttonRect, ">", gsButton))
                {
                    value += diffValue;
                }
                currentPos.x += 20 + margin;
            }

            currentPos = startPos;
            NextElement(baseDrawRect);

            return value;
        }

        public float DrawSlider(
            string label,
            float value,
            float min,
            float max,
            float width,
            float height)
        {
            if (label != null)
            {
                var labelRect = GetDrawRect(labelWidth, height);
                GUI.Label(labelRect, label, gsLabel);
                currentPos.x += labelWidth + margin;
                width -= labelWidth + margin;
            }

            var drawRect = GetDrawRect(width, height);
            value = GUI.HorizontalSlider(drawRect, value, min, max);
            this.NextElement(drawRect);

            return value;
        }

        public float DrawSlider(
            float value,
            float min,
            float max,
            float width,
            float height)
        {
            return DrawSlider(null, value, min, max, width, height);
        }

        public void DrawBox(float width, float height)
        {
            var drawRect = GetDrawRect(width, height);
            GUI.Box(drawRect, GUIContent.none, gsBox);
            //NextElement(drawRect);
        }

        public static Texture2D CreateColorTexture(Color color)
        {
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }

        public void DrawTexture(
            Texture2D texture,
            float width,
            float height,
            Color color,
            EventType eventType,
            Action<Vector2> onClickAction)
        {
            var drawRect = GetDrawRect(width, height);
            BeginColor(color);
            GUI.DrawTexture(drawRect, texture);
            EndColor();
            NextElement(drawRect);

            if (onClickAction != null
                && drawRect.Contains(Event.current.mousePosition)
                && Event.current.type == eventType
                && Event.current.button == 0)
            {
                Vector2 pos = Event.current.mousePosition - new Vector2(drawRect.x, drawRect.y);
                onClickAction(pos);
            }
        }

        public void DrawTexture(Texture2D texture, float width, float height, Color color)
        {
            DrawTexture(texture, width, height, color, EventType.MouseDown, null);
        }

        public void DrawTexture(Texture2D texture, float width, float height)
        {
            DrawTexture(texture, width, height, Color.white, EventType.MouseDown, null);
        }

        public void DrawTexture(Texture2D texture)
        {
            DrawTexture(texture, texture.width, texture.height);
        }

        public void DrawTexture(Texture2D texture, Color color)
        {
            DrawTexture(texture, texture.width, texture.height, color);
        }

        public void DrawHorizontalLine(Color color)
        {
            DrawTexture(texWhite, -1, 1, color);
        }

        public void DrawRect(
            float width,
            float height,
            Color color,
            float borderSize)
        {
            var drawRect = GetDrawRect(width, height);
            BeginColor(color);

            // 上
            GUI.DrawTexture(new Rect(drawRect.x, drawRect.y, drawRect.width, borderSize), texWhite);
            // 下
            GUI.DrawTexture(new Rect(drawRect.x, drawRect.y + drawRect.height - borderSize, drawRect.width, borderSize), texWhite);
            // 左
            GUI.DrawTexture(new Rect(drawRect.x, drawRect.y, borderSize, drawRect.height), texWhite);
            // 右
            GUI.DrawTexture(new Rect(drawRect.x + drawRect.width - borderSize, drawRect.y, borderSize, drawRect.height), texWhite);

            EndColor();
            NextElement(drawRect);
        }

        public void InvokeActionOnEvent(
            float width,
            float height,
            EventType eventType,
            Action<Vector2> onClickAction)
        {
            var drawRect = GetDrawRect(width, height);

            if (onClickAction != null
                && drawRect.Contains(Event.current.mousePosition)
                && Event.current.type == eventType
                && Event.current.button == 0)
            {
                Vector2 pos = Event.current.mousePosition - new Vector2(drawRect.x, drawRect.y);
                onClickAction(pos);
            }
        }

        public void DrawComboBoxButton<T>(
            ComboBoxCache<T> comboBox,
            float width,
            float height,
            bool showArrow)
        {
            var name = comboBox.label;
            if (name == null)
            {
                var index = comboBox.currentIndex;
                if (index >= 0 && index < comboBox.items.Count)
                {
                    name = comboBox.getName(comboBox.items[index], index);
                }
            }

            var subViewRect = GetDrawRect(width, height);
            var subView = new GUISubView(this, subViewRect)
            {
                margin = 0,
                padding = Vector2.zero
            };

            subView.BeginLayout(LayoutDirection.Horizontal);
            {
                if (showArrow)
                {
                    if (subView.DrawButton("<", 20, 20))
                    {
                        comboBox.currentIndex = comboBox.prevIndex;
                        if (comboBox.onSelected != null)
                        {
                            comboBox.onSelected(comboBox.items[comboBox.currentIndex], comboBox.currentIndex);
                        }
                    }
                }

                var buttonWidth = width;
                var buttonHeight = height;

                if (showArrow)
                {
                    buttonWidth -= 40;
                }

                comboBox.buttonRect = subView.GetDrawRect(buttonWidth, buttonHeight);
                comboBox.buttonRect.x += scrollViewRect.x - scrollPosition.x;
                comboBox.buttonRect.y += scrollViewRect.y - scrollPosition.y;

                if (subView.DrawButton(name, buttonWidth, buttonHeight))
                {
                    comboBox.focused = !comboBox.focused;
                }

                if (showArrow)
                {
                    if (subView.DrawButton(">", 20, 20))
                    {
                        comboBox.currentIndex = comboBox.nextIndex;
                        if (comboBox.onSelected != null)
                        {
                            comboBox.onSelected(comboBox.items[comboBox.currentIndex], comboBox.currentIndex);
                        }
                    }
                }
            }
            subView.EndLayout();

            NextElement(subViewRect);
        }

        public void DrawComboBoxContent<T>(
            ComboBoxCache<T> comboBox,
            float width,
            float height,
            float windowWidth,
            float windowHeight,
            float buttonHeight)
        {
            if (!comboBox.focused)
            {
                return;
            }

            var savedMergin = margin;
            var savedPadding = padding;

            margin = 0;
            padding = Vector2.zero;

            var windowRect = new Rect(0, 0, windowWidth, windowHeight);
            GUI.Box(windowRect, "", gsMask);

            var savedPos = currentPos;
            var savedLayoutMaxY = layoutMaxY;

            var buttonRect = comboBox.buttonRect;
            currentPos = buttonRect.position;

            if (height > comboBox.items.Count * buttonHeight)
            {
                height = comboBox.items.Count * buttonHeight;
            }

            if (currentPos.y + height > windowHeight)
            {
                currentPos.y = buttonRect.position.y - buttonRect.height - height;
            }
            if (currentPos.x + width > windowWidth)
            {
                currentPos.x = windowWidth - width;
            }

            var selectedIndex = DrawListView(
                comboBox.items,
                comboBox.getName,
                comboBox.getEnabled,
                width,
                height,
                ref comboBox.scrollPosition,
                comboBox.currentIndex,
                buttonHeight);

            if (selectedIndex >= 0)
            {
                comboBox.focused = false;
            }

            if (Event.current.type == EventType.MouseUp &&
                Event.current.button == 0)
            {
                comboBox.focused = false;
            }

            currentPos = savedPos;
            layoutMaxY = savedLayoutMaxY;

            margin = savedMergin;
            padding = savedPadding;

            if (selectedIndex >= 0 && selectedIndex < comboBox.items.Count)
            {
                comboBox.currentIndex = selectedIndex;
                if (comboBox.onSelected != null)
                {
                    comboBox.onSelected(comboBox.items[comboBox.currentIndex], comboBox.currentIndex);
                }
            }
        }

        public int DrawSelectList<T>(
            string label,
            List<T> items,
            Func<T, int, string> getName,
            float width,
            float height,
            int selectedIndex)
        {
            var startPos = currentPos;
            var baseDrawRect = GetDrawRect(width, height);

            if (label != null)
            {
                var labelRect = GetDrawRect(labelWidth, height);
                GUI.Label(labelRect, label, gsLabel);
                currentPos.x += labelWidth + margin;
                width -= labelWidth + margin;
            }

            {
                var buttonRect = GetDrawRect(20, height);
                if (GUI.Button(buttonRect, "<", gsButton))
                {
                    selectedIndex = selectedIndex - 1;
                    if (selectedIndex < 0)
                    {
                        selectedIndex = items.Count - 1;
                    }
                }
                currentPos.x += 20 + margin;
            }

            if (selectedIndex >= 0 && selectedIndex < items.Count) {
                var drawRect = GetDrawRect(width - (20 + margin) * 2, height);
                var name = getName(items[selectedIndex], selectedIndex);
                GUI.Label(drawRect, name, gsLabel);
            }
            currentPos.x += width - (20 + margin) * 2;

            {
                var buttonRect = GetDrawRect(20, height);
                if (GUI.Button(buttonRect, ">", gsButton))
                {
                    selectedIndex = selectedIndex + 1;
                    if (selectedIndex >= items.Count)
                    {
                        selectedIndex = 0;
                    }
                }
                currentPos.x += 20 + margin;
            }

            currentPos = startPos;
            NextElement(baseDrawRect);

            return selectedIndex;
        }

        public int DrawSelectList<T>(
            List<T> items,
            Func<T, int, string> getName,
            float width,
            float height,
            int selectedIndex)
        {
            return DrawSelectList(null, items, getName, width, height, selectedIndex);
        }

        public int DrawListView<T>(
            List<T> items,
            Func<T, int, string> getName,
            Func<T, int, bool> getEnabled,
            float width,
            float height,
            ref Vector2 scrollPosition,
            int currentIndex,
            float buttonHeight)
        {
            int selectedIndex = -1;
            var contentHeight = (buttonHeight + margin) * items.Count;
            var contentRect = GetDrawRect(0, 0, width, height);
            contentRect.width -= 20; // スクロールバーの幅分狭める
            contentRect.height = contentHeight;
            scrollPosition = BeginScrollView(width, height, contentRect, scrollPosition, false, false);

            var buttonWidth = contentRect.width;

            BeginLayout(LayoutDirection.Vertical);

            for (int i = 0; i < items.Count; i++)
            {
                var color = i == currentIndex ? Color.green : Color.white;
                var name = getName(items[i], i);
                var enabled = getEnabled != null ? getEnabled(items[i], i) : true;
                if (DrawButton(name, buttonWidth, buttonHeight, enabled, color))
                {
                    selectedIndex = i;
                    break;
                }
            }

            EndLayout();

            EndScrollView();
            return selectedIndex;
        }

        public void DrawContentListView<T>(
            List<T> items,
            Action<GUIView, T, int> drawContent,
            float width,
            float height,
            ref Vector2 scrollPosition,
            float itemHeight)
        {
            var contentHeight = (itemHeight + margin) * items.Count + 20;
            var contentRect = GetDrawRect(0, 0, width, height);
            contentRect.width -= 20; // スクロールバーの幅分狭める
            contentRect.height = contentHeight;
            scrollPosition = BeginScrollView(width, height, contentRect, scrollPosition, false, true);

            var itemWidth = contentRect.width;

            BeginLayout(LayoutDirection.Vertical);

            var itemRect = new Rect(0, 0, itemWidth, itemHeight);
            var itemView = new GUISubView(this, itemRect)
            {
                scrollViewRect = scrollViewRect,
                scrollPosition = scrollPosition
            };

            for (int i = 0; i < items.Count; i++)
            {
                var drawRect = GetDrawRect(itemWidth, itemHeight);
                itemView.Init(drawRect);

                var item = items[i];
                drawContent(itemView, item, i);

                NextElement(drawRect);
            }

            EndLayout();

            EndScrollView();
        }

        public void DrawTileThumb(
            Texture2D thumb,
            float x,
            float y,
            float width,
            float height)
        {
            if (thumb == null)
            {
                return;
            }

            var drawRect = GetDrawRect(currentPos.x + x, currentPos.y + y, width, height);

            float aspect = (float)thumb.width / thumb.height;

            float thmbWidth = drawRect.width;
            float thmbHeight = thmbWidth / aspect;

            if (thmbHeight > drawRect.height) {
                thmbHeight = drawRect.height;
                thmbWidth = thmbHeight * aspect;
            }

            float thumbX = drawRect.x + (drawRect.width - thmbWidth) / 2;
            float thumbY = drawRect.y + (drawRect.height - thmbHeight) / 2;

            var imageRect = new Rect(thumbX, thumbY, thmbWidth, thmbHeight);
            GUI.DrawTexture(imageRect, thumb);
        }

        public bool DrawTile(
            ITileViewContent content,
            float width,
            float height,
            Action<ITileViewContent> onMouseOver)
        {
            var drawRect = GetDrawRect(width, height);

            bool isClicked = GUI.Button(drawRect, "", gsTile);

            DrawTileThumb(content.thum, 0, 0, drawRect.width, drawRect.height - 20);

            var labelRect = new Rect(drawRect.x, drawRect.y + drawRect.height - 20, drawRect.width, 20);
            GUI.Label(labelRect, content.name, gsTileLabel);

            if (onMouseOver != null)
            {
                if (drawRect.Contains(Event.current.mousePosition))
                {
                    onMouseOver(content);
                }
            }

            NextElement(drawRect);
            return isClicked;
        }

        public bool DrawTileChildren(
            ITileViewContent content,
            float width,
            float height,
            Action<ITileViewContent> onMouseOver)
        {
            var drawRect = GetDrawRect(width, height);

            bool isClicked = GUI.Button(drawRect, "", gsTile);

            var thumbWidth = drawRect.width / 2;
            var thumbHeight = (drawRect.height - 20) / 2;

            var children = content.children;
            for (int i = 0; i < children.Count; i++)
            {
                if (i >= 4)
                {
                    break;
                }

                var child = children[i];
                DrawTileThumb(
                    child.thum,
                    (i % 2) * thumbWidth,
                    (i / 2) * thumbHeight,
                    thumbWidth,
                    thumbHeight);
            }

            var labelRect = new Rect(drawRect.x, drawRect.y + drawRect.height - 20, drawRect.width, 20);
            GUI.Label(labelRect, content.name, gsTileLabel);

            if (onMouseOver != null)
            {
                if (drawRect.Contains(Event.current.mousePosition))
                {
                    onMouseOver(content);
                }
            }

            NextElement(drawRect);
            return isClicked;
        }

        public int DrawTileView(
            IEnumerable<ITileViewContent> contents,
            float width,
            float height,
            ref Vector2 scrollPosition,
            float tileWidth,
            float tileHeight,
            int columns,
            Action<ITileViewContent> onMouseOver)
        {
            int selectedIndex = -1;
            var contentsCount = contents.Count();
            var rowsCount = columns > 0 ? (contentsCount + columns - 1) / columns : 1;
            var contentHeight = (tileHeight + margin) * rowsCount;
            var contentRect = GetDrawRect(0, 0, width, height);
            contentRect.width -= 20; // スクロールバーの幅分狭める
            contentRect.height = contentHeight;
            scrollPosition = BeginScrollView(width, height, contentRect, scrollPosition, false, true);

            BeginLayout(LayoutDirection.Horizontal);

            var index = 0;
            foreach (var content in contents)
            {
                if (index > 0 && index % columns == 0)
                {
                    EndLayout();
                    BeginLayout(LayoutDirection.Horizontal);
                }

                if (content.isDir)
                {
                    if (DrawTileChildren(content, tileWidth, tileHeight, onMouseOver))
                    {
                        selectedIndex = index;
                        break;
                    }
                }
                else
                {
                    if (DrawTile(content, tileWidth, tileHeight, onMouseOver))
                    {
                        selectedIndex = index;
                        break;
                    }
                }

                index++;
            }

            EndLayout();
            EndScrollView();

            return selectedIndex;
        }

        public bool DrawValue(
            FloatFieldCache fieldCache,
            float step1,
            float step2,
            float defaultValue,
            float value,
            Action<float> onChanged,
            Action<float> onDiffChanged)
        {
            return DrawValue(
                fieldCache,
                step1,
                step2,
                () => onChanged(defaultValue),
                value,
                onChanged,
                onDiffChanged);
        }

        public bool DrawValue(
            FloatFieldCache fieldCache,
            float step1,
            float step2,
            Action onReset,
            float value,
            Action<float> onChanged,
            Action<float> onDiffChanged)
        {
            fieldCache.UpdateValue(value);

            var newValue = value;
            var diffValue = 0f;
            var updated = false;

            var subViewRect = GetDrawRect(220, 20);
            var subView = new GUISubView(this, subViewRect)
            {
                margin = 0,
                padding = Vector2.zero
            };

            subView.BeginLayout(LayoutDirection.Horizontal);
            {
                var label = fieldCache.label;
                if (!string.IsNullOrEmpty(label))
                {
                    subView.DrawLabel(label, 50, 20);
                }

                if (subView.DrawRepeatButton("<<", 25, 20))
                {
                    diffValue = -step2;
                }
                if (subView.DrawRepeatButton("<", 20, 20))
                {
                    diffValue = -step1;
                }

                newValue = subView.DrawFloatFieldCache(
                    null,
                    fieldCache,
                    50,
                    20);

                if (subView.DrawRepeatButton(">", 20, 20))
                {
                    diffValue = step1;
                }
                if (subView.DrawRepeatButton(">>", 25, 20))
                {
                    diffValue = step2;
                }

                if (onReset != null && subView.DrawButton("R", 20, 20))
                {
                    onReset();
                    updated = true;
                }
            }
            subView.EndLayout();

            NextElement(subViewRect);

            if (!float.IsNaN(newValue) && newValue != value)
            {
                onChanged(newValue);
                updated = true;
            }
            if (diffValue != 0f)
            {
                onDiffChanged(diffValue);
                updated = true;
            }

            return updated;
        }

        public struct SliderOption
        {
            public float min;
            public float max;
            public float step;
            public float defaultValue;
            public float value;
            public Action onReset;
            public Action<float> onChanged;
            public float labelWidth;
        }

        public bool DrawSliderValue(
            FloatFieldCache fieldCache,
            SliderOption option)
        {
            fieldCache.UpdateValue(option.value);

            var newValue = option.value;
            var updated = false;

            var subViewRect = GetDrawRect(250, 20);
            var subView = new GUISubView(this, subViewRect)
            {
                margin = 0,
                padding = Vector2.zero
            };

            subView.BeginLayout(LayoutDirection.Horizontal);
            {
                var sliderWidth = 170f;

                var label = fieldCache.label;
                if (!string.IsNullOrEmpty(label))
                {
                    subView.DrawLabel(label, option.labelWidth, 20);
                    sliderWidth -= option.labelWidth;
                }

                newValue = subView.DrawFloatFieldCache(
                    null,
                    fieldCache,
                    option.min,
                    option.max,
                    50,
                    20);

                if (option.step > 0f)
                {
                    if (subView.DrawRepeatButton("<", 20, 20))
                    {
                        newValue -= option.step;
                    }
                    if (subView.DrawRepeatButton(">", 20, 20))
                    {
                        newValue += option.step;
                    }
                    sliderWidth -= 40;
                }

                subView.AddSpace(5);

                newValue = subView.DrawSlider(newValue, option.min, option.max, sliderWidth, 20);

                subView.AddSpace(5);

                if (subView.DrawButton("R", 20, 20))
                {
                    newValue = option.defaultValue;
                }
            }
            subView.EndLayout();

            NextElement(subViewRect);

            if (!float.IsNaN(newValue) && newValue != option.value)
            {
                option.onChanged(newValue);
                updated = true;
            }

            return updated;
        }

        public bool DrawColor(
            ColorFieldCache fieldCache,
            Color color,
            Color resetColor,
            Action<Color> onColorChanged)
        {
            fieldCache.UpdateColor(color, true);

            var newColor = color;
            var updated = false;

            BeginLayout(LayoutDirection.Horizontal);
            {
                if (fieldCache.label != null)
                {
                    DrawLabel(fieldCache.label, 50, 20);
                }

                DrawTexture(texWhite, 20, 20, color);

                newColor = DrawColorFieldCache(null, fieldCache, 120, 20);

                if (DrawButton("R", 20, 20))
                {
                    newColor = resetColor;
                }
            }
            EndLayout();

            if (newColor != color)
            {
                onColorChanged(newColor);
                updated = true;
            }

            return updated;
        }

        public void AddSpace(float width, float height)
        {
            var drawRect = GetDrawRect(width, height);
            NextElement(drawRect);
        }

        public void AddSpace(float size)
        {
            AddSpace(size, size);
        }

        public virtual FloatFieldCache GetFieldCache(string label, string format)
        {
            FloatFieldCache fieldCache;
            if (_fieldCacheIndex >= _fieldCaches.Count)
            {
                fieldCache = new FloatFieldCache();
                _fieldCaches.Add(fieldCache);
            }

            fieldCache = _fieldCaches[_fieldCacheIndex++];
            fieldCache.label = label;
            fieldCache.format = format;
            return fieldCache;
        }

        public FloatFieldCache GetFieldCache(string label)
        {
            return GetFieldCache(label, "F2");
        }

        public FloatFieldCache GetFieldCache(string label, float value)
        {
            var fieldCache = GetFieldCache(label);
            fieldCache.UpdateValue(value);
            return fieldCache;
        }

        public FloatFieldCache GetIntFieldCache(string label)
        {
            return GetFieldCache(label, "F0");
        }

        public FloatFieldCache GetIntFieldCache(string label, int value)
        {
            var fieldCache = GetIntFieldCache(label);
            fieldCache.UpdateValue(value);
            return fieldCache;
        }

        public FloatFieldCache[] GetFieldCaches(string[] label)
        {
            var fieldCaches = new FloatFieldCache[label.Length];
            for (var i = 0; i < label.Length; i++)
            {
                fieldCaches[i] = GetFieldCache(label[i]);
            }
            return fieldCaches;
        }

        public virtual TransformCache GetTransformCache(Transform transform)
        {
            if (_transformCacheIndex < _transformCaches.Count)
            {
                var cache = _transformCaches[_transformCacheIndex++];
                cache.Update(transform);
                return cache;
            }

            {
                var cache = new TransformCache();
                cache.Update(transform);
                _transformCaches.Add(cache);
                _transformCacheIndex++;
                return cache;
            }
        }
    }
}