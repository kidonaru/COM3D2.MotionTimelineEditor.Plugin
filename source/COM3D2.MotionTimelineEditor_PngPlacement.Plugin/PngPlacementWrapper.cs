using UnityEngine;
using COM3D2.MotionTimelineEditor.Plugin;
using CM3D2.PngPlacement.Plugin;
using System;
using System.Collections.Generic;
using System.Collections;
using COM3D2.MotionTimelineEditor;

namespace COM3D2.MotionTimelineEditor_PngPlacement.Plugin
{
    public class PngPlacementWrapper
    {
        public PngPlacement pngPlacement;
        private PngPlacementField pngPlacementField = new PngPlacementField();
        private ImageMgrField imageMgrField = new ImageMgrField();
        private PlacementMgrField placementMgrField = new PlacementMgrField();
        private EffectMgrField effectMgrField = new EffectMgrField();
        private ShaderMgrField shaderMgrField = new ShaderMgrField();
        private MaidMgrField maidMgrField = new MaidMgrField();
        private LoadDataField loadDataField = new LoadDataField();
        private PngObjectDataField pngObjectDataField = new PngObjectDataField();

        public object placementMgr
        {
            get => pngPlacementField.pm.GetValue(null);
        }

        public object effectMgr
        {
            get => pngPlacementField.em.GetValue(null);
        }

        public object imageMgr
        {
            get => pngPlacementField.im.GetValue(null);
            set => pngPlacementField.im.SetValue(null, value);
        }

        public object maidMgr
        {
            get => pngPlacementField.mm.GetValue(null);
        }

        public bool isShowNowSceen
        {
            get => (bool)pngPlacementField.isShowNowSceen.GetValue(pngPlacement);
            set => pngPlacementField.isShowNowSceen.SetValue(pngPlacement, value);
        }

        public int iPngSel
        {
            get => (int)imageMgrField.iPngSel.GetValue(imageMgr, null);
            set => imageMgrField.iPngSel.SetValue(imageMgr, value, null);
        }

        public List<Texture2D> listImage
        {
            get => (List<Texture2D>)imageMgrField.listImage.GetValue(imageMgr, null);
        }

        public List<string> listImageFilename
        {
            get => (List<string>)imageMgrField.listImageFilename.GetValue(imageMgr, null);
        }

        public int iCurrentObject
        {
            get => (int)placementMgrField.iCurrentObject.GetValue(placementMgr, null);
            set => placementMgrField.iCurrentObject.SetValue(placementMgr, value, null);
        }

        public IList listObjectData
        {
            get => (IList)placementMgrField.listObjectData.GetValue(placementMgr, null);
        }

        public int iPrimitive
        {
            get => (int)placementMgrField.iPrimitive.GetValue(placementMgr, null);
            set => placementMgrField.iPrimitive.SetValue(placementMgr, value, null);
        }

        public bool bSquareUV
        {
            get => (bool)placementMgrField.bSquareUV.GetValue(placementMgr, null);
            set => placementMgrField.bSquareUV.SetValue(placementMgr, value, null);
        }

        public string sShaderDispName
        {
            get => (string)placementMgrField.sShaderDispName.GetValue(placementMgr, null);
            set => placementMgrField.sShaderDispName.SetValue(placementMgr, value, null);
        }

        public bool enableDrag
        {
            get => (bool)placementMgrField.enableDrag.GetValue(placementMgr, null);
            set => placementMgrField.enableDrag.SetValue(placementMgr, value, null);
        }

        public bool bChangeState
        {
            get => (bool)maidMgrField.bChangeState.GetValue(maidMgr, null);
            set => maidMgrField.bChangeState.SetValue(maidMgr, value, null);
        }

        private List<PngObjectDataWrapper> _listObjectDataWrapper = new List<PngObjectDataWrapper>();
        public List<PngObjectDataWrapper> listObjectDataWrapper
        {
            get
            {
                _listObjectDataWrapper.Clear();

                var objList = listObjectData;
                for (var i = 0; i < objList.Count; i++)
                {
                    var obj = objList[i];
                    var wrapper = pngObjectDataField.ConvertToWrapper(obj, i);
                    _listObjectDataWrapper.Add(wrapper);
                }

                FixGroup(_listObjectDataWrapper);
                return _listObjectDataWrapper;
            }
        }

