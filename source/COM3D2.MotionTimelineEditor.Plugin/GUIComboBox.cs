namespace COM3D2.MotionTimelineEditor.Plugin
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public abstract class GUIComboBoxBase
    {
        public string defaultName;
        public int currentIndex = 0;
        public Vector2 buttonPos;
        public float labelWidth = 100;
        public Vector2 buttonSize = new Vector2(110, 20);
        public Vector2 contentSize = new Vector2(110, 300);
        public bool showArrow = true;

        public abstract int prevIndex { get; }
        public abstract int nextIndex { get; }

        public abstract void DrawButton(string label, GUIView view);

        public void DrawButton(GUIView view)
        {
            DrawButton("", view);
        }

        public abstract void DrawContent(GUIView view);
    }

    public class GUIComboBox<T> : GUIComboBoxBase
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
            set
            {
                currentIndex = items.IndexOf(value);
            }
        }

        private GUIView _buttonSubView = new GUIView(Rect.zero)
        {
            margin = 0,
            padding = Vector2.zero,
        };

        public override void DrawButton(string label, GUIView view)
        {
            var name = this.defaultName;
            if (name == null)
            {
                if (currentIndex >= 0 && currentIndex < this.items.Count)
                {
                    name = this.getName(this.items[currentIndex], currentIndex);
                }
            }

            var subViewWidth = buttonSize.x;
            if (!string.IsNullOrEmpty(label))
            {
                subViewWidth += labelWidth;
            }
            if (showArrow)
            {
                subViewWidth += 40;
            }

            var subViewRect = view.GetDrawRect(subViewWidth, buttonSize.y);
            _buttonSubView.parent = view;
            _buttonSubView.Init(subViewRect);

            _buttonSubView.BeginHorizontal();
            {
                if (!string.IsNullOrEmpty(label))
                {
                    _buttonSubView.DrawLabel(label, labelWidth, buttonSize.y);
                }

                if (showArrow)
                {
                    if (_buttonSubView.DrawButton("<", 20, 20))
                    {
                        this.currentIndex = this.prevIndex;
                        if (this.onSelected != null)
                        {
                            this.onSelected(this.items[this.currentIndex], this.currentIndex);
                        }
                    }
                }

                var buttonDrawRect = _buttonSubView.GetDrawRect(buttonSize.x, buttonSize.y);
                buttonPos = buttonDrawRect.position;

                var topView = view.topView;
                if (topView.isScrollViewEnabled)
                {
                    buttonPos.x += topView.scrollViewRect.x - topView.scrollPosition.x;
                    buttonPos.y += topView.scrollViewRect.y - topView.scrollPosition.y;
                }

                if (_buttonSubView.DrawButton(name, buttonSize.x, buttonSize.y))
                {
                    view.SetFocusComboBox(this);
                }

                if (showArrow)
                {
                    if (_buttonSubView.DrawButton(">", 20, 20))
                    {
                        this.currentIndex = this.nextIndex;
                        if (this.onSelected != null)
                        {
                            this.onSelected(this.items[this.currentIndex], this.currentIndex);
                        }
                    }
                }
            }
            _buttonSubView.EndLayout();

            view.NextElement(subViewRect);
        }

        public override void DrawContent(GUIView view)
        {
            float width = this.contentSize.x + 20; // スクロールバー分広げる
            float height = this.contentSize.y;
            float windowWidth = view.viewRect.width;
            float windowHeight = view.viewRect.height;
            float buttonHeight =  this.buttonSize.y;

            var savedMergin = view.margin;
            var savedPadding = view.padding;

            view.margin = 0;
            view.padding = Vector2.zero;

            var windowRect = new Rect(0, 0, windowWidth, windowHeight);
            GUI.Box(windowRect, "", GUIView.gsMask);

            var savedPos = view.currentPos;
            var savedMaxPos = view.layoutMaxPos;

            view.currentPos = buttonPos;
            view.currentPos.y += buttonSize.y;

            if (height > this.items.Count * buttonHeight)
            {
                height = this.items.Count * buttonHeight;
            }

            if (view.currentPos.y + height > windowHeight)
            {
                view.currentPos.y = buttonPos.y - height;
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