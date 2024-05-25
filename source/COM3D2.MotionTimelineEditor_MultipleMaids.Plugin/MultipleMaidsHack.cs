using System;
using System.IO;
using System.Reflection;
using CM3D2.MultipleMaids.Plugin;
using UnityEngine;
using UnityEngine.SceneManagement;
using COM3D2.MotionTimelineEditor.Plugin;
using System.Collections.Generic;
using UnityEngine.Rendering;
using MyRoomCustom;

namespace COM3D2.MotionTimelineEditor_MultipleMaids.Plugin
{
    using AttachPoint = PhotoTransTargetObject.AttachPoint;

    public class MultipleMaidsHack : StudioHackBase
    {
        private MultipleMaids multipleMaids = null;

        private FieldInfo fieldMaidArray = null;
        private FieldInfo fieldSelectMaidIndex = null;
        private FieldInfo fieldIsLock = null;
        private FieldInfo fieldUnLockFlg = null;
        private FieldInfo fieldIsStop = null;
        private FieldInfo fieldIsBone = null;
        private FieldInfo fieldOkFlg = null;
        private FieldInfo fieldDoguBObject = null;
        private FieldInfo fieldDoguCnt = null;
        private FieldInfo fieldMDogu = null;
        private FieldInfo fieldGDogu = null;
        private FieldInfo fieldMMaterial = null;
        private FieldInfo fieldCubeSize = null;

        private Maid[] maidArray
        {
            get
            {
                return (Maid[])fieldMaidArray.GetValue(multipleMaids);
            }
        }

        private int selectMaidIndex
        {
            get
            {
                return (int)fieldSelectMaidIndex.GetValue(multipleMaids);
            }
            set
            {
                fieldSelectMaidIndex.SetValue(multipleMaids, value);
            }
        }

        private bool[] isLockArray
        {
            get
            {
                return (bool[])fieldIsLock.GetValue(multipleMaids);
            }
        }

        private bool isLock
        {
            get
            {
                var isLockArray = this.isLockArray;
                var selectMaidIndex = this.selectMaidIndex;
                if (selectMaidIndex >= 0 && selectMaidIndex < isLockArray.Length)
                {
                    return isLockArray[selectMaidIndex];
                }

                return false;
            }
            set
            {
                var isLockArray = this.isLockArray;
                var selectMaidIndex = this.selectMaidIndex;
                if (selectMaidIndex >= 0 && selectMaidIndex < isLockArray.Length)
                {
                    isLockArray[selectMaidIndex] = value;
                }
            }
        }

        private bool unLockFlg
        {
            get
            {
                return (bool)fieldUnLockFlg.GetValue(multipleMaids);
            }
            set
            {
                fieldUnLockFlg.SetValue(multipleMaids, value);
            }
        }

        private bool[] isStopArray
        {
            get
            {
                return (bool[])fieldIsStop.GetValue(multipleMaids);
            }
        }

        private bool isStop
        {
            get
            {
                var isStopArray = this.isStopArray;
                var selectMaidIndex = this.selectMaidIndex;
                if (selectMaidIndex >= 0 && selectMaidIndex < isStopArray.Length)
                {
                    return isStopArray[selectMaidIndex];
                }

                return false;
            }
            set
            {
                var isStopArray = this.isStopArray;
                for (int i = 0; i < isStopArray.Length; i++)
                {
                    isStopArray[i] = value;
                }

                maidManager.SetMotionPlayingAll(!value);
            }
        }

        private bool[] isBoneArray
        {
            get
            {
                return (bool[])fieldIsBone.GetValue(multipleMaids);
            }
        }

        private bool isBone
        {
            get
            {
                var isBoneArray = this.isBoneArray;
                var selectMaidIndex = this.selectMaidIndex;
                if (selectMaidIndex >= 0 && selectMaidIndex < isBoneArray.Length)
                {
                    return isBoneArray[selectMaidIndex];
                }

                return false;
            }
            set
            {
                var isBoneArray = this.isBoneArray;
                var selectMaidIndex = this.selectMaidIndex;
                if (selectMaidIndex >= 0 && selectMaidIndex < isBoneArray.Length)
                {
                    isBoneArray[selectMaidIndex] = value;
                }
            }
        }

        private bool okFlg
        {
            get
            {
                return (bool)fieldOkFlg.GetValue(multipleMaids);
            }
        }

