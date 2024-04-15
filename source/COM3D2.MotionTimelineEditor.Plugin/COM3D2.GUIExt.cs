using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityInjector.ConsoleUtil;


/// Reference:  https://github.com/tsuneko/COM3D2.GUIExt


namespace COM3D2.GUIExtBase
{
    public static class GUIExt
    {
        private static SystemShortcut _SysShortcut = GameMain.Instance.SysShortcut;
        private static List<string> DefaultUIButtons = new List<string>() { "Config", "Ss", "SsUi", "ToTitle", "Info", "Help", "Dic", "Exit" };

        public static void WriteLine(string prefix, ConsoleColor prefixColor, string message, ConsoleColor messageColor, bool error)
        {
            SafeConsole.ForegroundColor = prefixColor;
            Console.Write(prefix);
            if (error)
            {
                SafeConsole.ForegroundColor = ConsoleColor.Red;
                Console.Write("[ERROR] ");
            }
            SafeConsole.ForegroundColor = messageColor;
            Console.WriteLine(message);
        }

        public static void WriteLine(string message, ConsoleColor messageColor, bool error)
        {
            if (error)
            {
                SafeConsole.ForegroundColor = ConsoleColor.Red;
                Console.Write("[ERROR] ");
            }
            SafeConsole.ForegroundColor = messageColor;
            Console.WriteLine(message);
        }

        public static void Write(string prefix, ConsoleColor prefixColor, string message, ConsoleColor messageColor, bool error)
        {
            SafeConsole.ForegroundColor = prefixColor;
            Console.Write(prefix);
            if (error)
            {
                SafeConsole.ForegroundColor = ConsoleColor.Red;
                Console.Write("[ERROR] ");
            }
            SafeConsole.ForegroundColor = messageColor;
            Console.Write(message);
        }

        public static void Write(string message, ConsoleColor messageColor, bool error)
        {
            if (error)
            {
                SafeConsole.ForegroundColor = ConsoleColor.Red;
                Console.Write("[ERROR] ");
            }
            SafeConsole.ForegroundColor = messageColor;
            Console.Write(message);
        }

        public static GameObject Add(string name, string tooltip, Action<GameObject> action)
        {
            return Add(name, tooltip, DefaultIcon, action);
        }

        public static GameObject Add(string name, string tooltip, byte[] png, Action<GameObject> action)
        {
            GameObject _Base = _SysShortcut.transform.Find("Base").gameObject;
            GameObject _Grid = _Base.transform.Find("Grid").gameObject;
            GameObject button = NGUITools.AddChild(_Grid, UTY.GetChildObject(_Grid, "Config", true));
            button.name = name;
            EventDelegate.Set(button.GetComponent<UIButton>().onClick, () => { action(button); });
            UIEventTrigger trigger = button.GetComponent<UIEventTrigger>();
            EventDelegate.Set(trigger.onHoverOver, () => { VisibleExplanationRaw(tooltip, true); });
            EventDelegate.Set(trigger.onHoverOut, () => { _SysShortcut.VisibleExplanation(null, false); });
            EventDelegate.Set(trigger.onDragStart, () => { _SysShortcut.VisibleExplanation(null, false); });
            UISprite sprite = button.GetComponent<UISprite>();
            sprite.type = UIBasicSprite.Type.Filled;
            sprite.fillAmount = 0.0f;
            Texture2D texture = new Texture2D(1, 1);
            texture.LoadImage(png);
            UITexture uitexture = NGUITools.AddWidget<UITexture>(button);
            uitexture.material = new Material(uitexture.shader);
            uitexture.material.mainTexture = texture;
            uitexture.MakePixelPerfect();
            repositionButtons();
            return button;
        }

        public static void Destroy(GameObject button)
        {
            if (button != null)
            {
                NGUITools.Destroy(button);
                repositionButtons();
            }
        }

        public static void SetFrameColor(GameObject button, Color color)
        {
            UITexture uitexture = button.GetComponentInChildren<UITexture>();
            if (uitexture == null)
            {
                return;
            }
            Texture2D texture = uitexture.mainTexture as Texture2D;
            if (texture == null)
            {
                return;
            }
            for (int x = 1; x < texture.width - 1; x++)
            {
                texture.SetPixel(x, 0, color);
                texture.SetPixel(x, texture.height - 1, color);
            }
            for (int y = 1; y < texture.height - 1; y++)
            {
                texture.SetPixel(0, y, color);
                texture.SetPixel(texture.width - 1, y, color);
            }
            texture.Apply();
        }

        public static void ResetFrameColor(GameObject button)
        {
            SetFrameColor(button, new Color(1f, 1f, 1f, 0f));
        }

