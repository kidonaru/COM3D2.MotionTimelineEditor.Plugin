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

        public static GUIStyle gsLabel = new GUIStyle("label")
        {
            fontSize = 12,
            alignment = TextAnchor.MiddleLeft
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
                background = CreateColorTexture(new Color(0, 0, 0, 0))
            },
            hover = {
                background = CreateColorTexture(new Color(0.75f, 0.75f, 0.75f, 0.5f))
            },
            active = {
                background = CreateColorTexture(new Color(0.5f, 0.5f, 0.5f, 0.5f))
            }
        };
        public static GUIStyle gsTooltip = new GUIStyle("box")
        {
            fontSize = 12,
            alignment = TextAnchor.MiddleCenter,
            normal = {
                background = CreateColorTexture(new Color(0, 0, 0, 0.5f))
            }
        };

        public static Vector2 defaultPadding = new Vector2(10, 10);

        public static Texture2D dummyTexture = new Texture2D(1, 1);

        public enum LayoutDirection
        {
            Vertical,
            Horizontal,
            Free,
        }

        public GUIView(float x, float y, float width, float height)
        {
            this._viewRect = new Rect(x, y, width, height);
            this.labelWidth = 100;
            this.padding = defaultPadding;
            this.margin = 5;
            this.layoutDirection = LayoutDirection.Vertical;

            ResetLayout();
        }

        private void ResetLayout()
        {
            this.currentPos.x = 0;
            this.currentPos.y = 0;
            this.layoutMaxX = 0;
            this.layoutMaxY = 0;
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
            scrollViewRect = GetDrawRect(width, height);
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
            if (!enabled) GUI.enabled = false;
            var result = DrawButton(text, width, height);
            if (!enabled) GUI.enabled = true;
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
            if (!enabled) GUI.enabled = false;
            BeginColor(value ? Color.green : Color.white);
            bool newValue = GUI.Toggle(drawRect, value, label, gsToggle);
            EndColor();
            if (!enabled) GUI.enabled = true;
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
            text = DrawTextField(label, text, width, height);
            int.TryParse(text, out value);
            return value;
        }

        public int DrawIntField(int value, float width, float height)
        {
            return DrawIntField(null, value, width, height);
        }

        public float DrawFloatField(string label, float value, float width, float height)
        {
            var text = value.ToString("F2");
            text = DrawTextField(label, text, width, height);
            float.TryParse(text, out value);
            return value;
        }

        public float DrawFloatField(float value, float width, float height)
        {
            return DrawFloatField(null, value, width, height);
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

        public void DrawBox(float width, float height, GUIStyle style)
        {
            var drawRect = GetDrawRect(width, height);
            GUI.Box(drawRect, GUIContent.none, style);
            NextElement(drawRect);
        }

        public static Texture2D CreateColorTexture(Color color)
        {
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }

        public void DrawTexture(Texture2D texture, float width, float height, Color color, Action onClickAction)
        {
            var drawRect = GetDrawRect(width, height);
            BeginColor(color);
            GUI.DrawTexture(drawRect, texture);
            EndColor();
            NextElement(drawRect);

            if (onClickAction != null
                && drawRect.Contains(Event.current.mousePosition)
                && Event.current.type == EventType.MouseDown
                && Event.current.button == 0)
            {
                onClickAction();
            }
        }

        public void DrawTexture(Texture2D texture, float width, float height, Color color)
        {
            DrawTexture(texture, width, height, color, null);
        }

        public void DrawTexture(Texture2D texture, float width, float height)
        {
            DrawTexture(texture, width, height, Color.white, null);
        }

        public void DrawTexture(Texture2D texture)
        {
            DrawTexture(texture, texture.width, texture.height);
        }

        public void DrawTexture(Texture2D texture, Color color)
        {
            DrawTexture(texture, texture.width, texture.height, color);
        }

        public void DrawRect(
            Texture2D texture,
            float width,
            float height,
            Color color,
            float borderSize)
        {
            var drawRect = GetDrawRect(width, height);
            BeginColor(color);

            // 上
            GUI.DrawTexture(new Rect(drawRect.x, drawRect.y, drawRect.width, borderSize), texture);
            // 下
            GUI.DrawTexture(new Rect(drawRect.x, drawRect.y + drawRect.height - borderSize, drawRect.width, borderSize), texture);
            // 左
            GUI.DrawTexture(new Rect(drawRect.x, drawRect.y, borderSize, drawRect.height), texture);
            // 右
            GUI.DrawTexture(new Rect(drawRect.x + drawRect.width - borderSize, drawRect.y, borderSize, drawRect.height), texture);

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
                Vector2 localTouchPosition = Event.current.mousePosition - new Vector2(drawRect.x, drawRect.y);
                onClickAction(localTouchPosition);
            }
        }

        public int DrawListView(
            string[] names,
            float width,
            float height,
            ref Vector2 scrollPosition,
            float buttonHeight)
        {
            int selectedIndex = -1;
            var contentHeight = (buttonHeight + margin) * names.Length;
            var contentRect = GetDrawRect(0, 0, width, height);
            contentRect.width -= 20; // スクロールバーの幅分狭める
            contentRect.height = contentHeight;
            scrollPosition = BeginScrollView(width, height, contentRect, scrollPosition, false, true);

            var buttonWidth = contentRect.width;
            padding.x = 10;
            padding.y = 10;

            BeginLayout(LayoutDirection.Vertical);

            for (int i = 0; i < names.Length; i++)
            {
                if (DrawButton(names[i], buttonWidth, buttonHeight))
                {
                    selectedIndex = i;
                    break;
                }
            }

            EndLayout();

            EndScrollView();
            return selectedIndex;
        }

        public bool DrawTile(
            ITileViewContent content,
            float width,
            float height,
            Action<ITileViewContent> onMouseOver)
        {
            var drawRect = GetDrawRect(width, height);

            bool isClicked = GUI.Button(drawRect, "", gsTile);

            var thumb = content.thum == null ? dummyTexture : content.thum;
            float aspect = (float)thumb.width / thumb.height;

            float drawWidth = drawRect.width;
            float drawHeight = drawWidth / aspect;

            if (drawHeight > drawRect.height - 20) {
                drawHeight = drawRect.height - 20;
                drawWidth = drawHeight * aspect;
            }

            float x = drawRect.x + (drawRect.width - drawWidth) / 2;
            float y = drawRect.y + (drawRect.height - 20 - drawHeight) / 2;

            var imageRect = new Rect(x, y, drawWidth, drawHeight);
            GUI.DrawTexture(imageRect, thumb);

            var labelRect = new Rect(drawRect.x, drawRect.y + drawRect.height - 20, drawRect.width, 20);
            GUI.Label(labelRect, content.name, gsTile);

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
            int columns)
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

            ITileViewContent mouseOverContent = null;
            Action<ITileViewContent> onMouseOver = (content) =>
            {
                mouseOverContent = content;
            };

            var index = 0;
            foreach (var content in contents)
            {
                if (index > 0 && index % columns == 0)
                {
                    EndLayout();
                    BeginLayout(LayoutDirection.Horizontal);
                }

                if (DrawTile(content, tileWidth, tileHeight, onMouseOver))
                {
                    selectedIndex = index;
                    break;
                }

                index++;
            }

            if (mouseOverContent != null)
            {
                var tooltipRect = new Rect(0, Event.current.mousePosition.y + 30, viewRect.width, 20);
                GUI.Box(tooltipRect, mouseOverContent.name, gsTooltip);
            }

            EndLayout();
            EndScrollView();

            return selectedIndex;
        }

        public void DrawTabs(
            string[] tabNames,
            ref int selectedIndex,
            float width,
            float height,
            Action<int> onClickAction)
        {
            BeginLayout(LayoutDirection.Horizontal);
            for (int i = 0; i < tabNames.Length; i++)
            {
                var isSelected = i == selectedIndex;
                var tabStyle = isSelected ? gsSelectedButton : gsButton;

                if (DrawButton(tabNames[i], width, height, tabStyle))
                {
                    selectedIndex = i;
                    if (onClickAction != null) {
                        onClickAction(i);
                    }
                }
            }
            EndLayout();
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