        private List<GameObject> doguBObject
        {
            get
            {
                return (List<GameObject>)fieldDoguBObject.GetValue(multipleMaids);
            }
        }

        private int doguCnt
        {
            get
            {
                return (int)fieldDoguCnt.GetValue(multipleMaids);
            }
            set
            {
                fieldDoguCnt.SetValue(multipleMaids, value);
            }
        }

        private MouseDrag6[] mDogu
        {
            get
            {
                return (MouseDrag6[])fieldMDogu.GetValue(multipleMaids);
            }
        }

        private GameObject[] gDogu
        {
            get
            {
                return (GameObject[])fieldGDogu.GetValue(multipleMaids);
            }
        }

        private Material m_material
        {
            get
            {
                return (Material)fieldMMaterial.GetValue(multipleMaids);
            }
        }

        private float cubeSize
        {
            get
            {
                return (float)fieldCubeSize.GetValue(multipleMaids);
            }
        }

        public override int priority
        {
            get
            {
                return 100;
            }
        }

        public override Maid selectedMaid
        {
            get
            {
                var maidArray = this.maidArray;
                var selectMaidIndex = this.selectMaidIndex;
                if (selectMaidIndex < 0 || selectMaidIndex >= maidArray.Length)
                {
                    return null;
                }

                return maidArray[selectMaidIndex];
            }
        }

        private List<Maid> _allMaids = new List<Maid>();
        public override List<Maid> allMaids
        {
            get
            {
                _allMaids.Clear();
                foreach (var maid in maidArray)
                {
                    if (maid != null)
                    {
                        _allMaids.Add(maid);
                    }
                }
                return _allMaids;
            }
        }

        private List<StudioModelStat> _modelList = new List<StudioModelStat>();

        public override List<StudioModelStat> modelList
        {
            get
            {
                _modelList.Clear();

                foreach (var dogu in doguBObject)
                {
                    if (dogu != null)
                    {
                        AttachPoint attachPoint = AttachPoint.Null;
                        int attachMaidSlotNo = -1;

                        var parent = dogu.transform.parent;
                        if (parent != null && BoneUtils.IsValidBoneName(parent.name))
                        {
                            var boneType = BoneUtils.GetBoneTypeByName(parent.name);
                            attachPoint = BoneUtils.GetAttachPoint(boneType);

                            if (attachPoint != AttachPoint.Null)
                            {
                                var maidCaches = maidManager.maidCaches;
                                for (int i = 0; i < maidCaches.Count; ++i)
                                {
                                    var maidCache = maidCaches[i];
                                    if (maidCache.ikManager == null)
                                    {
                                        continue;
                                    }

                                    var bone = maidCache.ikManager.GetBone(boneType);
                                    if (bone == parent.gameObject)
                                    {
                                        attachMaidSlotNo = i;
                                        break;
                                    }
                                }
                            }
                        }

                        var modelName = dogu.name;
                        var model = modelManager.CreateModelStat(
                            modelName,
                            dogu.transform,
                            attachPoint,
                            attachMaidSlotNo);
                        _modelList.Add(model);
                    }
                }

                return _modelList;
            }
        }

        public override int selectedMaidSlotNo
        {
            get
            {
                return selectMaidIndex;
            }
        }

        public override string outputAnmPath
        {
            get
            {
                var path = Path.GetFullPath(".\\") + "Mod\\MultipleMaidsPose\\";
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                return path;
            }
        }

        public override bool hasIkBoxVisible
        {
            get
            {
                return false;
            }
        }

        public override bool isIkBoxVisibleRoot
        {
            get
            {
                return false;
            }
            set
            {
                // do nothing
            }
        }

        public override bool isIkBoxVisibleBody
        {
            get
            {
                return false;
            }
            set
            {
                // do nothing
            }
        }

        public override bool isPoseEditing
        {
            get
            {
                return isLock;
            }
            set
            {
                if (value && isMotionPlaying)
                {
                    isMotionPlaying = false;
                }

                isLock = value;
                isBone = value;
                unLockFlg = value;
            }
        }

        public override bool isMotionPlaying
        {
            get
            {
                return !isStop;
            }
            set
            {
                if (value && isPoseEditing)
                {
                    isPoseEditing = false;
                }

                isStop = !value;
            }
        }

