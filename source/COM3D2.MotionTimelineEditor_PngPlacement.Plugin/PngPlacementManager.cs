using System.Collections.Generic;
using System.Linq;
using COM3D2.MotionTimelineEditor;
using COM3D2.MotionTimelineEditor.Plugin;
using UnityEngine;
using UnityEngine.Events;

namespace COM3D2.MotionTimelineEditor_PngPlacement.Plugin
{
    public class PngPlacementManager : ManagerBase
    {
        public PngPlacementWrapper pngPlacement;

        public List<PngObjectDataWrapper> pngObjects = new List<PngObjectDataWrapper>();
        private Dictionary<string, PngObjectDataWrapper> pngObjectMap = new Dictionary<string, PngObjectDataWrapper>();
        public List<string> pngObjectNames = new List<string>();

        public static event UnityAction<PngObjectDataWrapper> onObjectAdded;
        public static event UnityAction<PngObjectDataWrapper> onObjectRemoved;

        private static PngPlacementManager _instance;
        public static PngPlacementManager instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new PngPlacementManager();
                }

                return _instance;
            }
        }

        private PngPlacementManager()
        {
            StudioHackManager.onPoseEditingChanged += OnPoseEditingChanged;
        }

        public override void Init()
        {
            pngPlacement = new PngPlacementWrapper();

            if (!pngPlacement.Init())
            {
                pngPlacement = null;
            }
        }

        public bool IsValid()
        {
            return pngPlacement != null;
        }

        public override void LateUpdate()
        {
            LateUpdate(false);
        }

        public void ClearCache()
        {
            pngObjects.Clear();
            pngObjectMap.Clear();
            pngObjectNames.Clear();
        }

        private int _prevUpdateFrame = -1;

        public void LateUpdate(bool force)
        {
            if (!IsValid())
            {
                return;
            }

            if (!force)
            {
                if (Time.frameCount < _prevUpdateFrame + 30 || currentLayer.isAnmPlaying)
                {
                    return;
                }
            }
            _prevUpdateFrame = Time.frameCount;

            var sourceObjList = pngPlacement.listObjectDataWrapper;

            var addedObjects = new List<PngObjectDataWrapper>();
            var removedObjects = new List<PngObjectDataWrapper>();
            var refresh = false;

            foreach (var obj in sourceObjList)
            {
                if (obj.index >= pngObjects.Count)
                {
                    pngObjects.Add(obj);
                    addedObjects.Add(obj);
                    refresh = true;
                    continue;
                }

                var existingObj = pngObjects[obj.index];
                if (existingObj.original != obj.original)
                {
                    refresh = true;
                }

                pngObjects[obj.index].CopyFrom(obj);
            }

            while (pngObjects.Count > sourceObjList.Count)
            {
                var obj = pngObjects[pngObjects.Count - 1];
                pngObjects.RemoveAt(pngObjects.Count - 1);
                removedObjects.Add(obj);
                refresh = true;
            }

            if (refresh)
            {
                pngObjectMap.Clear();
                pngObjectNames.Clear();

                foreach (var obj in pngObjects)
                {
                    pngObjectMap[obj.displayName] = obj;
                    pngObjectNames.Add(obj.displayName);
                }

                MTEUtils.LogDebug("PngPlacementManager: obj list updated");

                foreach (var obj in pngObjects)
                {
                    MTEUtils.LogDebug("  {0}: {1}", obj.index, obj.displayName);
                }

                UpdateDragState();
            }

            foreach (var obj in addedObjects)
            {
                onObjectAdded?.Invoke(obj);
            }

            foreach (var obj in removedObjects)
            {
                onObjectRemoved?.Invoke(obj);
            }

            UpdateTimelineData();
        }

        public void Setup(List<TimelinePngObjectData> pngObjectDatas)
        {
            if (!IsValid())
            {
                return;
            }

            var sourceObjList = pngPlacement.listObjectDataWrapper;

            bool updated = false;
            if (sourceObjList.Count != pngObjectDatas.Count)
            {
                updated = true;
            }
            else
            {
                for (int i = 0; i < pngObjectDatas.Count; i++)
                {
                    var data = pngObjectDatas[i];
                    if (sourceObjList[i].imageName != data.imageName ||
                        sourceObjList[i].primitive != data.primitive ||
                        sourceObjList[i].squareUV != data.squareUV ||
                        sourceObjList[i].shaderDisplay != data.shaderDisplay ||
                        sourceObjList[i].renderQueue != data.renderQueue)
                    {
                        updated = true;
                        break;
                    }
                }
            }

            if (updated)
            {
                pngPlacement.DeleteAllObjects();

                for (int i = 0; i < pngObjectDatas.Count; i++)
                {
                    var data = pngObjectDatas[i];
                    pngPlacement.CreateObject(data.imageName, data.primitive, data.squareUV, data.shaderDisplay);
                }

                var newSourceObjList = pngPlacement.listObjectDataWrapper;

                for (int i = 0; i < pngObjectDatas.Count; i++)
                {
                    var data = pngObjectDatas[i];
                    var obj = newSourceObjList.Find(x =>
                            x.imageName == data.imageName && x.group == data.group);
                    if (obj != null)
                    {
                        obj.SetRenderQueue(data.renderQueue);
                    }
                }
            }

            LateUpdate(true);
        }

        public override void OnLoad()
        {
            if (!IsValid())
            {
                return;
            }

            if (!pngPlacement.isShowNowSceen)
            {
                pngPlacement.RequestUpdateMaid();
                pngPlacement.OnUpdateMaid();
                pngPlacement.LoadAllSetsPlacement();
                pngPlacement.LoadAllSetsEffect();
                pngPlacement.isShowNowSceen = true;
                pngPlacement.Update();
            }

            Setup(timeline.pngObjects);
        }

        public override void OnPluginDisable()
        {
            if (!IsValid())
            {
                return;
            }

            Reset();
        }

        public void Reset()
        {
            pngPlacement.DeleteAllObjects();
            ClearCache();
        }

        public PngObjectDataWrapper GetPngObject(string name)
        {
            return pngObjectMap.GetOrDefault(name);
        }

        private void UpdateTimelineData()
        {
            if (timeline == null)
            {
                return;
            }

            timeline.pngObjects.Clear();

            foreach (var obj in pngObjects)
            {
                var data = new TimelinePngObjectData();
                data.imageName = obj.imageName;
                data.group = obj.group;
                data.primitive = obj.primitive;
                data.squareUV = obj.squareUV;
                data.shaderDisplay = obj.shaderDisplay;
                data.renderQueue = obj.renderQueue;
                timeline.pngObjects.Add(data);
            }
        }

        private void OnPoseEditingChanged(bool isPoseEditing)
        {
            UpdateDragState();
        }

        private void UpdateDragState()
        {
            if (!IsValid())
            {
                return;
            }

            if (studioHackManager.isPoseEditing != pngPlacement.enableDrag)
            {
                pngPlacement.ChangeDragState();
            }
        }
    }
}