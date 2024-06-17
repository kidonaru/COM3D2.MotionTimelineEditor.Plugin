using System.Collections.Generic;
using System.Linq;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class PhotoBGManager
    {
        private Dictionary<string, PhotoBGData> _bgDataMap = null;
        public Dictionary<string, PhotoBGData> bgDataMap
        {
            get
            {
                if (_bgDataMap == null)
                {
                    _bgDataMap = new Dictionary<string, PhotoBGData>();
                    foreach (var _data in PhotoBGData.data)
                    {
                        _bgDataMap[_data.create_prefab_name] = _data;
                    }
                }
                return _bgDataMap;
            }
        }

        private List<PhotoBGData> _bgList = null;
        public List<PhotoBGData> bgList
        {
            get
            {
                if (_bgList == null)
                {
                    _bgList = bgDataMap.Values.ToList();
                }
                return _bgList;
            }
        }

        private Dictionary<string, int> _bgIndexMap = null;
        public Dictionary<string, int> bgIndexMap
        {
            get
            {
                if (_bgIndexMap == null)
                {
                    _bgIndexMap = new Dictionary<string, int>();
                    for (int i = 0; i < bgList.Count; i++)
                    {
                        _bgIndexMap[bgList[i].create_prefab_name] = i;
                    }
                }
                return _bgIndexMap;
            }
        }

        private static PhotoBGManager _instance = null;
        public static PhotoBGManager instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new PhotoBGManager();
                }
                return _instance;
            }
        }

        private PhotoBGManager()
        {
        }

        public int GetBGIndex(string bgName)
        {
            if (string.IsNullOrEmpty(bgName))
            {
                return -1;
            }

            int index;
            if (bgIndexMap.TryGetValue(bgName, out index))
            {
                return index;
            }

            return -1;
        }

        public PhotoBGData GetPhotoBGData(string bgName)
        {
            if (string.IsNullOrEmpty(bgName))
            {
                return null;
            }

            PhotoBGData data;
            if (bgDataMap.TryGetValue(bgName, out data))
            {
                return data;
            }

            return null;
        }

        public string GetDisplayName(string bgName)
        {
            var data = GetPhotoBGData(bgName);
            if (data != null)
            {
                return data.name;
            }

            return bgName;
        }
    }
}