        public override float motionSliderRate
        {
            set
            {
                // do nothing
            }
        }

        public override bool useMuneKeyL
        {
            set
            {
                // do nothing
            }
        }

        public override bool useMuneKeyR
        {
            set
            {
                // do nothing
            }
        }

        public MultipleMaidsHack()
        {
        }

        public override bool Init()
        {
            PluginUtils.Log("初期化中...");

            if (!base.Init())
            {
                return false;
            }

            {
                GameObject gameObject = GameObject.Find("UnityInjector");
                multipleMaids = gameObject.GetComponent<MultipleMaids>();
                PluginUtils.AssertNull(multipleMaids != null, "multipleMaids is null");
            }

            if (multipleMaids == null)
            {
                PluginUtils.LogError("複数メイドプラグインが見つかりませんでした");
                return false;
            }

            {
                BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.InvokeMethod;

                fieldMaidArray = typeof(MultipleMaids).GetField("maidArray", bindingAttr);
                PluginUtils.AssertNull(fieldMaidArray != null, "fieldMaidArray is null");

                fieldSelectMaidIndex = typeof(MultipleMaids).GetField("selectMaidIndex", bindingAttr);
                PluginUtils.AssertNull(fieldSelectMaidIndex != null, "fieldSelectMaidIndex is null");

                fieldIsLock = typeof(MultipleMaids).GetField("isLock", bindingAttr);
                PluginUtils.AssertNull(fieldIsLock != null, "fieldIsLock is null");

                fieldUnLockFlg = typeof(MultipleMaids).GetField("unLockFlg", bindingAttr);
                PluginUtils.AssertNull(fieldUnLockFlg != null, "fieldUnLockFlg is null");

                fieldIsStop = typeof(MultipleMaids).GetField("isStop", bindingAttr);
                PluginUtils.AssertNull(fieldIsStop != null, "fieldIsStop is null");

                fieldIsBone = typeof(MultipleMaids).GetField("isBone", bindingAttr);
                PluginUtils.AssertNull(fieldIsBone != null, "fieldIsBone is null");

                fieldOkFlg = typeof(MultipleMaids).GetField("okFlg", bindingAttr);
                PluginUtils.AssertNull(fieldOkFlg != null, "fieldOkFlg is null");

                fieldDoguBObject = typeof(MultipleMaids).GetField("doguBObject", bindingAttr);
                PluginUtils.AssertNull(fieldDoguBObject != null, "fieldDoguBObject is null");

                fieldDoguCnt = typeof(MultipleMaids).GetField("doguCnt", bindingAttr);
                PluginUtils.AssertNull(fieldDoguCnt != null, "fieldDoguCnt is null");

                fieldMDogu = typeof(MultipleMaids).GetField("mDogu", bindingAttr);
                PluginUtils.AssertNull(fieldMDogu != null, "fieldMDogu is null");

                fieldGDogu = typeof(MultipleMaids).GetField("gDogu", bindingAttr);
                PluginUtils.AssertNull(fieldGDogu != null, "fieldGDogu is null");

                fieldMMaterial = typeof(MultipleMaids).GetField("m_material", bindingAttr);
                PluginUtils.AssertNull(fieldMMaterial != null, "fieldMMaterial is null");

                fieldCubeSize = typeof(MultipleMaids).GetField("cubeSize", bindingAttr);
                PluginUtils.AssertNull(fieldCubeSize != null, "fieldCubeSize is null");
            }

            return true;
        }

        public override void ChangeMaid(Maid maid)
        {
            selectMaidIndex = allMaids.IndexOf(maid);
        }

        public override void OnChangedSceneLevel(Scene sceneName, LoadSceneMode sceneMode)
        {
            base.OnChangedSceneLevel(sceneName, sceneMode);
            isSceneActive = sceneName.name == "SceneEdit" || sceneName.name == "SceneDaily";
        }

        public override bool IsValid()
        {
            if (!base.IsValid())
            {
                return false;
            }

            if (!okFlg)
            {
                _errorMessage = "複数メイドを有効化してください";
                return false;
            }

            return true;
        }

