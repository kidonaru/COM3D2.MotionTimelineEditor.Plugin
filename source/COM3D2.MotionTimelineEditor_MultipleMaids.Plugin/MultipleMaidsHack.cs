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
using COM3D2.MotionTimelineEditor;

namespace COM3D2.MotionTimelineEditor_MultipleMaids.Plugin
{
    using AttachPoint = PhotoTransTargetObject.AttachPoint;

    public class MultipleMaidsHack : StudioHackBase
    {
        private MultipleMaidsWrapper multipleMaids = new MultipleMaidsWrapper();

        public override string pluginName => "MultipleMaids";

        public override int priority => 100;

        private bool isIK
        {
            get
            {
                var isIKArray = multipleMaids.isIK;
                var selectMaidIndex = multipleMaids.selectMaidIndex;
                if (selectMaidIndex >= 0 && selectMaidIndex < isIKArray.Length)
                {
                    return isIKArray[selectMaidIndex];
                }

                return false;
            }
            set
            {
                var isIKArray = multipleMaids.isIK;
                var selectMaidIndex = multipleMaids.selectMaidIndex;
                if (selectMaidIndex >= 0 && selectMaidIndex < isIKArray.Length)
                {
                    isIKArray[selectMaidIndex] = value;
                }
            }
        }

        private bool isLock
        {
            get
            {
                var isLockArray = multipleMaids.isLock;
                var selectMaidIndex = multipleMaids.selectMaidIndex;
                if (selectMaidIndex >= 0 && selectMaidIndex < isLockArray.Length)
                {
                    return isLockArray[selectMaidIndex];
                }

                return false;
            }
            set
            {
                var isLockArray = multipleMaids.isLock;
                var selectMaidIndex = multipleMaids.selectMaidIndex;
                if (selectMaidIndex >= 0 && selectMaidIndex < isLockArray.Length)
                {
                    isLockArray[selectMaidIndex] = value;
                }
            }
        }

        private bool isStop
        {
            get
            {
                var isStopArray = multipleMaids.isStop;
                var selectMaidIndex = multipleMaids.selectMaidIndex;
                if (selectMaidIndex >= 0 && selectMaidIndex < isStopArray.Length)
                {
                    return isStopArray[selectMaidIndex];
                }

                return false;
            }
            set
            {
                var isStopArray = multipleMaids.isStop;
                for (int i = 0; i < isStopArray.Length; i++)
                {
                    isStopArray[i] = value;
                }

                maidManager.SetAnmEnabledAll(!value);
            }
        }

        public bool isBone
        {
            get
            {
                var isBoneArray = multipleMaids.isBone;
                var selectMaidIndex = multipleMaids.selectMaidIndex;
                if (selectMaidIndex >= 0 && selectMaidIndex < isBoneArray.Length)
                {
                    return isBoneArray[selectMaidIndex];
                }

                return false;
            }
            set
            {
                var isBoneArray = multipleMaids.isBone;
                var selectMaidIndex = multipleMaids.selectMaidIndex;
                if (selectMaidIndex >= 0 && selectMaidIndex < isBoneArray.Length)
                {
                    isBoneArray[selectMaidIndex] = value;
                }
            }
        }

        public override Maid selectedMaid
        {
            get
            {
                var maidArray = multipleMaids.maidArray;
                var selectMaidIndex = multipleMaids.selectMaidIndex;
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
                foreach (var maid in multipleMaids.maidArray)
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

                foreach (var dogu in multipleMaids.doguBObject)
                {
                    if (dogu != null)
                    {
                        AttachPoint attachPoint = AttachPoint.Null;
                        int attachMaidSlotNo = -1;

                        var parent = dogu.transform.parent;
                        if (parent != null && BoneUtils.IsDefaultBoneName(parent.name))
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
                            attachMaidSlotNo,
                            dogu,
                            pluginName,
                            dogu.activeSelf);
                        _modelList.Add(model);
                    }
                }

                return _modelList;
            }
        }

