using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TimelineLoadItem : ITileViewContent
    {
        public string name { get; set; }
        private Texture2D _thum;
        public Texture2D thum
        {
            get
            {
                if (children != null && children.Count > 0)
                {
                    return children[0].thum;
                }

                return _thum;
            }
            set
            {
                if (_thum != null)
                {
                    UnityEngine.Object.Destroy(_thum);
                }
                _thum = value;
            }
        }

        public bool isDir { get; set; }
        public List<ITileViewContent> children { get; set; }

        public string path;
        public TimelineLoadItem parent;
    }

    public class TimelineLoadManager : ManagerBase
    {
        private static TimelineLoadManager _instance;
        public static TimelineLoadManager instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new TimelineLoadManager();
                }
                return _instance;
            }
        }

        public TimelineLoadItem rootItem = new TimelineLoadItem();
        public TimelineLoadItem selectedItem = null;

        private TimelineLoadManager()
        {
        }

        public override void Init()
        {
            UpdateItems(false);
        }

        public void Reload()
        {
            UpdateItems(true);
        }

        private void UpdateItems(bool reload)
        {
            if (reload)
            {
                selectedItem = null;
                ClearAllItems(rootItem);
            }

            SearchItems(rootItem, PluginUtils.TimelineDirPath);

            if (selectedItem == null)
            {
                selectedItem = rootItem;
            }
        }

        private void SearchItems(TimelineLoadItem item, string basePath)
        {
            if (item == null)
            {
                return;
            }

            item.path = basePath;

            if (item.children == null)
            {
                item.children = new List<ITileViewContent>(16);
            }

            var files = Directory.GetFiles(basePath, "*.xml");
            Array.Sort(files, new NaturalStringComparer());

            foreach (var path in files)
            {
                var anmName = Path.GetFileNameWithoutExtension(path);
                if (item.children.Exists(t => t.name == anmName && !t.isDir))
                {
                    continue;
                }

                var thumPath = PluginUtils.ConvertThumPath(path);
                var thum = TextureUtils.LoadTexture(thumPath);

                var fileItem = new TimelineLoadItem
                {
                    name = anmName,
                    thum = thum,
                };
                item.children.Add(fileItem);
            }

            var dirs = Directory.GetDirectories(basePath);
            Array.Sort(dirs, new NaturalStringComparer());

            foreach (var path in dirs)
            {
                var dirName = Path.GetFileName(path);
                var dirItem = item.children.Find(t => t.name == dirName && t.isDir) as TimelineLoadItem;
                if (dirItem == null)
                {
                    dirItem = new TimelineLoadItem
                    {
                        name = dirName,
                        parent = item,
                        isDir = true,
                    };
                    item.children.Add(dirItem);
                }

                SearchItems(dirItem, path);
            }
        }

        private void ClearAllItems(TimelineLoadItem item)
        {
            if (item == null)
            {
                return;
            }

            item.thum = null;

            if (item.children != null)
            {
                foreach (var child in item.children)
                {
                    ClearAllItems(child as TimelineLoadItem);
                }

                item.children.Clear();
            }
        }
    }

    // 自然順序での比較
    public class NaturalStringComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            if (x == null && y == null) return 0;
            if (x == null) return -1;
            if (y == null) return 1;

            int len1 = x.Length;
            int len2 = y.Length;
            int marker1 = 0;
            int marker2 = 0;

            while (marker1 < len1 && marker2 < len2)
            {
                char ch1 = x[marker1];
                char ch2 = y[marker2];

                var space1 = new StringBuilder();
                var space2 = new StringBuilder();

                while (marker1 < len1 && char.IsDigit(x[marker1]))
                    space1.Append(x[marker1++]);
                while (marker2 < len2 && char.IsDigit(y[marker2]))
                    space2.Append(y[marker2++]);

                if (space1.Length > 0 && space2.Length > 0)
                {
                    int num1, num2;
                    if (int.TryParse(space1.ToString(), out num1) && 
                        int.TryParse(space2.ToString(), out num2))
                    {
                        int result = num1.CompareTo(num2);
                        if (result != 0) return result;
                    }
                }

                if (ch1 != ch2) return ch1.CompareTo(ch2);
                marker1++;
                marker2++;
            }

            return len1 - len2;
        }
    }
}