        public static void repositionButtons(int maxButtonsPerLine, List<string> hiddenButtons)
        {
            GameObject _Base = _SysShortcut.transform.Find("Base").gameObject;
            GameObject _Grid = _Base.transform.Find("Grid").gameObject;
            UISprite _UIBase = _Base.GetComponent<UISprite>();
            UIGrid _UIGrid = _Grid.GetComponent<UIGrid>();
            List<Transform> children = _UIGrid.GetChildList();
            int numButtons = 0;
            if (hiddenButtons == null)
            {
                numButtons = children.Count;
            }
            else
            {
                foreach (Transform child in children)
                {
                    if (!hiddenButtons.Contains(child.name))
                    {
                        numButtons++;
                    }
                }
            }
            float width = _UIGrid.cellWidth;
            float height = width;
            _UIGrid.pivot = UIWidget.Pivot.TopLeft;
            _UIGrid.arrangement = UIGrid.Arrangement.CellSnap;
            _UIGrid.sorting = UIGrid.Sorting.None;
            _UIGrid.maxPerLine = (int)(Screen.width / (width / UIRoot.GetPixelSizeAdjustment(_Base)) * (3f / 4f));
            if (maxButtonsPerLine > 0)
            {
                _UIGrid.maxPerLine = Math.Min(_UIGrid.maxPerLine, maxButtonsPerLine);
            }
            int buttonsX = Math.Min(numButtons, _UIGrid.maxPerLine);
            int buttonsY = Math.Max(1, (numButtons - 1) / _UIGrid.maxPerLine + 1);
            _UIBase.pivot = UIWidget.Pivot.TopRight;
            int baseMarginWidth = (int)(width * 3 / 2 + 8);
            int baseMarginHeight = (int)(height / 2);
            _UIBase.width = (int)(baseMarginWidth + width * buttonsX);
            _UIBase.height = (int)(baseMarginHeight + height * buttonsY + 2f);
            float baseOffsetHeight = baseMarginHeight * 1.5f + 2f;
            _UIBase.transform.localPosition = new Vector3(946f, 502f + baseOffsetHeight, 0f);
            _UIGrid.transform.localPosition = new Vector3(-2f - 2 * width, -baseOffsetHeight, 0f);

            List<string> UIButtons = new List<string>(DefaultUIButtons);
            if (GameMain.Instance.CMSystem.NetUse)
            {
                UIButtons.Insert(3, "Shop");
            }

            int i = 0;
            for (int j = 0; j < UIButtons.Count; j++)
            {
                foreach (Transform child in children)
                {
                    if (hiddenButtons != null && hiddenButtons.Contains(child.name))
                    {
                        child.localPosition = new Vector3(10000, 10000);
                    }
                    else if (child.name == UIButtons[j])
                    {
                        child.localPosition = new Vector3((i % _UIGrid.maxPerLine) * -width, (i / _UIGrid.maxPerLine) * -height, 0f);
                        i++;
                    }
                }
            }

            foreach (Transform child in children)
            {
                if (hiddenButtons != null && hiddenButtons.Contains(child.name))
                {
                    child.localPosition = new Vector3(10000, 10000);
                }
                else if (!UIButtons.Contains(child.name))
                {
                    child.localPosition = new Vector3((i % _UIGrid.maxPerLine) * -width, (i / _UIGrid.maxPerLine) * -height, 0f);
                    i++;
                }
            }

            UISprite _tooltip = typeof(SystemShortcut).GetField("m_spriteExplanation", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(_SysShortcut) as UISprite;
            Vector3 pos = _tooltip.transform.localPosition;
            pos.y = _Base.transform.localPosition.y - _UIBase.height - _tooltip.height;
            _tooltip.transform.localPosition = pos;
        }

        public static void repositionButtons()
        {
            repositionButtons(-1, null);
        }

        public static void VisibleExplanationRaw(string text, bool visible)
        {
            UILabel _labelExplanation = typeof(SystemShortcut).GetField("m_labelExplanation", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(_SysShortcut) as UILabel;
            _labelExplanation.text = text;
            _labelExplanation.width = 0;
            _labelExplanation.MakePixelPerfect();
            UISprite _spriteExplanation = typeof(SystemShortcut).GetField("m_spriteExplanation", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(_SysShortcut) as UISprite;
            _spriteExplanation.width = _labelExplanation.width + 15;
            _spriteExplanation.gameObject.SetActive(visible);
        }

        public static byte[] DefaultIcon = Convert.FromBase64String(
                "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAIAAAD8GO2jAAAAA3NCSVQICAjb4U/g" +
                "AAAACXBIWXMAABYlAAAWJQFJUiTwAAAA/0lEQVRIie2WPYqFMBRGb35QiARM4QZS" +
                "uAX3X7sDkWwgRYSQgJLEKfLGh6+bZywG/JrbnZPLJfChfd/hzuBb6QBA89i2zTln" +
                "jFmWZV1XAPjrZgghAKjrum1bIUTTNFVVvQXOOaXUNE0xxhDC9++llBDS972U8iTQ" +
                "Ws/zPAyDlPJreo5SahxHzrkQAo4baK0B4Dr9gGTgW4Ax5pxfp+dwzjH+JefhvaeU" +
                "lhJQSr33J0GMsRT9A3j7P3gEj+ARPIJHUFBACCnLPYAvAWPsSpn4SAiBMXYSpJSs" +
                "taUE1tqU0knQdR0AKKWu0zMkAwEA5QZnjClevHIvegnuq47o37frH81sg91rI7H3" +
                "AAAAAElFTkSuQmCC");
    }
}