        private List<StudioLightStat> _lightList = new List<StudioLightStat>();
        public override List<StudioLightStat> lightList
        {
            get
            {
                _lightList.Clear();

                int index = 0;
                foreach (var lightObj in multipleMaids.lightList)
                {
                    var light = lightObj.GetComponent<Light>();
                    if (light == null)
                    {
                        continue;
                    }

                    var transform = lightObj.transform;
                    var stat = new StudioLightStat(light, transform, lightObj, index++);
                    _lightList.Add(stat);
                }

                return _lightList;
            }
        }

        public override int selectedMaidSlotNo
        {
            get => multipleMaids.selectMaidIndex;
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

        public override bool isPoseEditing
        {
            get => isLock;
            set
            {
                MTEUtils.LogDebug("isPoseEditing: {0} -> {1}", isPoseEditing, value);
                if (value && isAnmPlaying)
                {
                    isAnmPlaying = false;
                }

                isLock = value;
                isBone = value;
                multipleMaids.unLockFlg = value;
            }
        }

        public override bool isIKVisible
        {
            get => isIK;
            set => isIK = value;
        }

        public override bool isAnmEnabled
        {
            get => !isStop;
            set
            {
                MTEUtils.LogDebug("isAnmEnabled: {0} -> {1}", isAnmEnabled, value);
                isStop = !value;
            }
        }

        public override float motionSliderRate
        {
            set { } // do nothing
        }

        public override bool useMuneKeyL
        {
            set { } // do nothing
        }

        public override bool useMuneKeyR
        {
            set { } // do nothing
        }

        public override Camera subCamera => multipleMaids.subcamera;

        public override bool isUIVisible
        {
            get => multipleMaids.bGui;
            set => multipleMaids.bGui = value;
        }

        public override DepthOfFieldScatter depthOfField
        {
            get => multipleMaids.depth_field_;
        }

        public MultipleMaidsHack()
        {
        }

        public override bool Init()
        {
            MTEUtils.Log("MultipleMaidsHack: 初期化中...");

            if (!base.Init())
            {
                return false;
            }

            if (!multipleMaids.Init())
            {
                return false;
            }

            return true;
        }

        public override void ChangeMaid(Maid maid)
        {
            multipleMaids.selectMaidIndex = allMaids.IndexOf(maid);
        }

        public override void OnChangedSceneLevel(Scene scene, LoadSceneMode sceneMode)
        {
            base.OnChangedSceneLevel(scene, sceneMode);
            isSceneActive = scene.name == "SceneEdit" || scene.name == "SceneDaily";
        }

        public override bool IsValid()
        {
            if (!base.IsValid())
            {
                return false;
            }

            if (!multipleMaids.okFlg)
            {
                _errorMessage = "複数メイドを有効化してください";
                return false;
            }

            return true;
        }

        public GameObject LoadGameModel(string assetName)
        {
            var objIndex = -1;

            if (assetName.IndexOf(":") >= 0)
            {
                string[] splited = assetName.Split(new char[] { ':' });
                if (splited.Length == 2)
                {
                    assetName = splited[0];
                    objIndex = int.Parse(splited[1]);
                }
            }

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
                return LoadModObject(assetName);
            }

            var obj = UnityEngine.Object.Instantiate(sourceObj);

            if (objIndex != -1)
            {
                var index = 0;
                foreach (object child in obj.transform)
                {
                    var transform = (Transform)child;
                    if (index++ == objIndex)
                    {
                        transform.parent = null;
                        UnityEngine.Object.Destroy(obj);
                        obj.SetActive(false);

                        obj = transform.gameObject;
                        break;
                    }
                }
            }

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

            ParticleSystem[] particles = obj.GetComponentsInChildren<ParticleSystem>();
            foreach (ParticleSystem particle in particles)
            {
                if (particle != null)
                {
                    var main = particle.main;
                    main.loop = true;
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
                MTEUtils.AssertNull(methodProcScriptBin != null, "methodProcScriptBin is null");
            }

            return (string[]) methodProcScriptBin.Invoke(null, new object[] { maid, cd, filename, f_bTemp });
        }

        private GameObject LoadModObject(string assetName)
        {
            if (!assetName.EndsWith(".menu", StringComparison.Ordinal))
            {
                assetName += ".menu";
            }

            byte[] menuBytes = null;
            using (var file = GameUty.FileOpen(assetName, null))
            {
                if (!file.IsValid())
                {
                    MTEUtils.LogError("メニューファイルが存在しません。 :" + assetName);
                    return null;
                }
                menuBytes = new byte[file.GetSize()];
                file.Read(ref menuBytes, file.GetSize());
            }

            var maid = selectedMaid;
            if (maid == null)
            {
                MTEUtils.LogError("LoadModObject: maid is null");
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
            multipleMaids.doguBObject.Add(obj);

            multipleMaids.doguCnt = multipleMaids.doguBObject.Count - 1;
            var doguCnt = multipleMaids.doguCnt;
            var gDogu = multipleMaids.gDogu;
            var mDogu = multipleMaids.mDogu;

            gDogu[doguCnt] = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Vector3 localEulerAngles = obj.transform.localEulerAngles;
            gDogu[doguCnt].transform.localEulerAngles = obj.transform.localEulerAngles;
            gDogu[doguCnt].transform.position = obj.transform.position;
            gDogu[doguCnt].GetComponent<Renderer>().material = multipleMaids.m_material;
            gDogu[doguCnt].layer = 8;
            gDogu[doguCnt].GetComponent<Renderer>().enabled = false;
            gDogu[doguCnt].SetActive(false);
            gDogu[doguCnt].transform.position = obj.transform.position;
            mDogu[doguCnt] = gDogu[doguCnt].AddComponent<MouseDrag6>();
            mDogu[doguCnt].obj = gDogu[doguCnt];
            mDogu[doguCnt].maid = obj;
            mDogu[doguCnt].angles = localEulerAngles;
            gDogu[doguCnt].transform.localScale = new Vector3(multipleMaids.cubeSize, multipleMaids.cubeSize, multipleMaids.cubeSize);
            mDogu[doguCnt].ido = 1;
            mDogu[doguCnt].isScale = false;

            if (assetName == "Particle/pLineY")
            {
                mDogu[doguCnt].count = 180;
            }
            if (assetName == "Particle/pLineP02")
            {
                mDogu[doguCnt].count = 115;
            }
            if (obj.name == "Particle/pLine_act2")
            {
                mDogu[doguCnt].count = 80;
                obj.transform.localScale = new Vector3(3f, 3f, 3f);
            }
            if (obj.name == "Particle/pHeart01")
            {
                mDogu[doguCnt].count = 80;
            }
            if (assetName == "mirror1" || assetName == "mirror2" || assetName == "mirror3")
            {
                mDogu[doguCnt].isScale = true;
                mDogu[doguCnt].isScale2 = true;
                mDogu[doguCnt].scale2 = obj.transform.localScale;
                if (assetName == "mirror1")
                {
                    mDogu[doguCnt].scale = new Vector3(obj.transform.localScale.x * 5f, obj.transform.localScale.y * 5f, obj.transform.localScale.z * 5f);
                }
                if (assetName == "mirror2")
                {
                    mDogu[doguCnt].scale = new Vector3(obj.transform.localScale.x * 10f, obj.transform.localScale.y * 10f, obj.transform.localScale.z * 10f);
                }
                if (assetName == "mirror3")
                {
                    mDogu[doguCnt].scale = new Vector3(obj.transform.localScale.x * 33f, obj.transform.localScale.y * 33f, obj.transform.localScale.z * 33f);
                }
            }
            if (assetName == "Odogu_XmasTreeMini_photo_ver" || assetName == "Odogu_KadomatsuMini_photo_ver")
            {
                mDogu[doguCnt].isScale2 = true;
                mDogu[doguCnt].scale2 = obj.transform.localScale;
            }
        }

        public override void DeleteAllModels()
        {
            for (int l = 0; l < multipleMaids.doguBObject.Count; l++)
            {
                UnityEngine.Object.Destroy(multipleMaids.doguBObject[l]);
            }
            multipleMaids.doguBObject.Clear();
            multipleMaids.doguCnt = 0;
        }

        public override void DeleteModel(StudioModelStat model)
        {
            var doguBObject = multipleMaids.doguBObject;
            var index = doguBObject.FindIndex(d => d.transform == model.transform);
            if (index >= 0)
            {
                UnityEngine.Object.Destroy(doguBObject[index]);
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
                    MTEUtils.LogError("CreateModel: モデルの追加に失敗しました" + model.name);
                    return;
                }

                model.transform = obj.transform;
                model.obj = obj;

                UpdateAttachPoint(model);
            }
            catch (Exception e)
            {
                MTEUtils.LogException(e);
            }
        }

        public override void UpdateAttachPoint(StudioModelStat model)
        {
            var attachPoint = model.attachPoint;
            var attachMaidSlotNo = model.attachMaidSlotNo;
            var maidCache = maidManager.GetMaidCache(attachMaidSlotNo);
            AttachItem(maidCache, model.transform, attachPoint, false);
        }

        public void AttachItem(MaidCache maidCache, Transform item, AttachPoint point, bool keepWorldPosition)
        {
            Transform parent = maidCache?.GetAttachPointTransform(point);
            Quaternion quaternion = item.rotation;
            Vector3 localScale = item.localScale;

            item.SetParent(parent, keepWorldPosition);
            if (keepWorldPosition)
            {
                item.rotation = quaternion;
            }
            else
            {
                item.localPosition = Vector3.zero;
                item.rotation = Quaternion.identity;
            }
            item.localScale = localScale;
        }

        public override bool CanCreateLight()
        {
            return true;
        }

        public override void DeleteAllLights()
        {
            var lightList = multipleMaids.lightList;
            for (int l = 1; l < lightList.Count; l++)
            {
                UnityEngine.Object.Destroy(lightList[l]);
            }
            lightList.RemoveAllButFirst();

            multipleMaids.lightColorR.RemoveAllButFirst();
            multipleMaids.lightColorG.RemoveAllButFirst();
            multipleMaids.lightColorB.RemoveAllButFirst();
            multipleMaids.lightIndex.RemoveAllButFirst();
            multipleMaids.lightX.RemoveAllButFirst();
            multipleMaids.lightY.RemoveAllButFirst();
            multipleMaids.lightAkarusa.RemoveAllButFirst();
            multipleMaids.lightKage.RemoveAllButFirst();
            multipleMaids.lightRange.RemoveAllButFirst();
            multipleMaids.selectLightIndex = 0;
            UpdateLightCombo();
        }

        public override void DeleteLight(StudioLightStat light)
        {
            var lightObj = light.obj as GameObject;
            var lightList = multipleMaids.lightList;
            var index = lightList.FindIndex(d => d == lightObj);
            if (index > 0)
            {
                UnityEngine.Object.Destroy(lightList[index]);
                multipleMaids.lightList.RemoveAt(index);
                multipleMaids.lightColorR.RemoveAt(index);
                multipleMaids.lightColorG.RemoveAt(index);
                multipleMaids.lightColorB.RemoveAt(index);
                multipleMaids.lightIndex.RemoveAt(index);
                multipleMaids.lightX.RemoveAt(index);
                multipleMaids.lightY.RemoveAt(index);
                multipleMaids.lightAkarusa.RemoveAt(index);
                multipleMaids.lightKage.RemoveAt(index);
                multipleMaids.lightRange.RemoveAt(index);
                multipleMaids.selectLightIndex = 0;
                UpdateLightCombo();
            }
        }

        private void UpdateLightCombo()
        {
            var lightList = multipleMaids.lightList;
            multipleMaids.lightComboList = new GUIContent[lightList.Count];
            for (int i = 0; i < lightList.Count; i++)
            {
                if (i == 0)
                {
                    multipleMaids.lightComboList[i] = new GUIContent("メイン");
                }
                else
                {
                    multipleMaids.lightComboList[i] = new GUIContent("追加" + i);
                }
            }
            multipleMaids.lightCombo.selectedItemIndex = multipleMaids.selectLightIndex;
        }

        public int ConvertLightType(LightType lightType)
        {
            switch (lightType)
            {
                case LightType.Directional:
                    return 0;
                case LightType.Spot:
                    return 1;
                case LightType.Point:
                    return 2;
                default:
                    return 0;
            }
        }

        public override void CreateLight(StudioLightStat stat)
        {
            MTEUtils.Log("CreateLight: ライトを追加します");

            var gameObject = new GameObject("Light");
			var light = gameObject.AddComponent<Light>();
            var lightList = multipleMaids.lightList;
            multipleMaids.lightList.Add(gameObject);
            multipleMaids.lightColorR.Add(1f);
            multipleMaids.lightColorG.Add(1f);
            multipleMaids.lightColorB.Add(1f);
            multipleMaids.lightIndex.Add(ConvertLightType(stat.type));
            multipleMaids.lightX.Add(40f);
            multipleMaids.lightY.Add(180f);
            multipleMaids.lightAkarusa.Add(0.95f);
            multipleMaids.lightKage.Add(0.098f);
            multipleMaids.lightRange.Add(50f);
            gameObject.transform.position = GameMain.Instance.MainLight.transform.position;
            multipleMaids.selectLightIndex = 0;
            UpdateLightCombo();

            light.intensity = 0.95f;
            light.spotAngle = 50f;
            light.range = 10f;
            light.type = LightType.Directional;
            light.color = new Color(0.5f, 1f, 0f);

            stat.light = light;
            stat.transform = gameObject.transform;
            stat.obj = gameObject;
            ChangeLight(stat);
            ApplyLight(stat);

            var gLight = multipleMaids.gLight;
            var mLight = multipleMaids.mLight;

            if (gLight[0] == null)
            {
                gLight[0] = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Material material = new Material(Shader.Find("Transparent/Diffuse"));
                material.color = new Color(0.5f, 0.5f, 1f, 0.8f);
                gLight[0].GetComponent<Renderer>().material = material;
                gLight[0].layer = 8;
                gLight[0].GetComponent<Renderer>().enabled = false;
                gLight[0].SetActive(false);
                gLight[0].transform.position = GameMain.Instance.MainLight.transform.position;
                mLight[0] = gLight[0].AddComponent<MouseDrag6>();
                mLight[0].obj = gLight[0];
                mLight[0].maid = GameMain.Instance.MainLight.gameObject;
                mLight[0].angles = GameMain.Instance.MainLight.gameObject.transform.eulerAngles;
                gLight[0].transform.localScale = new Vector3(0.12f, 0.12f, 0.12f);
                mLight[0].ido = 1;
                mLight[0].isScale = false;
            }
        }

        public override void ChangeLight(StudioLightStat stat)
        {
            var lightObj = stat.obj as GameObject;
            if (lightObj == null || stat.light == null || stat.transform == null)
            {
                MTEUtils.LogError("ChangeLight: ライトが見つかりません" + stat.name);
                return;
            }

            var lightList = multipleMaids.lightList;
            var lightIndex = lightList.FindIndex(d => d == lightObj);
            if (lightIndex < 0)
            {
                MTEUtils.LogError("ChangeLight: ライトが見つかりません" + stat.name);
                return;
            }

            var light = stat.light;
            light.type = stat.type;
        }

        public override void ApplyLight(StudioLightStat stat)
        {
            var lightObj = stat.obj as GameObject;
            if (lightObj == null || stat.light == null || stat.transform == null)
            {
                MTEUtils.LogError("ApplyLight: ライトが見つかりません" + stat.name);
                return;
            }

            var lightList = multipleMaids.lightList;
            var lightIndex = lightList.FindIndex(d => d == lightObj);
            if (lightIndex < 0)
            {
                MTEUtils.LogError("ApplyLight: ライトが見つかりません" + stat.name);
                return;
            }

            var light = stat.light;
            var transform = stat.transform;

            multipleMaids.lightColorR[lightIndex] = light.color.r;
            multipleMaids.lightColorG[lightIndex] = light.color.g;
            multipleMaids.lightColorB[lightIndex] = light.color.b;
            multipleMaids.lightX[lightIndex] = transform.eulerAngles.x;
            multipleMaids.lightY[lightIndex] = transform.eulerAngles.y;
            multipleMaids.lightAkarusa[lightIndex] = light.intensity;
            multipleMaids.lightKage[lightIndex] = light.shadowStrength;
            multipleMaids.lightRange[lightIndex] = light.spotAngle;
            light.enabled = stat.visible;
        }

        private bool mekure1
        {
            get
            {
                var mekure1Array = multipleMaids.mekure1;
                var selectMaidIndex = multipleMaids.selectMaidIndex;
                if (selectMaidIndex >= 0 && selectMaidIndex < mekure1Array.Length)
                {
                    return mekure1Array[selectMaidIndex];
                }

                return false;
            }
            set
            {
                var mekure1Array = multipleMaids.mekure1;
                for (int i = 0; i < mekure1Array.Length; i++)
                {
                    mekure1Array[i] = value;
                }
            }
        }

        private bool mekure2
        {
            get
            {
                var mekure2Array = multipleMaids.mekure2;
                var selectMaidIndex = multipleMaids.selectMaidIndex;
                if (selectMaidIndex >= 0 && selectMaidIndex < mekure2Array.Length)
                {
                    return mekure2Array[selectMaidIndex];
                }

                return false;
            }
            set
            {
                var mekure2Array = multipleMaids.mekure2;
                for (int i = 0; i < mekure2Array.Length; i++)
                {
                    mekure2Array[i] = value;
                }
            }
        }

        private bool zurasi
        {
            get
            {
                var zurasiArray = multipleMaids.zurasi;
                var selectMaidIndex = multipleMaids.selectMaidIndex;
                if (selectMaidIndex >= 0 && selectMaidIndex < zurasiArray.Length)
                {
                    return zurasiArray[selectMaidIndex];
                }

                return false;
            }
            set
            {
                var zurasiArray = multipleMaids.zurasi;
                for (int i = 0; i < zurasiArray.Length; i++)
                {
                    zurasiArray[i] = value;
                }
            }
        }

        public MouseDrag mHandL
        {
            get
            {
                var dragArray = multipleMaids.mHandL;
                var selectMaidIndex = multipleMaids.selectMaidIndex;
                if (selectMaidIndex >= 0 && selectMaidIndex < dragArray.Length)
                {
                    return dragArray[selectMaidIndex];
                }

                return null;
            }
        }

        public MouseDrag mHandR
        {
            get
            {
                var dragArray = multipleMaids.mHandR;
                var selectMaidIndex = multipleMaids.selectMaidIndex;
                if (selectMaidIndex >= 0 && selectMaidIndex < dragArray.Length)
                {
                    return dragArray[selectMaidIndex];
                }

                return null;
            }
        }

        public MouseDrag mArmL
        {
            get
            {
                var dragArray = multipleMaids.mArmL;
                var selectMaidIndex = multipleMaids.selectMaidIndex;
                if (selectMaidIndex >= 0 && selectMaidIndex < dragArray.Length)
                {
                    return dragArray[selectMaidIndex];
                }

                return null;
            }
        }

        public MouseDrag mArmR
        {
            get
            {
                var dragArray = multipleMaids.mArmR;
                var selectMaidIndex = multipleMaids.selectMaidIndex;
                if (selectMaidIndex >= 0 && selectMaidIndex < dragArray.Length)
                {
                    return dragArray[selectMaidIndex];
                }

                return null;
            }
        }

        public MouseDrag mFootL
        {
            get
            {
                var dragArray = multipleMaids.mFootL;
                var selectMaidIndex = multipleMaids.selectMaidIndex;
                if (selectMaidIndex >= 0 && selectMaidIndex < dragArray.Length)
                {
                    return dragArray[selectMaidIndex];
                }

                return null;
            }
        }

        public MouseDrag mFootR
        {
            get
            {
                var dragArray = multipleMaids.mFootR;
                var selectMaidIndex = multipleMaids.selectMaidIndex;
                if (selectMaidIndex >= 0 && selectMaidIndex < dragArray.Length)
                {
                    return dragArray[selectMaidIndex];
                }

                return null;
            }
        }

        public MouseDrag mHizaL
        {
            get
            {
                var dragArray = multipleMaids.mHizaL;
                var selectMaidIndex = multipleMaids.selectMaidIndex;
                if (selectMaidIndex >= 0 && selectMaidIndex < dragArray.Length)
                {
                    return dragArray[selectMaidIndex];
                }

                return null;
            }
        }

        public MouseDrag mHizaR
        {
            get
            {
                var dragArray = multipleMaids.mHizaR;
                var selectMaidIndex = multipleMaids.selectMaidIndex;
                if (selectMaidIndex >= 0 && selectMaidIndex < dragArray.Length)
                {
                    return dragArray[selectMaidIndex];
                }

                return null;
            }
        }

        public override void UpdateUndress(Maid maid, DressSlotID slotId, bool isVisible)
        {
            if (maid != selectedMaid)
            {
                return;
            }

            switch (slotId)
            {
                case DressSlotID.wear:
                case DressSlotID.onepiece:
                case DressSlotID.mizugi:
                    multipleMaids.isWear = isVisible;
                    break;
                case DressSlotID.skirt:
                    multipleMaids.isSkirt = isVisible;
                    break;
                case DressSlotID.bra:
                    multipleMaids.isBra = isVisible;
                    break;
                case DressSlotID.panz:
                    multipleMaids.isPanz = isVisible;
                    break;
                case DressSlotID.undressfront:
                    mekure1 = isVisible;
                    break;
                case DressSlotID.undressback:
                    mekure2 = isVisible;
                    break;
                case DressSlotID.undressshift:
                    zurasi = isVisible;
                    break;
                case DressSlotID.headset:
                case DressSlotID.accHead:
                case DressSlotID.accKami_1_:
                case DressSlotID.accKami_2_:
                case DressSlotID.accKami_3_:
                case DressSlotID.accKamiSubL:
                case DressSlotID.accKamiSubR:
                    multipleMaids.isHeadset = isVisible;
                    break;
                case DressSlotID.accUde:
                    multipleMaids.isAccUde = isVisible;
                    break;
                case DressSlotID.stkg:
                    multipleMaids.isStkg = isVisible;
                    break;
                case DressSlotID.shoes:
                    multipleMaids.isShoes = isVisible;
                    break;
                case DressSlotID.glove:
                    multipleMaids.isGlove = isVisible;
                    break;
                case DressSlotID.megane:
                    multipleMaids.isMegane = isVisible;
                    break;
                case DressSlotID.accSenaka:
                    multipleMaids.isAccSenaka = isVisible;
                    break;
            }
        }

        public override void Update()
        {
            base.Update();

            // ドラッグしたままになるバグがあるので、リセットする
            if (Input.GetMouseButtonUp(0))
            {
                var dragArray = new MouseDrag[]
                {
                    mHandL,
                    mHandR,
                    mArmL,
                    mArmR,
                    mFootL,
                    mFootR,
                    mHizaL,
                    mHizaR,
                };

                foreach (var drag in dragArray)
                {
                    if (drag != null)
                    {
                        drag.OnMouseUp();
                    }
                }
            }
        }

        public override void OnUpdateDepthOfField()
        {
            base.OnUpdateDepthOfField();

            var dof = multipleMaids.depth_field_;
            if (dof != null)
            {
                multipleMaids.isDepth = dof.enabled;
                multipleMaids.isDepthA = dof.visualizeFocus;
                multipleMaids.depth1 = dof.focalLength;
                multipleMaids.depth2 = dof.focalSize;
                multipleMaids.depth3 = dof.aperture;
                multipleMaids.depth4 = dof.maxBlurSize;
            }
        }

        public override bool IsIKDragging(IKHoldType iKHoldType)
        {
            MouseDrag drag = null;
            switch (iKHoldType)
            {
                case IKHoldType.Arm_L_Tip:
                    drag = mHandL;
                    break;
                case IKHoldType.Arm_R_Tip:
                    drag = mHandR;
                    break;
                case IKHoldType.Arm_L_Joint:
                    drag = mArmL;
                    break;
                case IKHoldType.Arm_R_Joint:
                    drag = mArmR;
                    break;
                case IKHoldType.Foot_L_Tip:
                    drag = mFootL;
                    break;
                case IKHoldType.Foot_R_Tip:
                    drag = mFootR;
                    break;
                case IKHoldType.Foot_L_Joint:
                    drag = mHizaL;
                    break;
                case IKHoldType.Foot_R_Joint:
                    drag = mHizaR;
                    break;
            }

            return drag != null && drag.isMouseDown;
        }
    }
}