        public GameObject LoadGameModel(string assetName)
        {
            var sourceObj = GameMain.Instance.BgMgr.CreateAssetBundle(assetName);
            if (!sourceObj)
            {
                sourceObj = Resources.Load<GameObject>("Prefab/" + assetName);
            }
            if (!sourceObj)
            {
                sourceObj = Resources.Load<GameObject>("BG/" + assetName);
            }
            if (!sourceObj)
            {
                PluginUtils.LogError("Could not load game model '" + assetName + "'");
                return null;
            }

            var obj = UnityEngine.Object.Instantiate(sourceObj);
            obj.transform.localPosition = Vector3.zero;
            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                if ((bool)renderer && renderer.gameObject.name.Contains("castshadow"))
                {
                    renderer.shadowCastingMode = ShadowCastingMode.Off;
                }
            }

            Collider[] colliders = obj.GetComponentsInChildren<Collider>();
            foreach (Collider collider in colliders)
            {
                if ((bool)collider)
                {
                    collider.enabled = false;
                }
            }

            /*if (obj.transform.localScale != Vector3.one)
            {
                GameObject gameObject3 = new GameObject();
                obj.transform.SetParent(gameObject3.transform, worldPositionStays: true);
                obj = gameObject3;
            }*/

            AddDogu(obj, assetName);

            return obj;
        }

        private GameObject LoadMyRoomObject(int myRoomId)
        {
            var parentObj = GameObject.Find("Deployment Object Parent");
            if (parentObj == null)
            {
                parentObj = new GameObject("Deployment Object Parent");
            }
            var data = PlacementData.GetData(myRoomId);
            var prefab = data.GetPrefab();
            var newObj = UnityEngine.Object.Instantiate<GameObject>(prefab);
            var obj = new GameObject(newObj.name);
            newObj.transform.SetParent(obj.transform, true);
            obj.transform.SetParent(parentObj.transform, false);

            var assetName = "MYR_" + myRoomId;
            AddDogu(obj, assetName);

            return obj;
        }

        private MethodInfo methodProcScriptBin = null;
        private string[] ProcScriptBin(Maid maid, byte[] cd, string filename, bool f_bTemp)
        {
            if (methodProcScriptBin == null)
            {
                methodProcScriptBin = typeof(MultipleMaids).GetMethod("ProcScriptBin", BindingFlags.Static | BindingFlags.InvokeMethod | BindingFlags.NonPublic);
                PluginUtils.AssertNull(methodProcScriptBin != null, "methodProcScriptBin is null");
            }

            return (string[]) methodProcScriptBin.Invoke(null, new object[] { maid, cd, filename, f_bTemp });
        }

        private GameObject LoadModObject(string assetName)
        {
            if (!assetName.EndsWith(".menu", StringComparison.Ordinal))
            {
                PluginUtils.LogWarning("未対応のファイルです。 :" + assetName);
                return null;
            }

            byte[] menuBytes = null;
            using (var file = GameUty.FileOpen(assetName, null))
            {
                if (!file.IsValid())
                {
                    PluginUtils.LogError("メニューファイルが存在しません。 :" + assetName);
                    return null;
                }
                menuBytes = new byte[file.GetSize()];
                file.Read(ref menuBytes, file.GetSize());
            }

            var maid = selectedMaid;
            if (maid == null)
            {
                PluginUtils.LogError("LoadModObject: maid is null");
                return null;
            }

            string[] bin = ProcScriptBin(maid, menuBytes, assetName, false);
            var obj = ImportCM2.LoadSkinMesh_R(bin[0], bin, "", maid.body0.goSlot[8], 1);

            AddDogu(obj, assetName);

            return obj;
        }