        public string[] shaderDisplays
        {
            get => (string[])shaderMgrField.displays.GetValue(null, null);
        }

        private Dictionary<string, int> _objGroupMap = new Dictionary<string, int>();

        private void FixGroup(List<PngObjectDataWrapper> objList)
        {
            _objGroupMap.Clear();

            foreach (var obj in objList)
            {
                int group = 0;

                if (_objGroupMap.TryGetValue(obj.imageName, out group))
                {
                    group++;
                    if (group == 1) group++; // 1は使わない
                }

                obj.SetGroup(group);
                _objGroupMap[obj.imageName] = group;
            }
        }

        public void Update()
        {
            pngPlacement.Update();
        }

        public void InitImageMgr()
        {
            if (imageMgr == null)
            {
                imageMgr = Activator.CreateInstance(imageMgrField.imageMgrType);
                imageMgrField._Init.Invoke(imageMgr, null);
            }
        }

        public void CreateObject()
        {
            placementMgrField.Create.Invoke(placementMgr, null);
        }

        public bool CreateObject(LoadDataWrapper loadData)
        {
            var obj = ConvertToOriginal(loadData);
            return (bool)placementMgrField.CreateByLoadData.Invoke(placementMgr, new object[] { obj });
        }

        public void CreateObject(string imageName, int primitive, bool squareUV, string shaderDisplay)
        {
            var imageIndex = listImageFilename.IndexOf(imageName);
            if (imageIndex == -1)
            {
                MTEUtils.LogError("Image not found: {0}", imageName);
                return;
            }

            MTEUtils.Log("Create PngObject: {0}, {1}, {2}, {3}", imageName, primitive, squareUV, shaderDisplay);
            this.iPngSel = imageIndex;
            this.iPrimitive = primitive;
            this.bSquareUV = squareUV;
            this.sShaderDispName = shaderDisplay;
            placementMgrField.Create.Invoke(placementMgr, null);
        }

        public void DeleteAllObjects()
        {
            MTEUtils.LogDebug("DeleteAll PngObjects");
            placementMgrField.DeleteAll.Invoke(placementMgr, null);
        }

        public void SetEnable(PngObjectDataWrapper pngObj, bool b)
        {
            if (pngObj.enable == b) return;
            pngObj.enable = b;
            var storedCurrentObject = iCurrentObject;
            iCurrentObject = pngObj.index;
            placementMgrField.SetEnable.Invoke(placementMgr, new object[] { b });
            iCurrentObject = storedCurrentObject;
        }

        public void SetAPNGSpeed(PngObjectDataWrapper pngObj, float f)
        {
            if (pngObj.apng == null || pngObj.apngAnm.speed == f) return;
            pngObj.apngAnm.speed = f;
            placementMgrField.SetAPNGSpeed.Invoke(placementMgr, new object[] { pngObj.index, f });
        }

        public void SetAPNGIsFixedSpeed(PngObjectDataWrapper pngObj, bool b)
        {
            if (pngObj.apng == null || pngObj.apngAnm.isFixedSpeed == b) return;
            pngObj.apngAnm.isFixedSpeed = b;
            placementMgrField.SetAPNGIsFixedSpeed.Invoke(placementMgr, new object[] { pngObj.index, b });
        }

        public void SetScale(PngObjectDataWrapper pngObj, float f)
        {
            if (pngObj.scale == f) return;
            pngObj.scale = f;
            var storedCurrentObject = iCurrentObject;
            iCurrentObject = pngObj.index;
            placementMgrField.SetScale.Invoke(placementMgr, new object[] { f });
            iCurrentObject = storedCurrentObject;
        }

        public void SetScaleMag(PngObjectDataWrapper pngObj, int i)
        {
            if (pngObj.scaleMag == i) return;
            pngObj.scaleMag = i;
            var storedCurrentObject = iCurrentObject;
            iCurrentObject = pngObj.index;
            placementMgrField.SetScaleMag.Invoke(placementMgr, new object[] { i });
            iCurrentObject = storedCurrentObject;
        }

