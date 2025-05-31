using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TimelineLoadItem : TileViewContentBase
    {
        public string path;
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
}