        private void AddDogu(GameObject obj, string assetName)
        {
            obj.name = assetName;
            doguBObject.Add(obj);

            this.doguCnt = this.doguBObject.Count - 1;
            var doguCnt = this.doguCnt;

            this.gDogu[doguCnt] = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Vector3 localEulerAngles = obj.transform.localEulerAngles;
            this.gDogu[doguCnt].transform.localEulerAngles = obj.transform.localEulerAngles;
            this.gDogu[doguCnt].transform.position = obj.transform.position;
            this.gDogu[doguCnt].GetComponent<Renderer>().material = this.m_material;
            this.gDogu[doguCnt].layer = 8;
            this.gDogu[doguCnt].GetComponent<Renderer>().enabled = false;
            this.gDogu[doguCnt].SetActive(false);
            this.gDogu[doguCnt].transform.position = obj.transform.position;
            this.mDogu[doguCnt] = this.gDogu[doguCnt].AddComponent<MouseDrag6>();
            this.mDogu[doguCnt].obj = this.gDogu[doguCnt];
            this.mDogu[doguCnt].maid = obj;
            this.mDogu[doguCnt].angles = localEulerAngles;
            this.gDogu[doguCnt].transform.localScale = new Vector3(this.cubeSize, this.cubeSize, this.cubeSize);
            this.mDogu[doguCnt].ido = 1;
            this.mDogu[doguCnt].isScale = false;

            if (assetName == "Particle/pLineY")
            {
                this.mDogu[doguCnt].count = 180;
            }
            if (assetName == "Particle/pLineP02")
            {
                this.mDogu[doguCnt].count = 115;
            }
            if (obj.name == "Particle/pLine_act2")
            {
                this.mDogu[doguCnt].count = 80;
                obj.transform.localScale = new Vector3(3f, 3f, 3f);
            }
            if (obj.name == "Particle/pHeart01")
            {
                this.mDogu[doguCnt].count = 80;
            }
            if (assetName == "mirror1" || assetName == "mirror2" || assetName == "mirror3")
            {
                this.mDogu[doguCnt].isScale = true;
                this.mDogu[doguCnt].isScale2 = true;
                this.mDogu[doguCnt].scale2 = obj.transform.localScale;
                if (assetName == "mirror1")
                {
                    this.mDogu[doguCnt].scale = new Vector3(obj.transform.localScale.x * 5f, obj.transform.localScale.y * 5f, obj.transform.localScale.z * 5f);
                }
                if (assetName == "mirror2")
                {
                    this.mDogu[doguCnt].scale = new Vector3(obj.transform.localScale.x * 10f, obj.transform.localScale.y * 10f, obj.transform.localScale.z * 10f);
                }
                if (assetName == "mirror3")
                {
                    this.mDogu[doguCnt].scale = new Vector3(obj.transform.localScale.x * 33f, obj.transform.localScale.y * 33f, obj.transform.localScale.z * 33f);
                }
            }
            if (assetName == "Odogu_XmasTreeMini_photo_ver" || assetName == "Odogu_KadomatsuMini_photo_ver")
            {
                this.mDogu[doguCnt].isScale2 = true;
                this.mDogu[doguCnt].scale2 = obj.transform.localScale;
            }
        }

        public override void DeleteAllModels()
        {
            for (int l = 0; l < this.doguBObject.Count; l++)
            {
                UnityEngine.Object.Destroy(this.doguBObject[l]);
            }
            this.doguBObject.Clear();
            this.doguCnt = 0;
        }

        public override void DeleteModel(StudioModelStat model)
        {
            var index = doguBObject.FindIndex(d => d.transform == model.transform);
            if (index >= 0)
            {
                UnityEngine.Object.Destroy(this.doguBObject[index]);
                doguBObject.RemoveAt(index);
            }
        }

        public override void CreateModel(StudioModelStat model)
        {
            try
            {
                GameObject obj = null;
                if (model.info.type == StudioModelType.Prefab ||
                    model.info.type == StudioModelType.Asset)
                {
                    obj = LoadGameModel(model.info.fileName);
                }
                else if (model.info.type == StudioModelType.MyRoom)
                {
                    obj = LoadMyRoomObject((int)model.info.myRoomId);
                }
                else if (model.info.type == StudioModelType.Mod)
                {
                    obj = LoadModObject(model.info.fileName);
                }

                if (obj == null)
                {
                    PluginUtils.LogError("CreateModel: モデルの追加に失敗しました" + model.name);
                    return;
                }

                var attachPoint = model.attachPoint;
                var attachMaidSlotNo = model.attachMaidSlotNo;
                var maidCache = maidManager.GetMaidCache(attachMaidSlotNo);
                if (maidCache != null)
                {
                    maidCache.AttachItem(obj.transform, attachPoint, false);
                }
            }
            catch (Exception e)
            {
                PluginUtils.LogException(e);
            }
        }

        protected override void OnMaidChanged(int maidSlotNo, Maid maid)
        {
            base.OnMaidChanged(maidSlotNo, maid);
        }

        protected override void OnAnmChanged(int maidSlotNo, string anmName)
        {
            base.OnAnmChanged(maidSlotNo, anmName);
        }

        public override void Update()
        {
            base.Update();
        }
    }
}