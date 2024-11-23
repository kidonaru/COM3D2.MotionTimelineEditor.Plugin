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

    public class GUIView
    {
        private GUIView _parent = null;
        public GUIView parent
        {
            get
            {
                return _parent;
            }
            set
            {
                _parent = value;

                if (_parent != null)
                {
                    SetEnabled(_parent.guiEnabled);
                }
            }
        }

        public Vector2 currentPos;
        private LayoutDirection layoutDirection;
        public Vector2 padding = defaultPadding;

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

        public Rect scrollViewContentRect;
        public Rect scrollViewRect;
        public Vector2 scrollPosition;

        public bool isScrollViewEnabled;
        public float labelWidth = 100;
        public Vector2 layoutMaxPos;
        public float margin = defaultMargin;
        public Color defaultColor = Color.white;
        public bool guiEnabled = true;

        public class RepeatButtonInfo
        {
            public int lastPressFrame;
            public float startTime;
            public float lastInvokeTime;
        }

        private RepeatButtonInfo _repeatButtonInfo = new RepeatButtonInfo();
        public RepeatButtonInfo repeatButtonInfo
        {
            get
            {
                if (parent != null)
                {
                    return parent.repeatButtonInfo;
                }
                return _repeatButtonInfo;
            }
            set
            {
                if (parent != null)
                {
                    parent.repeatButtonInfo = value;
                }
                else
                {
                    _repeatButtonInfo = value;
                }
            }
        }

        private GUIComboBoxBase _focusedComboBox;
        public GUIComboBoxBase focusedComboBox
        {
            get
            {
                if (parent != null)
                {
                    return parent.focusedComboBox;
                }
                return _focusedComboBox;
            }
            set
            {
                if (parent != null)
                {
                    parent.focusedComboBox = value;
                }
                else
                {
                    _focusedComboBox = value;
                }
            }
        }

        public GUIView topView
        {
            get
            {
                if (parent != null)
                {
                    return parent.topView;
                }
                return this;
            }
        }

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
        public static GUIStyle gsTextArea = new GUIStyle("textArea")
        {
            fontSize = 12,
            alignment = TextAnchor.UpperLeft,
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
            fontSize = 12,
            alignment = TextAnchor.LowerCenter,
            wordWrap = true,
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

        private static TimelineBundleManager bundleManager
        {
            get
            {
                return TimelineBundleManager.instance;
            }
        }

        public enum LayoutDirection
        {
            Vertical,
            Horizontal,
            Free,
        }

        public GUIView()
        {
            Init(Rect.zero);
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
            ResetLayout();
        }

        public void ResetLayout()
        {
            this.layoutDirection = LayoutDirection.Vertical;
            this.currentPos = Vector2.zero;
            this.layoutMaxPos = Vector2.zero;

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

        public void BeginHorizontal()
        {
            BeginLayout(LayoutDirection.Horizontal);
        }

        public void EndLayout()
        {
            this.currentPos.x = 0;
            this.currentPos.y = this.layoutMaxPos.y;
            this.layoutDirection = LayoutDirection.Vertical;
        }

        private void UpdateScrollViewContentRect(Rect newContentRect)
        {
            if (newContentRect.width < 0f) newContentRect.width = viewRect.width - 20;
            if (newContentRect.height < 0f) newContentRect.height = scrollViewContentRect.height;
            if (newContentRect.height < scrollViewRect.height) newContentRect.height = scrollViewRect.height;
            scrollViewContentRect = newContentRect;
        }

        public void BeginScrollView(
            float width,
            float height,
            Rect contentRect,
            GUIStyle horizontalScrollbar,
            GUIStyle verticalScrollbar)
        {
            var savedPadding = padding;
            padding = Vector2.zero;
            scrollViewRect = GetDrawRect(width, height);
            padding = savedPadding;

            UpdateScrollViewContentRect(contentRect);

            scrollPosition = GUI.BeginScrollView(
                scrollViewRect,
                scrollPosition,
                scrollViewContentRect,
                horizontalScrollbar,
                verticalScrollbar);

            this.isScrollViewEnabled = true;
            this.currentPos = Vector2.zero;
        }

        public readonly static Rect AutoScrollViewRect = new Rect(0, 0, -1, -1);

        public void BeginScrollView()
        {
            BeginScrollView(-1, -1, AutoScrollViewRect, false, true);;
        }

        public void BeginScrollView(
            float width,
            float height,
            Rect contentRect,
            bool alwaysShowHorizontal,
            bool alwaysShowVertical)
        {
            var savedPadding = padding;
            padding = Vector2.zero;
            scrollViewRect = GetDrawRect(width, height);
            padding = savedPadding;

            UpdateScrollViewContentRect(contentRect);

            scrollPosition = GUI.BeginScrollView(
                scrollViewRect,
                scrollPosition,
                scrollViewContentRect,
                alwaysShowHorizontal,
                alwaysShowVertical);

            this.isScrollViewEnabled = true;
            this.currentPos = Vector2.zero;
            this.layoutMaxPos = Vector2.zero;
        }

        public void EndScrollView()
        {
            scrollViewContentRect.height = currentPos.y + 20;

            GUI.EndScrollView();
            this.isScrollViewEnabled = false;

            currentPos = scrollViewRect.position;
            NextElement(scrollViewRect);

            this.scrollViewRect = Rect.zero;
        }

        public void NextElement(Rect drawRect)
        {
            if (this.layoutDirection == LayoutDirection.Vertical)
            {
                this.currentPos.x = 0;
                this.currentPos.y += drawRect.height + margin;
                this.layoutMaxPos.y = Math.Max(this.layoutMaxPos.y, this.currentPos.y);
            }
            if (this.layoutDirection == LayoutDirection.Horizontal)
            {
                this.currentPos.x += drawRect.width + margin;
                this.layoutMaxPos.x = Math.Max(this.layoutMaxPos.x, this.currentPos.x);
                this.layoutMaxPos.y = Math.Max(this.layoutMaxPos.y, this.currentPos.y + drawRect.height + margin);
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
            float height)
        {
            var drawRect = GetDrawRect(width, height);
            bool result = GUI.Button(drawRect, "", gsButton);
            if (!GUI.enabled) BeginColor(new Color(1f, 1f, 1f, 0.5f));
            DrawTileThumb(texture, 0, 0, drawRect.width, drawRect.height);
            if (!GUI.enabled) EndColor();
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
                var info = repeatButtonInfo;

                if (info.lastPressFrame < frameNo - 1)
                {
                    PluginUtils.LogDebug("DrawRepeatButton: first press frame={0} lastPressFrame={1}",
                        frameNo, info.lastPressFrame);
                    info.startTime = currentTime;
                    info.lastInvokeTime = currentTime;
                    result = true;
                }

                info.lastPressFrame = frameNo;

                if (currentTime > info.startTime + config.keyRepeatTimeFirst &&
                    currentTime > info.lastInvokeTime + config.keyRepeatTime)
                {
                    //PluginUtils.LogDebug("DrawRepeatButton: repeat frame={0} lastInvokeTime={1}",
                    //    frameNo, info.lastInvokeTime);
                    info.lastInvokeTime = currentTime;
                    result = true;
                }
            }

            return result;
        }

        public bool DrawToggle(
            string label,
            bool value,
            float width,
            float height,
            bool enabled,
            Action<bool> onChanged)
        {
            var drawRect = GetDrawRect(width, height);
            BeginEnabled(enabled);
            BeginColor(value ? Color.green : Color.white);
            bool newValue = GUI.Toggle(drawRect, value, label, gsToggle);
            EndColor();
            EndEnabled();
            this.NextElement(drawRect);

            if (newValue != value)
            {
                onChanged(newValue);
                return true;
            }
            return false;
        }

        public bool DrawToggle(string label, bool value, float width, float height, Action<bool> onChanged)
        {
            return DrawToggle(label, value, width, height, true, onChanged);
        }

        public bool DrawToggle(bool value, float width, float height, Action<bool> onChanged)
        {
            return DrawToggle(null, value, width, height, true, onChanged);
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

        public bool DrawTextField(
            string label,
            float labelWidth,
            string text,
            float width,
            float height,
            Action<string> onChanged,
            bool hasNewLine)
        {
            if (!string.IsNullOrEmpty(label))
            {
                if (labelWidth <= 0f)
                {
                    labelWidth = this.labelWidth;
                }

                var labelRect = GetDrawRect(labelWidth, height);
                GUI.Label(labelRect, label, gsLabel);
                currentPos.x += labelWidth + margin;
                width -= labelWidth + margin;
            }

            if (onChanged == null) GUI.enabled = false;

            var drawRect = GetDrawRect(width, height);
            var newText = text;
            if (hasNewLine)
            {
                newText = GUI.TextArea(drawRect, text, gsTextArea);
            }
            else
            {
                newText = GUI.TextField(drawRect, text, gsTextField);
            }
            this.NextElement(drawRect);

            if (onChanged == null) GUI.enabled = guiEnabled;

            var updated = false;
            if (newText != text)
            {
                onChanged(newText);
                updated = true;
            }

            return updated;
        }

        public bool DrawTextField(
            string label,
            float labelWidth,
            string text,
            float width,
            float height,
            Action<string> onChanged)
        {
            return DrawTextField(label, labelWidth, text, width, height, onChanged, false);
        }

        public void DrawTextField(
            string text,
            float width,
            float height,
            Action<string> onChanged)
        {
            DrawTextField(null, 0f, text, width, height, onChanged);
        }

        public struct TextFieldOption
        {
            public string label;
            public float labelWidth;
            public string value;
            public Action<string> onChanged;
            public int maxLines;
        }

        public bool DrawTextField(TextFieldOption option)
        {
            var height = option.maxLines > 1 ? 20 * option.maxLines : 20;
            var subViewRect = GetDrawRect(-1, height);
            var subView = new GUIView(subViewRect)
            {
                parent = this,
                margin = 0,
                padding = Vector2.zero
            };

            var updated = false;

            subView.BeginLayout(LayoutDirection.Horizontal);
            {
                if (!string.IsNullOrEmpty(option.label))
                {
                    subView.DrawLabel(option.label, option.labelWidth, 20);
                }

                var fieldWidth = subViewRect.width - subView.currentPos.x - 20 * 2;

                updated = subView.DrawTextField(
                    "",
                    0f,
                    option.value,
                    fieldWidth,
                    height,
                    option.onChanged,
                    option.maxLines > 1);

                if (subView.DrawButton("C", 20, 20))
                {
                    GUIUtility.systemCopyBuffer = option.value;
                }

                if (subView.DrawButton("P", 20, 20))
                {
                    option.onChanged(GUIUtility.systemCopyBuffer);
                }
            }
            subView.EndLayout();

            NextElement(subViewRect);

            return updated;
        }

        public struct FloatFieldOption
        {
            public string label;
            public float labelWidth;
            public FloatFieldType fieldType;
            public float value;
            public float minValue;
            public float maxValue;
            public float width;
            public float height;
            public FloatFieldCache fieldCache;
            public Action<float> onChanged;
        }

        public bool DrawFloatField(FloatFieldOption option)
        {
            var fieldCache = option.fieldCache;
            if (fieldCache == null)
            {
                fieldCache = GetFieldCache(option.label, option.fieldType);
                fieldCache.UpdateValue(option.value);
            }

            var updated = false;

            Action<string> onChanged = null;
            if (option.onChanged != null)
            {
                onChanged = newText =>
                {
                    fieldCache.text = newText;

                    float newValue;
                    if (float.TryParse(newText, out newValue))
                    {
                        if (option.minValue != 0f || option.maxValue != 0f)
                        {
                            newValue = Mathf.Clamp(newValue, option.minValue, option.maxValue);
                        }
                        fieldCache.UpdateValue(newValue, false);
                        option.onChanged(newValue);
                        updated = true;
                    }
                };
            }

            DrawTextField(
                option.label,
                option.labelWidth,
                fieldCache.text,
                option.width,
                option.height,
                onChanged);

            return updated;
        }

        public struct IntFieldOption
        {
            public string label;
            public float labelWidth;
            public float value;
            public float minValue;
            public float maxValue;
            public float width;
            public float height;
            public FloatFieldCache fieldCache;
            public Action<int> onChanged;
        }

        public bool DrawIntField(IntFieldOption option)
        {
            var floatOption = new FloatFieldOption
            {
                label = option.label,
                labelWidth = option.labelWidth,
                fieldType = FloatFieldType.Int,
                value = option.value,
                minValue = option.minValue,
                maxValue = option.maxValue,
                width = option.width,
                height = option.height,
                fieldCache = option.fieldCache,
            };
            if (option.onChanged != null)
            {
                floatOption.onChanged = value => option.onChanged((int) value);
            }
            return DrawFloatField(floatOption);
        }

        public Color DrawColorFieldCache(
            string label,
            ColorFieldCache fieldCache,
            float width,
            float height)
        {
            DrawTextField(label, 0f, fieldCache.text, width, height, newText =>
            {
                fieldCache.text = newText;

                Color newColor;
                if (ColorUtility.TryParseHtmlString(newText, out newColor))
                {
                    fieldCache.UpdateColor(newColor, false);
                }
            });

            return fieldCache.color;
        }

        private float DrawSlider(
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

        private float DrawSlider(
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

        public void InvokeActionOnMouse(
            float width,
            float height,
            Action<Vector2> onAction)
        {
            var drawRect = GetDrawRect(width, height);

            if (onAction != null
                && drawRect.Contains(Event.current.mousePosition)
                && Event.current.button == 0)
            {
                Vector2 pos = Event.current.mousePosition - new Vector2(drawRect.x, drawRect.y);
                onAction(pos);
            }
        }

        public void SetFocusComboBox(GUIComboBoxBase comboBox)
        {
            PluginUtils.LogDebug("SetFocusComboBox comboBox={0}", comboBox);
            focusedComboBox = comboBox;
        }

        public bool IsComboBoxFocused()
        {
            return focusedComboBox != null;
        }

        public void CancelFocusComboBox()
        {
            focusedComboBox = null;
        }

        private GUIView _comboBoxSubView = null;

        public void DrawComboBox()
        {
            SetEnabled(true);

            if (focusedComboBox != null)
            {
                if (_comboBoxSubView == null)
                {
                    _comboBoxSubView = new GUIView()
                    {
                        padding = Vector2.zero,
                        margin = 0,
                    };
                }

                _comboBoxSubView.parent = this;
                _comboBoxSubView.Init(_viewRect);

                focusedComboBox.DrawContent(_comboBoxSubView);
            }
        }

        public int DrawListView<T>(
            List<T> items,
            Func<T, int, string> getName,
            Func<T, int, bool> getEnabled,
            float width,
            float height,
            int currentIndex,
            float buttonHeight)
        {
            int selectedIndex = -1;
            var contentHeight = (buttonHeight + margin) * items.Count;
            var contentRect = GetDrawRect(0, 0, width, height);
            contentRect.width -= 20; // スクロールバーの幅分狭める
            contentRect.height = contentHeight;
            BeginScrollView(
                width,
                height,
                contentRect,
                false,
                false);

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
            float itemHeight)
        {
            var contentHeight = (itemHeight + margin) * items.Count + 20;
            var contentRect = GetDrawRect(0, 0, width, height);
            contentRect.width -= 20; // スクロールバーの幅分狭める
            contentRect.height = contentHeight;
            BeginScrollView(
                width,
                height,
                contentRect,
                false,
                true);

            var itemWidth = contentRect.width;

            BeginLayout(LayoutDirection.Vertical);

            var itemRect = new Rect(0, 0, itemWidth, itemHeight);
            var itemView = new GUIView(itemRect)
            {
                parent = this,
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

            if (drawRect.position.y + drawRect.height < scrollPosition.y ||
                drawRect.position.y > scrollPosition.y + scrollViewRect.height)
            {
                NextElement(drawRect);
                return false;
            }

            bool isClicked = GUI.Button(drawRect, "", gsTile);

            DrawTileThumb(content.thum, 0, 0, drawRect.width, drawRect.height - 20);

            var labelRect = new Rect(drawRect.x, drawRect.y + drawRect.height - 40, drawRect.width, 40);
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

            if (drawRect.position.y + drawRect.height < scrollPosition.y ||
                drawRect.position.y > scrollPosition.y + scrollViewRect.height)
            {
                NextElement(drawRect);
                return false;
            }

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

            var labelRect = new Rect(drawRect.x, drawRect.y + drawRect.height - 40, drawRect.width, 40);
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

            BeginScrollView(
                width,
                height,
                contentRect,
                false,
                true);

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

        public bool DrawFloatSelect(
            string label,
            float step1,
            float step2,
            Action onReset,
            float value,
            Action<float> onChanged,
            Action<float> onDiffChanged)
        {
            return DrawValueSelect(label, FloatFieldType.Float, step1, step2, onReset, value, onChanged, onDiffChanged);
        }

        public bool DrawIntSelect(
            string label,
            int step1,
            int step2,
            Action onReset,
            int value,
            Action<int> onChanged,
            Action<int> onDiffChanged)
        {
            return DrawValueSelect(
                label,
                FloatFieldType.Int,
                step1,
                step2,
                onReset,
                value,
                v => onChanged((int)v), 
                v => onDiffChanged((int)v)
            );
        }

        public bool DrawValueSelect(
            string label,
            FloatFieldType fieldType,
            float step1,
            float step2,
            Action onReset,
            float value,
            Action<float> onChanged,
            Action<float> onDiffChanged)
        {
            var fieldCache = GetFieldCache(label, fieldType);
            fieldCache.UpdateValue(value);

            var newValue = value;
            var diffValue = 0f;
            var updated = false;

            var subViewRect = GetDrawRect(220, 20);
            var subView = new GUIView(subViewRect)
            {
                parent = this,
                margin = 0,
                padding = Vector2.zero
            };

            subView.BeginLayout(LayoutDirection.Horizontal);
            {
                if (!string.IsNullOrEmpty(label))
                {
                    subView.DrawLabel(label, 50, 20);
                }

                if (step2 != 0f && subView.DrawRepeatButton("<<", 25, 20))
                {
                    diffValue = -step2;
                }
                if (subView.DrawRepeatButton("<", 20, 20))
                {
                    diffValue = -step1;
                }

                subView.DrawFloatField(new FloatFieldOption
                {
                    value = value,
                    width = 50,
                    height = 20,
                    fieldCache = fieldCache,
                    onChanged = x => newValue = x,
                });

                if (subView.DrawRepeatButton(">", 20, 20))
                {
                    diffValue = step1;
                }
                if (step2 != 0f && subView.DrawRepeatButton(">>", 25, 20))
                {
                    diffValue = step2;
                }

                subView.AddSpace(5);

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
            public string label;
            public float labelWidth;
            public FloatFieldType fieldType;
            public float min;
            public float max;
            public float step;
            public float defaultValue;
            public float value;
            public Action<float> onChanged;
        }

        public bool DrawSliderValue(SliderOption option)
        {
            var fieldCache = GetFieldCache(option.label, option.fieldType);
            fieldCache.UpdateValue(option.value);

            var newValue = option.value;
            var updated = false;

            var subViewRect = GetDrawRect(250, 20);
            var subView = new GUIView(subViewRect)
            {
                parent = this,
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

                subView.DrawFloatField(new FloatFieldOption
                {
                    value = option.value,
                    minValue = option.min,
                    maxValue = option.max,
                    width = 50,
                    height = 20,
                    fieldCache = fieldCache,
                    onChanged = x => newValue = x,
                });

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
            fieldCache.UpdateDefaultColor(resetColor);

            if (fieldCache.useHSV)
            {
                var newHSV = fieldCache.hsv;
                var defaultHSV = fieldCache.defaultHSV;

                DrawSliderValue(new SliderOption
                {
                    label = "H",
                    labelWidth = 30,
                    min = 0f,
                    max = 1f,
                    step = 0.01f,
                    defaultValue = defaultHSV.x,
                    value = newHSV.x,
                    onChanged = x => newHSV.x = x,
                });

                DrawSliderValue(new SliderOption
                {
                    label = "S",
                    labelWidth = 30,
                    min = 0f,
                    max = 1f,
                    step = 0.01f,
                    defaultValue = defaultHSV.y,
                    value = newHSV.y,
                    onChanged = y => newHSV.y = y,
                });

                DrawSliderValue(new SliderOption
                {
                    label = "V",
                    labelWidth = 30,
                    min = 0f,
                    max = 1f,
                    step = 0.01f,
                    defaultValue = defaultHSV.z,
                    value = newHSV.z,
                    onChanged = z => newHSV.z = z,
                });

                if (fieldCache.hasAlpha)
                {
                    DrawSliderValue(new SliderOption
                    {
                        label = "A",
                        labelWidth = 30,
                        min = 0f,
                        max = 1f,
                        step = 0.01f,
                        defaultValue = defaultHSV.w,
                        value = newHSV.w,
                        onChanged = z => newHSV.w = z,
                    });
                }

                fieldCache.UpdateHSV(newHSV, true);
            }
            else
            {
                var newColor = color;

                DrawSliderValue(new SliderOption
                {
                    label = "R",
                    labelWidth = 30,
                    min = 0f,
                    max = 1f,
                    step = 0.01f,
                    defaultValue = resetColor.r,
                    value = color.r,
                    onChanged = x => newColor.r = x,
                });

                DrawSliderValue(new SliderOption
                {
                    label = "G",
                    labelWidth = 30,
                    min = 0f,
                    max = 1f,
                    step = 0.01f,
                    defaultValue = resetColor.g,
                    value = color.g,
                    onChanged = y => newColor.g = y,
                });

                DrawSliderValue(new SliderOption
                {
                    label = "B",
                    labelWidth = 30,
                    min = 0f,
                    max = 1f,
                    step = 0.01f,
                    defaultValue = resetColor.b,
                    value = color.b,
                    onChanged = z => newColor.b = z,
                });

                if (fieldCache.hasAlpha)
                {
                    DrawSliderValue(new SliderOption
                    {
                        label = "A",
                        labelWidth = 30,
                        min = 0f,
                        max = 1f,
                        step = 0.01f,
                        defaultValue = resetColor.a,
                        value = color.a,
                        onChanged = z => newColor.a = z,
                    });
                }

                fieldCache.UpdateColor(newColor, true);
            }

            BeginLayout(LayoutDirection.Horizontal);
            {
                if (fieldCache.label != null)
                {
                    DrawLabel(fieldCache.label, 50, 20);
                }

                DrawTexture(texWhite, 20, 20, color);

                DrawColorFieldCache(null, fieldCache, 120, 20);

                if (DrawButton("R", 20, 20))
                {
                    fieldCache.ResetColor();
                }

                if (DrawTextureButton(bundleManager.changeIcon, 20, 20))
                {
                    fieldCache.useHSV = !fieldCache.useHSV;
                }
            }
            EndLayout();

            var updated = false;
            if (fieldCache.color != color)
            {
                onColorChanged(fieldCache.color);
                updated = true;
            }

            return updated;
        }

        public T DrawTabs<T>(T currentTab, float width, float height)
        {
            BeginLayout(LayoutDirection.Horizontal);
            {
                var savedMargin = margin;
                margin = 0;
                foreach (T tabType in Enum.GetValues(typeof(T)))
                {
                    var color = currentTab.Equals(tabType) ? Color.green : Color.white;
                    if (DrawButton(tabType.ToString(), width, height, true, color))
                    {
                        currentTab = tabType;
                    }
                }
                margin = savedMargin;
            }
            EndLayout();

            AddSpace(5);

            return currentTab;
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

        public FloatFieldCache GetFieldCache(
            string label,
            FloatFieldType fieldType)
        {
            if (parent != null)
            {
                return parent.GetFieldCache(label, fieldType);
            }

            FloatFieldCache fieldCache;
            if (_fieldCacheIndex >= _fieldCaches.Count)
            {
                fieldCache = new FloatFieldCache();
                _fieldCaches.Add(fieldCache);
            }

            fieldCache = _fieldCaches[_fieldCacheIndex++];
            fieldCache.label = label;
            fieldCache.fieldType = fieldType;
            return fieldCache;
        }

        public FloatFieldCache GetFieldCache(string label)
        {
            return GetFieldCache(label, FloatFieldType.Float);
        }

        public FloatFieldCache GetFieldCache(string label, float value)
        {
            var fieldCache = GetFieldCache(label);
            fieldCache.UpdateValue(value);
            return fieldCache;
        }

        public FloatFieldCache GetIntFieldCache(string label)
        {
            return GetFieldCache(label, FloatFieldType.Int);
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

        public TransformCache GetTransformCache(Transform transform)
        {
            if (parent != null)
            {
                return parent.GetTransformCache(transform);
            }

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