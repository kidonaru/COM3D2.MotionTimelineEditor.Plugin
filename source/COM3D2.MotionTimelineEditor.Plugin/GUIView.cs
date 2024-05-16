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

    public class FloatFieldValue
    {
        public string label = null;
        public string text = "";
        public string format = "F2";

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

        public FloatFieldValue()
        {
        }

        public FloatFieldValue(string label)
        {
            this.label = label;
        }

        public static FloatFieldValue[] CreateArray(int count)
        {
            var array = new FloatFieldValue[count];
            for (int i = 0; i < count; i++)
            {
                array[i] = new FloatFieldValue();
            }
            return array;
        }

        public static FloatFieldValue[] CreateArray(string[] labels)
        {
            var array = new FloatFieldValue[labels.Length];
            for (int i = 0; i < labels.Length; i++)
            {
                array[i] = new FloatFieldValue(labels[i]);
            }
            return array;
        }
    }

    public class ComboBoxValue<T>
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
        public bool isScrollViewEnabled { get; private set; }
        public float labelWidth { get; private set; }
        public float layoutMaxX  { get; private set; }
        public float layoutMaxY  { get; private set; }
        public float margin { get; set; }
        public Color defaultColor = Color.white;
        public bool guiEnabled = true;

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
            this.isScrollViewEnabled = true;
            this.currentPos = Vector2.zero;
            return ret;
        }

        public void EndScrollView()
        {
            GUI.EndScrollView();
            this.isScrollViewEnabled = false;

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

        public bool DrawButton(
            string text,
            float width,
            float height,
            GUIStyle style)
        {
            var drawRect = GetDrawRect(width, height);
            var result = GUI.Button(drawRect, text, style);
            this.NextElement(drawRect);
            return result;
        }

        public bool DrawButton(string text, float width, float height)
        {
            return DrawButton(text, width, height, gsButton);
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
            if (label != null)
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

        public float DrawFloatFieldValue(
            FloatFieldValue fieldValue,
            float width,
            float height)
        {
            return DrawFloatFieldValue(fieldValue.label, fieldValue, width, height);
        }

        public float DrawFloatFieldValue(
            string label,
            FloatFieldValue fieldValue,
            float width,
            float height)
        {
            var newText = DrawTextField(label, fieldValue.text, width, height);
            if (newText != fieldValue.text)
            {
                fieldValue.text = newText;

                float newValue;
                if (float.TryParse(newText, out newValue))
                {
                    fieldValue.UpdateValue(newValue, false);
                }
            }
            return fieldValue.value;
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
            ComboBoxValue<T> comboBox,
            float width,
            float height)
        {
            comboBox.buttonRect = GetDrawRect(width, height);

            var name = comboBox.label;
            if (name == null)
            {
                var index = comboBox.currentIndex;
                if (index >= 0 && index < comboBox.items.Count)
                {
                    name = comboBox.getName(comboBox.items[index], index);
                }
            }

            if (DrawButton(name, width, height))
            {
                comboBox.focused = !comboBox.focused;
            }
        }

        public void DrawComboBoxContent<T>(
            ComboBoxValue<T> comboBox,
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
                comboBox.onSelected(comboBox.items[selectedIndex], selectedIndex);
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
            Action<GUIView, T> drawContent,
            float width,
            float height,
            ref Vector2 scrollPosition,
            float itemHeight)
        {
            var contentHeight = (itemHeight + margin) * items.Count;
            var contentRect = GetDrawRect(0, 0, width, height);
            contentRect.width -= 20; // スクロールバーの幅分狭める
            contentRect.height = contentHeight;
            scrollPosition = BeginScrollView(width, height, contentRect, scrollPosition, false, true);

            var itemWidth = contentRect.width;

            BeginLayout(LayoutDirection.Vertical);

            var itemView = new GUIView(0, 0, itemWidth, itemHeight);

            for (int i = 0; i < items.Count; i++)
            {
                var drawRect = GetDrawRect(itemWidth, itemHeight);
                itemView.Init(drawRect);

                var item = items[i];
                drawContent(itemView, item);

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
            FloatFieldValue fieldValue,
            float addedValue1,
            float addedValue2,
            float resetValue,
            float value,
            Action<float> updateNewValue,
            Action<float> updateDiffValue)
        {
            return DrawValue(
                fieldValue,
                addedValue1,
                addedValue2,
                () => updateNewValue(resetValue),
                value,
                updateNewValue,
                updateDiffValue);
        }

        public bool DrawValue(
            FloatFieldValue fieldValue,
            float addedValue1,
            float addedValue2,
            Action resetValueFunc,
            float value,
            Action<float> updateNewValue,
            Action<float> updateDiffValue)
        {
            fieldValue.UpdateValue(value, true);

            var newValue = value;
            var diffValue = 0f;
            var updated = false;

            BeginLayout(LayoutDirection.Horizontal);
            {
                var label = fieldValue.label;
                if (label != null)
                {
                    DrawLabel(label, 50, 20);
                }

                if (DrawButton("<<", 25, 20))
                {
                    diffValue = -addedValue2;
                }
                if (DrawButton("<", 20, 20))
                {
                    diffValue = -addedValue1;
                }

                newValue = DrawFloatFieldValue(null, fieldValue, 50, 20);

                if (DrawButton(">", 20, 20))
                {
                    diffValue = addedValue1;
                }
                if (DrawButton(">>", 25, 20))
                {
                    diffValue = addedValue2;
                }
                if (DrawButton("R", 20, 20))
                {
                    resetValueFunc();
                    updated = true;
                }
            }
            EndLayout();

            if (!float.IsNaN(newValue) && newValue != value)
            {
                updateNewValue(newValue);
                updated = true;
            }
            if (diffValue != 0f)
            {
                updateDiffValue(diffValue);
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
    }
}