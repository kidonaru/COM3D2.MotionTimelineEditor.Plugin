using UnityEngine;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace COM3D2.MotionTimelineEditor.Plugin
{
    [System.Serializable]
    public class StageLightInfo
    {
        public float spotAngle = 10f;
        public float spotRange = 10f;
        public float rangeMultiplier = 0.8f;
        public float falloffExp = 0.5f;
        public float noiseStrength = 0.2f;
        public float noiseScale = 5f;
        public float coreRadius = 0.2f;
        public float offsetRange = 0.5f;
        public int segmentAngle = 10;
        public int segmentRange = 10;
        public bool zTest = true;
    }

    [ExecuteInEditMode]
    public class StageLightController : MonoBehaviour
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
        public List<StageLight> lights = new List<StageLight>();

        [Header("一括位置設定")]
        public bool autoVisible = false;
        public bool visible = true;

        [Header("一括位置設定")]
        public bool autoPosition = false;
        public Vector3 positionMin = new Vector3(-5f, 10f, 0f);
        public Vector3 positionMax = new Vector3(5f, 10f, 0f);

        [Header("一括回転設定")]
        public bool autoRotation = false;
        public Vector3 rotationMin = new Vector3(90f, 0f, 0f);
        public Vector3 rotationMax = new Vector3(90f, 0f, 0f);

        [Header("一括色設定")]
        public bool autoColor = false;
        public Color colorMin = new Color(1f, 1f, 1f, 0.3f);
        public Color colorMax = new Color(1f, 1f, 1f, 0.3f);

        [Header("一括ライト情報調整")]
        public bool autoLightInfo = false;
        public StageLightInfo lightInfo = new StageLightInfo();

        public enum PatternType
        {
            None,
            FadeAndMove,
        }

        [Header("動作パターン")]
        public PatternType patternType = PatternType.None;
        public float patternCycleTime = 5f;

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
            UpdateLights();
        }

        void OnDestroy()
        {
            foreach (var light in lights)
            {
                if (light == null) continue;
                DestroyImmediate(light.gameObject);
            }
        }

        void LateUpdate()
        {
            if (isManualUpdate) return;
            UpdateLights();
        }

        public void Initialize()
        {
            lights = GetComponentsInChildren<StageLight>().ToList();
            lights.Sort((a, b) => a.index - b.index);

            UpdateName();
        }

        public string AddLight()
        {
            var index = lights.Count;
            var light = new GameObject("StageLight").AddComponent<StageLight>();
            light.controller = this;
            light.index = index;
            light.transform.SetParent(transform);
            lights.Add(light);

            return light.name;
        }

        public string RemoveLight()
        {
            if (lights.Count == 0) return "";

            var index = lights.Count - 1;
            var name = lights[index].name;
            DestroyImmediate(lights[index].gameObject);
            lights.RemoveAt(index);

            return name;
        }

        void UpdateName()
        {
            var suffix = " (" + groupIndex + ")";
            name = "StageLightController" + suffix;
            displayName = "コントローラー" + suffix;
        }

        public void UpdateLights()
        {
            if (lights.Count == 0) return;

            int count = lights.Count;
            for (int i = 0; i < count; i++)
            {
                var light = lights[i];
                if (light == null) continue;

                // 0.0から1.0の間で均等に分布する値を計算
                float t = count > 1 ? (float)i / (count - 1) : 0.5f;

                if (autoVisible)
                {
                    light.visible = visible;
                }

                if (autoPosition)
                {
                    light.position = new Vector3(
                        Mathf.Lerp(positionMin.x, positionMax.x, t),
                        Mathf.Lerp(positionMin.y, positionMax.y, t),
                        Mathf.Lerp(positionMin.z, positionMax.z, t)
                    );
                }

                if (autoRotation)
                {
                    Vector3 rotation = new Vector3(
                        Mathf.Lerp(rotationMin.x, rotationMax.x, t),
                        Mathf.Lerp(rotationMin.y, rotationMax.y, t),
                        Mathf.Lerp(rotationMin.z, rotationMax.z, t)
                    );
                    light.eulerAngles = rotation;
                }

                if (autoColor)
                {
                    light.color = Color.Lerp(colorMin, colorMax, t);
                }

                if (autoLightInfo)
                {
                    light.spotAngle = lightInfo.spotAngle;
                    light.spotRange = lightInfo.spotRange;
                    light.rangeMultiplier = lightInfo.rangeMultiplier;
                    light.falloffExp = lightInfo.falloffExp;
                    light.noiseStrength = lightInfo.noiseStrength;
                    light.noiseScale = lightInfo.noiseScale;
                    light.coreRadius = lightInfo.coreRadius;
                    light.offsetRange = lightInfo.offsetRange;
                    light.segmentAngle = lightInfo.segmentAngle;
                    light.segmentRange = lightInfo.segmentRange;
                    light.zTest = lightInfo.zTest;
                }
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(StageLightController))]
    public class StageLightControllerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            StageLightController controller = (StageLightController)target;

            if (GUILayout.Button("ライト追加"))
            {
                controller.AddLight();
            }

            if (GUILayout.Button("ライト削除"))
            {
                controller.RemoveLight();
            }
        }
    }
#endif
}