using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace COM3D2.MotionTimelineEditor.Plugin
{
    [ExecuteInEditMode]
    public class PsylliumController : MonoBehaviour
    {
        const float BPM_TO_TIME = 1 / 60f;

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

        public PsylliumBarConfig barConfig = new PsylliumBarConfig();
        public PsylliumHandConfig handConfig = new PsylliumHandConfig();
        public PsylliumAnimationConfig animationConfig = new PsylliumAnimationConfig();
        public List<PsylliumArea> areas;
        public float time;
        public bool refreshRequired;

        [SerializeField]
        private Vector3 _position = DefaultPosition;
        public Vector3 position
        {
            get
            {
                return _position;
            }
            set
            {
                if (_position == value) return;
                _position = value;
                transform.localPosition = value;
            }
        }

        [SerializeField]
        private Vector3 _eulerAngles = DefaultEulerAngles;
        public Vector3 eulerAngles
        {
            get
            {
                return _eulerAngles;
            }
            set
            {
                if (_eulerAngles == value) return;
                _eulerAngles = value;
                transform.localEulerAngles = value;
            }
        }

        [SerializeField]
        private Vector3 _scale = Vector3.one;
        public Vector3 scale
        {
            get
            {
                return _scale;
            }
            set
            {
                if (_scale == value) return;
                _scale = value;
                transform.localScale = value;
            }
        }

        public bool visible
        {
            get
            {
                return gameObject.activeSelf;
            }
            set
            {
                if (gameObject.activeSelf == value) return;
                gameObject.SetActive(value);
            }
        }

        private List<float> _animationTimes = new List<float>();

        public static Vector3 DefaultPosition = new Vector3(0f, 0f, 10f);
        public static Vector3 DefaultEulerAngles = new Vector3(0f, 180f, 0f);

        void OnEnable()
        {
            Initialize();
#if UNITY_EDITOR
            EditorApplication.update += OnEditorUpdate;
#endif
        }

        void Reset()
        {
            Initialize();
        }

        public void Initialize()
        {
            time = 0.0f;
            areas = GetComponentsInChildren<PsylliumArea>().ToList();
            areas.Sort((a, b) => a.index - b.index);

            UpdateName();
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            refreshRequired = true;
        }

        void OnDisable()
        {
            EditorApplication.update -= OnEditorUpdate;
        }

        private void OnEditorUpdate()
        {
            if (Application.isPlaying)
            {
                ManualUpdate(time + Time.deltaTime);
            }
            else
            {
                ManualUpdate((float) EditorApplication.timeSinceStartup);
            }
        }
#endif

        public void Refresh()
        {
            foreach (var area in areas)
            {
                area.Refresh();
            }

            refreshRequired = false;
        }

        public void UpdateName()
        {
            var suffix = " (" + groupIndex + ")";
            name = "PsylliumController" + suffix;
            displayName = "コントローラー" + suffix;

            barConfig.UpdateName(groupIndex);
            handConfig.UpdateName(groupIndex);
            animationConfig.UpdateName(groupIndex);
        }

        public void UpdateMaterial()
        {
            foreach (var area in areas)
            {
                area.UpdateMaterial();
            }
        }

        public PsylliumArea AddArea()
        {
            var obj = new GameObject("PsylliumArea");
            obj.transform.SetParent(this.transform, false);

            var area = obj.AddComponent<PsylliumArea>();
            area.index = areas.Count;
            area.Setup(this);

            areas.Add(area);

            return area;
        }

        public string RemoveAreaLast()
        {
            if (areas.Count == 0) return "";

            var area = areas[areas.Count - 1];
            var areaName = area.name;

            if (Application.isPlaying)
            {
                Destroy(area.gameObject);
            }
            else
            {
                DestroyImmediate(area.gameObject);
            }

            areas.RemoveAt(areas.Count - 1);

            return areaName;
        }

        public float GetAnimationTime(int randomIndex)
        {
            if (_animationTimes.Count == 0) return 0;
            return _animationTimes[randomIndex % _animationTimes.Count];
        }

        public void ManualUpdate(float time)
        {
            if (refreshRequired)
            {
                Refresh();
            }

            this.time = time;

            var randomTimeCount = animationConfig.randomTimeCount;
            var randomTime = animationConfig.randomTime;
            var randomTimePerCount = randomTime / randomTimeCount;

            _animationTimes.Clear();
            for (int i = 0; i < randomTimeCount; ++i)
            {
                float t = time - randomTime * 0.5f + i * randomTimePerCount;

                float timeRatio = animationConfig.timeRatio;
                float normalizedTime = (t * 0.5f * animationConfig.bpm * BPM_TO_TIME) % 1;

                if (normalizedTime < timeRatio)
                {
                    t = normalizedTime / timeRatio;
                    t = EasingFunctions.MoveEasing(t, animationConfig.easingType1);
                }
                else
                {
                    t = (normalizedTime - timeRatio) / (1 - timeRatio);
                    t = 1 - EasingFunctions.MoveEasing(t, animationConfig.easingType2);
                }
                _animationTimes.Add(t);
            }

            foreach (var area in areas)
            {
                area.ManualUpdate();
            }
        }

        public void CopyFrom(PsylliumController src)
        {
            groupIndex = src.groupIndex;
            position = src.position;
            eulerAngles = src.eulerAngles;
            visible = src.visible;

            barConfig.CopyFrom(src.barConfig);
            handConfig.CopyFrom(src.handConfig);
            animationConfig.CopyFrom(src.animationConfig);
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(PsylliumController))]
    public class PsylliumControllerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var controller = (PsylliumController) target;

            if (GUILayout.Button("エリア追加"))
            {
                controller.AddArea();
            }

            if (GUILayout.Button("エリア削除"))
            {
                controller.RemoveAreaLast();
            }
        }
    }
#endif
}