        public void SetScaleZ(PngObjectDataWrapper pngObj, float f)
        {
            if (pngObj.scaleZ == f) return;
            pngObj.scaleZ = f;
            var storedCurrentObject = iCurrentObject;
            iCurrentObject = pngObj.index;
            placementMgrField.SetScaleZ.Invoke(placementMgr, new object[] { f });
            iCurrentObject = storedCurrentObject;
        }

        public void SetRotation(PngObjectDataWrapper pngObj, Vector3 v)
        {
            if (pngObj.rotation == v) return;
            pngObj.rotation = v;
            var storedCurrentObject = iCurrentObject;
            iCurrentObject = pngObj.index;
            placementMgrField.SetRotation.Invoke(placementMgr, new object[] { v });
            iCurrentObject = storedCurrentObject;
        }

        public void SetStopRotation(PngObjectDataWrapper pngObj, bool b)
        {
            if (pngObj.stopRotation == b) return;
            pngObj.stopRotation = b;
            var storedCurrentObject = iCurrentObject;
            iCurrentObject = pngObj.index;
            placementMgrField.SetStopRotation.Invoke(placementMgr, new object[] { b });
            iCurrentObject = storedCurrentObject;
        }

        public void SetFixedCamera(PngObjectDataWrapper pngObj, bool b)
        {
            if (pngObj.fixedCamera == b) return;
            pngObj.fixedCamera = b;
            var storedCurrentObject = iCurrentObject;
            iCurrentObject = pngObj.index;
            placementMgrField.SetFixedCamera.Invoke(placementMgr, new object[] { b });
            iCurrentObject = storedCurrentObject;
        }

        public void SetInversion(PngObjectDataWrapper pngObj, bool b)
        {
            if (pngObj.inversion == b) return;
            pngObj.inversion = b;
            var storedCurrentObject = iCurrentObject;
            iCurrentObject = pngObj.index;
            placementMgrField.SetInversion.Invoke(placementMgr, new object[] { b });
            iCurrentObject = storedCurrentObject;
        }

        public void SetBrightness(PngObjectDataWrapper pngObj, byte b)
        {
            if (pngObj.brightness == b) return;
            pngObj.brightness = b;
            var storedCurrentObject = iCurrentObject;
            iCurrentObject = pngObj.index;
            placementMgrField.SetBrightness.Invoke(placementMgr, new object[] { b });
            iCurrentObject = storedCurrentObject;
        }

        public void SetColor(PngObjectDataWrapper pngObj, Color32 c)
        {
            if (pngObj.color.r == c.r &&
                pngObj.color.g == c.g &&
                pngObj.color.b == c.b &&
                pngObj.color.a == c.a) return;
            pngObj.color = c;
            var storedCurrentObject = iCurrentObject;
            iCurrentObject = pngObj.index;
            placementMgrField.SetColor.Invoke(placementMgr, new object[] { c });
            iCurrentObject = storedCurrentObject;
        }

        public void SetShaderName(PngObjectDataWrapper pngObj, string s)
        {
            if (pngObj.shaderDisplay == s) return;
            pngObj.shaderDisplay = s;
            var storedCurrentObject = iCurrentObject;
            iCurrentObject = pngObj.index;
            placementMgrField.SetShaderName.Invoke(placementMgr, new object[] { s });
            iCurrentObject = storedCurrentObject;
        }

        public void SetRenderQueue(PngObjectDataWrapper pngObj, int i)
        {
            if (pngObj.renderQueue == i) return;
            pngObj.renderQueue = i;
            var storedCurrentObject = iCurrentObject;
            iCurrentObject = pngObj.index;
            placementMgrField.SetRenderQueue.Invoke(placementMgr, new object[] { i });
            iCurrentObject = storedCurrentObject;
        }

        public void SetAttachPoint(PngObjectDataWrapper pngObj, PngAttachPoint p, int iMaid)
        {
            if (pngObj.attach == p && pngObj.maid == iMaid) return;
            pngObj.attach = p;
            var storedCurrentObject = iCurrentObject;
            iCurrentObject = pngObj.index;
            placementMgrField.SetAttachPoint.Invoke(placementMgr, new object[] { (int) p, iMaid });
            iCurrentObject = storedCurrentObject;
        }

