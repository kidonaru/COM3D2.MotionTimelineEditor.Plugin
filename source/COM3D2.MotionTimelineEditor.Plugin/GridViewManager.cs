using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class GridViewManager
    {
        private static GridViewManager _instance;
        public static GridViewManager instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GridViewManager();
                }

                return _instance;
            }
        }

        private GridView _gridView = null;

        private GridViewManager()
        {
        }

        public void Init()
        {
            TimelineManager.onRefresh += CreateGrid;
        }

        public void OnPluginDisable()
        {
            RemoveGrid();
        }

        public void OnPluginEnable()
        {
            CreateGrid();
        }

        public void CreateGrid()
        {
            if (_gridView == null)
            {
                var go = new GameObject("Grid");
                _gridView = go.AddComponent<GridView>(); 
            }
        }

        public void RemoveGrid()
        {
            if (_gridView != null)
            {
                Object.Destroy(_gridView.gameObject);
                _gridView = null;
            }
        }

        public void UpdateGridLines()
        {
            if (_gridView != null)
            {
                _gridView.CreateGridLines();
            }
        }
    }
    
}