using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TimelineHistoryData
    {
        public string description;
        public TimelineXml xml;
        public long timestamp;
    }

    public class TimelineHistoryManager : ManagerBase
    {
        public List<TimelineHistoryData> historyList = new List<TimelineHistoryData>();
        public int historyIndex = -1;

        private List<TimelineHistoryData> _historyListInv = new List<TimelineHistoryData>();
        public List<TimelineHistoryData> historyListInv
        {
            get
            {
                _historyListInv.Clear();
                for (int i = historyList.Count - 1; i >= 0; i--)
                {
                    _historyListInv.Add(historyList[i]);
                }
                return _historyListInv;
            }
        }

        private static TimelineHistoryManager _instance = null;

        public static TimelineHistoryManager instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new TimelineHistoryManager();
                }
                return _instance;
            }
        }

        private int historyLimit => config.historyLimit;

        private TimelineHistoryManager()
        {
        }

        public void AddHistory(TimelineData timeline, string description)
        {
            if (historyIndex < historyList.Count - 1)
            {
                historyList.RemoveRange(historyIndex + 1, historyList.Count - historyIndex - 1);
            }

            while (historyList.Count > 0 && historyList.Count >= historyLimit)
            {
                historyList.RemoveAt(0);
                historyIndex--;
            }

            if (historyLimit <= 0)
            {
                return;
            }

            var now = System.DateTime.Now;

            var history = new TimelineHistoryData();
            history.xml = timeline.ToXml();
            history.timestamp = now.Ticks;
            history.description = string.Format("[{0}] {1}", now.ToString("MM/dd HH:mm:ss"), description);

            historyList.Add(history);
            historyIndex = historyList.Count - 1;
        }

        public void Undo()
        {
            if (historyIndex <= 0 || historyList.Count == 0)
            {
                return;
            }

            RestoreHistory(historyIndex - 1);
        }

        public void Redo()
        {
            if (historyIndex >= historyList.Count - 1 || historyList.Count == 0)
            {
                return;
            }

            RestoreHistory(historyIndex + 1);
        }

        public void RestoreHistory(int historyIndex)
        {
            if (this.historyIndex == historyIndex)
            {
                return;
            }
            if (historyIndex < 0 || historyIndex >= historyList.Count)
            {
                return;
            }

            this.historyIndex = historyIndex;

            var xml = historyList[historyIndex].xml;
            TimelineManager.instance.UpdateTimeline(xml);
        }

        public void ClearHistory()
        {
            historyList.Clear();
            historyIndex = -1;
        }
    }
}