        public void SetAttachRotation(PngObjectDataWrapper pngObj, bool b)
        {
            if (pngObj.attachRotation == b) return;
            pngObj.attachRotation = b;
            var storedCurrentObject = iCurrentObject;
            iCurrentObject = pngObj.index;
            placementMgrField.SetAttachRotation.Invoke(placementMgr, new object[] { b });
            iCurrentObject = storedCurrentObject;
        }

        public void LoadAllSetsPlacement()
        {
            placementMgrField.LoadAllSets.Invoke(placementMgr, null);
        }

        public void ChangeDragState()
        {
            placementMgrField.ChangeDragState.Invoke(placementMgr, null);
        }

        public void LoadAllSetsEffect()
        {
            effectMgrField.LoadAllSets.Invoke(effectMgr, null);
        }

        public string GetShaderName(string sDisplay)
        {
            return (string)shaderMgrField.GetName.Invoke(null, new object[] { sDisplay });
        }

        public void SetStopRotationVector(PngObjectDataWrapper pngObj, Vector3 v)
        {
            //if (pngObj.stopRotationVector == v) return;
            pngObj.stopRotationVector = v;
            pngObjectDataField.stopRotationVector.SetValue(pngObj.original, v, null);
        }

        public void SetFixedPos(PngObjectDataWrapper pngObj, Vector3 v)
        {
            //if (pngObj.fixedPos == v) return;
            pngObj.fixedPos = v;
            pngObjectDataField.fixedPos.SetValue(pngObj.original, v, null);
        }

        public LoadDataWrapper ConvertToLoadData(PngObjectDataWrapper pngObj)
        {
            var loadData = new LoadDataWrapper();
            loadData.enable = pngObj.enable;
            loadData.name = pngObj.parentObject.name;
            if (pngObj.attach == PngAttachPoint.none)
            {
                loadData.pos = pngObj.parentObject.transform.position;
            }
            else
            {
                loadData.pos = pngObj.parentObject.transform.localPosition;
            }
            loadData.rotation = pngObj.rotation;
            loadData.inversion = pngObj.inversion;
            loadData.stoprotation = pngObj.stopRotation;
            loadData.stoprotationv = pngObj.stopRotationVector;
            loadData.scale = pngObj.scale;
            loadData.scalemag = pngObj.scaleMag;
            loadData.shader = GetShaderName(pngObj.shaderDisplay);
            loadData.rq = pngObj.renderQueue;
            loadData.fixcamera = pngObj.fixedCamera;
            loadData.fixpos = pngObj.fixedPos;
            loadData.attach = pngObj.attach;
            loadData.attachrotation = pngObj.attachRotation;
            loadData.brightness = pngObj.brightness;
            loadData.primitive = pngObj.primitive;
            loadData.scalez = pngObj.scaleZ;
            loadData.primitivereferencex = pngObj.primitiveReferenceX;
            loadData.squareuv = pngObj.squareUV;
            loadData.maid = pngObj.maid;
            loadData.color = pngObj.color;
            return loadData;
        }

        public object ConvertToOriginal(LoadDataWrapper loadData)
        {
            return loadDataField.ConvertToOriginal(loadData);
        }

        public void FindMaid()
        {
            maidMgrField.Find.Invoke(maidMgr, null);
        }

        public void OnUpdateMaid()
        {
            maidMgrField.OnUpdateMaid.Invoke(maidMgr, null);
        }

        public void RequestUpdateMaid()
        {
            maidMgrField.RequestUpdate.Invoke(maidMgr, null);
        }

        public bool Init()
        {
            {
                GameObject gameObject = GameObject.Find("UnityInjector");
                pngPlacement = gameObject.GetComponent<PngPlacement>();
                MTEUtils.AssertNull(pngPlacement != null, "pngPlacement is null");
            }

            if (pngPlacement == null)
            {
                MTEUtils.LogError("PngPlacementが見つかりませんでした");
                return false;
            }

            var fields = new CustomFieldBase[]
            {
                pngPlacementField,
                imageMgrField,
                placementMgrField,
                effectMgrField,
                shaderMgrField,
                maidMgrField,
                loadDataField,
                pngObjectDataField,
            };

            foreach (var f in fields)
            {
                if (!f.Init())
                {
                    return false;
                }
            }

            InitImageMgr();

            return true;
        }
    }
}