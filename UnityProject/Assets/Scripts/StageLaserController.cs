using UnityEngine;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace COM3D2.MotionTimelineEditor.Plugin
{
    [System.Serializable]
    public class StageLaserInfo
    {
        public float laserRange = 13f;
        public float laserWidth = 0.05f;
        public float falloffExp = 0.2f;
        public float noiseStrength = 0.2f;
        public float noiseScale = 5f;
        public float coreRadius = 0f;
        public float offsetRange = 0f;
        public float glowWidth = 0.1f;
        public int segmentRange = 10;
        public bool zTest = true;
        public float intensity = 1f;

        public void CopyFrom(StageLaserInfo other)
        {
            laserRange = other.laserRange;
            laserWidth = other.laserWidth;
            falloffExp = other.falloffExp;
            noiseStrength = other.noiseStrength;
            noiseScale = other.noiseScale;
            coreRadius = other.coreRadius;
            offsetRange = other.offsetRange;
            glowWidth = other.glowWidth;
            segmentRange = other.segmentRange;
            zTest = other.zTest;
            intensity = other.intensity;
        }
    }

    [ExecuteInEditMode]
    public class StageLaserController : MonoBehaviour
    {
        [SerializeField]
        private int _groupIndex = 0;
        public int groupIndex
        {
            get
            {
                return _groupIndex;
            }
            set
            {
                if (_groupIndex == value) return;
                _groupIndex = value;
                UpdateName();
            }
        }

        public string displayName;
        public List<StageLaser> lasers = new List<StageLaser>();

        [Header("一括位置設定")]
        public bool autoVisible = true;
        public bool visible = true;

        [Header("一括位置設定")]
        public bool autoPosition = true;
        public Vector3 position = StageLaser.DefaultPosition;


        [Header("一括回転設定")]
        public bool autoRotation = true;
        public Vector3 rotationMin = new Vector3(-15f, 40f, 0f);
        public Vector3 rotationMax = new Vector3(-15f, -40f, 0f);

        [Header("一括色設定")]
        public bool autoColor = true;
        public Color color1 = StageLaser.DefaultColor1;
        public Color color2 = StageLaser.DefaultColor2;

        [Header("一括ライト情報調整")]
        public bool autoLaserInfo = true;
        public StageLaserInfo laserInfo = new StageLaserInfo();

        public bool isManualUpdate = false;

        void OnEnable()
        {
            Initialize();
        }

        void Reset()
        {
            Initialize();
        }

        void OnValidate()
        {
            UpdateLasers();
        }

        void OnDestroy()
        {
            foreach (var laser in lasers)
            {
                if (laser == null) continue;
                DestroyImmediate(laser.gameObject);
            }
        }

        void LateUpdate()
        {
            if (isManualUpdate) return;
            UpdateLasers();
        }

        public void Initialize()
        {
            lasers = GetComponentsInChildren<StageLaser>().ToList();
            lasers.Sort((a, b) => a.index - b.index);

            UpdateName();
        }

        public string AddLaser()
        {
            var index = lasers.Count;
            var laser = new GameObject("StageLaser").AddComponent<StageLaser>();
            laser.controller = this;
            laser.index = index;
            laser.transform.SetParent(transform);
            laser.position = StageLaser.DefaultPosition;
            laser.eulerAngles = StageLaser.DefaultEulerAngles;
            lasers.Add(laser);

            return laser.name;
        }

        public string RemoveLaser()
        {
            if (lasers.Count == 0) return "";

            var index = lasers.Count - 1;
            var name = lasers[index].name;
            DestroyImmediate(lasers[index].gameObject);
            lasers.RemoveAt(index);

            return name;
        }

        void UpdateName()
        {
            var suffix = " (" + groupIndex + ")";
            name = "StageLaserController" + suffix;
            displayName = "コントローラー" + suffix;
        }

        public void UpdateLasers()
        {
            if (lasers.Count == 0) return;

            int count = lasers.Count;
            for (int i = 0; i < count; i++)
            {
                var laser = lasers[i];
                if (laser == null) continue;

                // 0.0から1.0の間で均等に分布する値を計算
                float t = count > 1 ? (float)i / (count - 1) : 0.5f;

                if (autoVisible)
                {
                    laser.visible = visible;
                }

                if (autoPosition)
                {
                    laser.position = position;
                }

                if (autoRotation)
                {
                    Vector3 rotation = new Vector3(
                        Mathf.Lerp(rotationMin.x, rotationMax.x, t),
                        Mathf.Lerp(rotationMin.y, rotationMax.y, t),
                        Mathf.Lerp(rotationMin.z, rotationMax.z, t)
                    );
                    laser.eulerAngles = rotation;
                }

                if (autoColor)
                {
                    laser.color1 = color1;
                    laser.color2 = color2;
                }

                if (autoLaserInfo)
                {
                    laser.laserRange = laserInfo.laserRange;
                    laser.laserWidth = laserInfo.laserWidth;
                    laser.falloffExp = laserInfo.falloffExp;
                    laser.noiseStrength = laserInfo.noiseStrength;
                    laser.noiseScale = laserInfo.noiseScale;
                    laser.coreRadius = laserInfo.coreRadius;
                    laser.offsetRange = laserInfo.offsetRange;
                    laser.glowWidth = laserInfo.glowWidth;
                    laser.segmentRange = laserInfo.segmentRange;
                    laser.zTest = laserInfo.zTest;
                    laser.intensity = laserInfo.intensity;
                }
            }
        }

        public void CopyFrom(StageLaserController other)
        {
            if (other == null) return;
            autoVisible = other.autoVisible;
            visible = other.visible;
            autoPosition = other.autoPosition;
            position = other.position;
            autoRotation = other.autoRotation;
            rotationMin = other.rotationMin;
            rotationMax = other.rotationMax;
            autoColor = other.autoColor;
            color1 = other.color1;
            color2 = other.color2;
            autoLaserInfo = other.autoLaserInfo;
            laserInfo.CopyFrom(other.laserInfo);
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(StageLaserController))]
    public class StageLaserControllerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            StageLaserController controller = (StageLaserController)target;

            if (GUILayout.Button("レーザー追加"))
            {
                controller.AddLaser();
            }

            if (GUILayout.Button("レーザー削除"))
            {
                controller.RemoveLaser();
            }
        }
    }
#endif
}