using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class MaidFollowSubCamera : MonoBehaviour
    {
        public Transform targetTransform;
        public int maidSlotNo = -1;
        public Vector3 offset = Vector3.zero;

        protected static MaidManager maidManager => MaidManager.instance;

        public MaidCache maidCache
        {
            get
            {
                return maidManager.GetMaidCache(maidSlotNo);
            }
        }

        public Maid maid
        {
            get
            {
                if (maidCache != null)
                {
                    return maidCache.maid;
                }
                return null;
            }
        }

        private static StudioHackBase studioHack => StudioHackManager.instance.studioHack;

        public bool isFollow
        {
            get
            {
                return maid != null;
            }
        }

        private void LateUpdate()
        {
            if (studioHack == null)
            {
                return;
            }

            if (isFollow && maid != null && targetTransform != null)
            {
                targetTransform.position = maid.body0.Pelvis.position + offset;
            }
        }
    }

    public class SubCameraData
    {
        public string name;
        public string displayName;
        public Camera camera;
        public Rect viewportRect;

        private MaidFollowSubCamera _follow = null;
        public MaidFollowSubCamera follow
        {
            get
            {
                if (_follow == null)
                {
                    _follow = camera.gameObject.GetOrAddComponent<MaidFollowSubCamera>();
                }
                _follow.targetTransform = camera.transform;
                return _follow;
            }
        }

        public bool visible
        {
            get => camera != null && camera.enabled;
            set
            {
                if (camera != null)
                {
                    camera.enabled = value;
                }
            }
        }

        // 追従中はオフセット、非追従中はワールド座標として扱う
        public Vector3 position
        {
            get
            {
                if (follow.isFollow)
                {
                    return follow.offset;
                }
                return camera.transform.position;
            }
            set
            {
                if (follow.isFollow)
                {
                    follow.offset = value;
                }
                else
                {
                    camera.transform.position = value;
                }
            }
        }

        public Quaternion rotation
        {
            get => camera.transform.rotation;
            set => camera.transform.rotation = value;
        }

        public SubCameraData(string name, string displayName, Camera camera, Rect viewportRect)
        {
            this.name = name;
            this.displayName = displayName;
            this.camera = camera;
            this.viewportRect = viewportRect;
        }

        public void ApplyViewport(Rect rect)
        {
            viewportRect = rect;
            if (camera != null)
            {
                camera.rect = rect;
            }
        }
    }

    public class SubCameraManager : ManagerBase
    {
        public const int InitialSubCameraCount = 1;
        public const int MinSubCameraCount = 1;
        public const int MaxSubCameraCount = 8;
        private const string CameraNamePrefix = "SubCamera";
        public static readonly Rect DefaultViewport = new Rect(0.75f, 0f, 0.25f, 0.25f);

        private List<SubCameraData> _subCameras = new List<SubCameraData>();
        private Dictionary<string, SubCameraData> _subCameraMap = new Dictionary<string, SubCameraData>();
        public List<string> subCameraNames = new List<string>();

        public List<SubCameraData> subCameras => _subCameras;

        public static event UnityAction<SubCameraData> onCameraAdded;
        public static event UnityAction<SubCameraData> onCameraRemoved;
        public static event UnityAction<SubCameraData> onCameraUpdated;

        private static SubCameraManager _instance = null;
        public static SubCameraManager instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new SubCameraManager();
                }
                return _instance;
            }
        }

        private SubCameraManager()
        {
        }

        public static string GetCameraName(int index)
        {
            return CameraNamePrefix + (index + 1);
        }

        public static string GetCameraDisplayName(int index)
        {
            return "サブカメラ" + (index + 1);
        }

        public override void OnPluginDisable()
        {
            base.OnPluginDisable();
            DestroyAllCameras();
        }

        public void SetupCameras()
        {
            while (_subCameras.Count < InitialSubCameraCount)
            {
                if (AddNewCamera() == null)
                {
                    break;
                }
            }
        }

        public SubCameraData GetOrCreateCamera(string name)
        {
            var cameraData = GetCamera(name);
            if (cameraData != null)
            {
                return cameraData;
            }

            // タイムライン読込時など、キーフレームのカメラ名から不足分を生成する
            int no;
            if (!name.StartsWith(CameraNamePrefix) ||
                !int.TryParse(name.Substring(CameraNamePrefix.Length), out no))
            {
                return null;
            }

            // 外部データ由来の名前で無制限に生成されないよう上限を設ける
            if (no < 1 || no > MaxSubCameraCount)
            {
                MTEUtils.LogWarning("サブカメラ番号が不正のため生成をスキップしました name={0}", name);
                return null;
            }

            while (_subCameras.Count < no)
            {
                if (AddNewCamera() == null)
                {
                    return null;
                }
            }
            return GetCamera(name);
        }

        public SubCameraData AddNewCamera()
        {
            if (_subCameras.Count >= MaxSubCameraCount)
            {
                MTEUtils.LogWarning("サブカメラ数が上限に達しています max={0}", MaxSubCameraCount);
                return null;
            }

            var index = _subCameras.Count;
            var cameraData = CreateSubCamera(
                GetCameraName(index), GetCameraDisplayName(index), DefaultViewport);

            if (cameraData != null && onCameraAdded != null)
            {
                onCameraAdded(cameraData);
            }
            return cameraData;
        }

        public void RemoveLastCamera()
        {
            if (_subCameras.Count <= MinSubCameraCount)
            {
                return;
            }

            var cameraData = _subCameras[_subCameras.Count - 1];

            _subCameras.Remove(cameraData);
            _subCameraMap.Remove(cameraData.name);
            subCameraNames.Remove(cameraData.name);

            if (cameraData.camera != null)
            {
                Object.Destroy(cameraData.camera.gameObject);
            }

            if (onCameraRemoved != null)
            {
                onCameraRemoved(cameraData);
            }
        }

        public void DestroyAllCameras()
        {
            foreach (var cameraData in _subCameras)
            {
                if (cameraData.camera != null)
                {
                    Object.Destroy(cameraData.camera.gameObject);
                }
            }
            _subCameras.Clear();
            _subCameraMap.Clear();
            subCameraNames.Clear();
        }

        public SubCameraData GetCamera(string name)
        {
            SubCameraData camera;
            if (_subCameraMap.TryGetValue(name, out camera))
            {
                return camera;
            }
            return null;
        }

        private SubCameraData CreateSubCamera(string name, string displayName, Rect viewportRect)
        {
            var mainCam = PluginUtils.MainCamera;
            if (mainCam == null)
            {
                MTEUtils.LogWarning("メインカメラが見つからないためサブカメラを作成できません name={0}", name);
                return null;
            }

            var cameraObj = new GameObject(name);
            var newCamera = cameraObj.AddComponent<Camera>();

            newCamera.orthographic = mainCam.orthographic;
            newCamera.orthographicSize = mainCam.orthographicSize;
            newCamera.transform.position = mainCam.transform.position;
            newCamera.transform.rotation = mainCam.transform.rotation;
            newCamera.fieldOfView = mainCam.fieldOfView;
            newCamera.nearClipPlane = mainCam.nearClipPlane;
            newCamera.farClipPlane = mainCam.farClipPlane;
            newCamera.depth = mainCam.depth + 1;
            newCamera.cullingMask = mainCam.cullingMask;
            newCamera.clearFlags = mainCam.clearFlags;
            newCamera.allowHDR = mainCam.allowHDR;
            newCamera.allowMSAA = mainCam.allowMSAA;
            newCamera.rect = viewportRect;

            // 有効化はキーフレームのEnableで制御する
            newCamera.enabled = false;

            var cameraData = new SubCameraData(name, displayName, newCamera, viewportRect);

            _subCameras.Add(cameraData);
            _subCameraMap.Add(name, cameraData);
            subCameraNames.Add(name);

            return cameraData;
        }

        public void UpdateCameraViewport(string name, Rect newViewportRect)
        {
            var cameraData = GetCamera(name);
            if (cameraData != null && cameraData.camera != null)
            {
                cameraData.ApplyViewport(newViewportRect);

                if (onCameraUpdated != null)
                {
                    onCameraUpdated(cameraData);
                }
            }
        }
    }
}
