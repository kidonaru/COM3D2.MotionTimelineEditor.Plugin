using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TimelineHistoryUI : SubWindowUIBase
    {
        public override string title
        {
            get
            {
                return "操作履歴";
            }
        }

        private static TimelineHistoryManager historyManager
        {
            get
            {
                return TimelineHistoryManager.instance;
            }
        }

        public TimelineHistoryUI(SubWindow subWindow) : base(subWindow)
        {
        }

        public override void DrawContent(GUIView view)
        {
            if (timeline == null)
            {
                return;
            }

            view.BeginHorizontal();
            {
                view.DrawIntField(new GUIView.IntFieldOption
                {
                    label = "最大履歴数",
                    value = config.historyLimit,
                    width = 150,
                    height = 20,
                    onChanged = x =>
                    {
                        config.historyLimit = x;
                        config.dirty = true;
                    }
                });

                if (view.DrawButton("クリア", 50, 20))
                {
                    historyManager.ClearHistory();
                }
            }
            view.EndLayout();

            view.AddSpace(10);

            var historyList = historyManager.historyListInv;
            if (historyList.Count == 0)
            {
                view.DrawLabel("履歴がありません", -1, 20);
            }
            else
            {
                view.padding = Vector2.zero;
                var currentIndex = historyList.Count - 1 - historyManager.historyIndex;

                var index = view.DrawListView(
                    historyList,
                    (history, _) => history.description,
                    null,
                    -1,
                    -1,
                    currentIndex,
                    20);
                if (index >= 0)
                {
                    historyManager.RestoreHistory(historyList.Count - 1 - index);
                }
            }
        }
    }
}