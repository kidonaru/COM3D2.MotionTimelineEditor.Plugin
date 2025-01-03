﻿using System;
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
        public PsylliumAnimationHandConfig animationHandConfigLeft = new PsylliumAnimationHandConfig();
        public PsylliumAnimationHandConfig animationHandConfigRight = new PsylliumAnimationHandConfig();
        public List<PsylliumArea> areas;
        public Material[] materials;
        public Mesh[] meshes;
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
        private List<Vector3> _animationPositionsLeft = new List<Vector3>();
        private List<Vector3> _animationPositionsRight = new List<Vector3>();
        private List<Quaternion> _animationRotationsLeft = new List<Quaternion>();
        private List<Quaternion> _animationRotationsRight = new List<Quaternion>();

        private int _randomAnimationSeed = 0;
        private List<Vector3> _randomAnimationPosition1Params = new List<Vector3>();
        private List<Vector3> _randomAnimationPosition2Params = new List<Vector3>();
        private List<Vector3> _randomAnimationEulerAnglesParams = new List<Vector3>();
        private List<Vector3> _randomAnimationPositions1 = new List<Vector3>();
        private List<Vector3> _randomAnimationPositions2 = new List<Vector3>();
        private List<Quaternion> _randomAnimationRotations = new List<Quaternion>();

        public static Vector3 DefaultPosition = new Vector3(0f, 0f, 0f);
        public static Vector3 DefaultEulerAngles = new Vector3(0f, 0f, 0f);

#if COM3D2
        private static TimelineBundleManager bundleManager => TimelineBundleManager.instance;

        private Config config => ConfigManager.config;
#endif

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

        private Material CreateMaterial(string materialName)
        {
#if COM3D2
            var material = bundleManager.LoadMaterial(materialName);
#else
            var material = new Material(Shader.Find("MTE/" + materialName));
            material.SetTexture("_MainTex", Resources.Load<Texture2D>("psyllium"));
#endif
            return material;
        }

        public void Initialize()
        {
            time = 0.0f;
            areas = GetComponentsInChildren<PsylliumArea>().ToList();
            areas.Sort((a, b) => a.index - b.index);

            if (animationConfig.randomSeed == 0)
            {
                animationConfig.randomSeed = UnityEngine.Random.Range(1, int.MaxValue);
            }

            materials = new Material[2];
            materials[0] = CreateMaterial("Psyllium");
            materials[1] = CreateMaterial("PsylliumAdd");

            meshes = new Mesh[2];
            meshes[0] = new Mesh();
            meshes[1] = new Mesh();

            UpdateName();
            UpdateMaterials();
            UpdateMeshs();
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

            UpdateMaterials();
            UpdateMeshs();

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
            animationHandConfigLeft.UpdateName(groupIndex, true);
            animationHandConfigRight.UpdateName(groupIndex, false);
        }

        public void UpdateMaterials()
        {
            foreach (var material in materials)
            {
                material.SetColor(Uniforms._Color1a, barConfig.color1a);
                material.SetColor(Uniforms._Color1b, barConfig.color1b);
                material.SetColor(Uniforms._Color1c, barConfig.color1c);
                material.SetColor(Uniforms._Color2a, barConfig.color2a);
                material.SetColor(Uniforms._Color2b, barConfig.color2b);
                material.SetColor(Uniforms._Color2c, barConfig.color2c);
                material.SetFloat(Uniforms._CutoffAlpha, barConfig.cutoffAlpha);
            }
        }

        private static class Uniforms
        {
            internal static readonly int _Color1a = Shader.PropertyToID("_Color1a");
            internal static readonly int _Color1b = Shader.PropertyToID("_Color1b");
            internal static readonly int _Color1c = Shader.PropertyToID("_Color1c");
            internal static readonly int _Color2a = Shader.PropertyToID("_Color2a");
            internal static readonly int _Color2b = Shader.PropertyToID("_Color2b");
            internal static readonly int _Color2c = Shader.PropertyToID("_Color2c");
            internal static readonly int _CutoffAlpha = Shader.PropertyToID("_CutoffAlpha");
        }

        public void UpdateMeshs()
        {
            UpdateMesh(0);
            UpdateMesh(1);
        }

        public void UpdateMesh(int colorIndex)
        {
            var halfWidth = barConfig.width * 0.5f * barConfig.baseScale;
            var barHeight = barConfig.height * barConfig.baseScale;
            var barRadius = barConfig.radius * barConfig.baseScale;
            var positionY = barConfig.positionY * barConfig.baseScale;
            var barTopThreshold = barConfig.topThreshold;

            var vertices = new Vector3[] {
                new Vector3(-halfWidth, 0, 0),  // 0
                new Vector3(-halfWidth, 0, 0),  // 1
                new Vector3( halfWidth, 0, 0),  // 2
                new Vector3( halfWidth, 0, 0),  // 3
                new Vector3(-halfWidth, barHeight, 0),  // 4
                new Vector3( halfWidth, barHeight, 0),  // 5
                new Vector3(-halfWidth, barHeight, 0),  // 6 
                new Vector3( halfWidth, barHeight, 0),  // 7
            };

            for (int i = 0; i < vertices.Length; i++)
            {
                var v = vertices[i];
                v.y += positionY;
                vertices[i] = v;
            }

            var uv = new Vector2[] {
                new Vector2(0, 0),
                new Vector2(0, barTopThreshold),
                new Vector2(1, 0),
                new Vector2(1, barTopThreshold),
                new Vector2(0, 1 - barTopThreshold),
                new Vector2(1, 1 - barTopThreshold),
                new Vector2(0, 1),
                new Vector2(1, 1),
            };

            var uv2 = new Vector2[] {
                new Vector2(-barRadius, 0),
                new Vector2(0, 0),
                new Vector2(-barRadius, 0),
                new Vector2(0, 0),
                new Vector2(0, 0),
                new Vector2(0, 0),
                new Vector2(barRadius, 0),
                new Vector2(barRadius, 0),
            };

            for (int i = 0; i < uv2.Length; i++)
            {
                var v = uv2[i];
                v.y = colorIndex;
                uv2[i] = v;
            }

            var triangles = new int[] {
                0, 1, 2,
                1, 3, 2,
                1, 4, 3,
                4, 5, 3,
                4, 6, 5,
                6, 7, 5,
            };

            var mesh = meshes[colorIndex];
            mesh.Clear();

            mesh.subMeshCount = 2;
            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.uv2 = uv2;

            mesh.SetTriangles(triangles, 0);
            mesh.SetTriangles(triangles, 1);
        }

        public PsylliumArea AddArea()
        {
            var obj = new GameObject("PsylliumArea");
            obj.transform.SetParent(this.transform, false);

            var area = obj.AddComponent<PsylliumArea>();
            area.index = areas.Count;
            area.Setup(this);

            areas.Add(area);

#if COM3D2
            // 1つ前のエリア設定を引き継ぐ
            if (areas.Count > 1)
            {
                var prevArea = areas[areas.Count - 2];
                area.CopyFrom(prevArea, false);
            }
#endif

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

        private int CalcLoopIndex(int index, int count)
        {
            var loopIndex = index % (count * 2);
            if (loopIndex >= count) loopIndex = count * 2 - 1 - loopIndex;
            return loopIndex;
        }

        public float GetAnimationTime(int randomIndex)
        {
            if (_animationTimes.Count == 0) return 0;
            return _animationTimes[CalcLoopIndex(randomIndex, _animationTimes.Count)];
        }

        public Vector3 GetAnimationPosition(int randomIndex, bool isLeftHand)
        {
            if (isLeftHand)
            {
                if (_animationPositionsLeft.Count == 0) return Vector3.zero;
                return _animationPositionsLeft[CalcLoopIndex(randomIndex, _animationPositionsLeft.Count)];
            }
            else
            {
                if (_animationPositionsRight.Count == 0) return Vector3.zero;
                return _animationPositionsRight[CalcLoopIndex(randomIndex, _animationPositionsRight.Count)];
            }
        }

        public Quaternion GetAnimationRotation(int randomIndex, bool isLeftHand)
        {
            if (isLeftHand)
            {
                if (_animationRotationsLeft.Count == 0) return Quaternion.identity;
                return _animationRotationsLeft[CalcLoopIndex(randomIndex, _animationRotationsLeft.Count)];
            }
            else
            {
                if (_animationRotationsRight.Count == 0) return Quaternion.identity;
                return _animationRotationsRight[CalcLoopIndex(randomIndex, _animationRotationsRight.Count)];
            }
        }

        public Vector3 GetRandomAnimationPosition1(int randomIndex)
        {
            if (_randomAnimationPositions1.Count == 0) return Vector3.zero;
            return _randomAnimationPositions1[CalcLoopIndex(randomIndex, _randomAnimationPositions1.Count)];
        }

        public Vector3 GetRandomAnimationPosition2(int randomIndex)
        {
            if (_randomAnimationPositions2.Count == 0) return Vector3.zero;
            return _randomAnimationPositions2[CalcLoopIndex(randomIndex, _randomAnimationPositions2.Count)];
        }

        public Vector3 GetRandomAnimationPosition(int randomIndex, int timeIndex)
        {
            var position1 = GetRandomAnimationPosition1(randomIndex);
            var position2 = GetRandomAnimationPosition2(randomIndex);
            return Vector3.Lerp(position1, position2, GetAnimationTime(timeIndex));
        }

        public Quaternion GetRandomAnimationRotation(int randomIndex)
        {
            if (_randomAnimationRotations.Count == 0) return Quaternion.identity;
            return _randomAnimationRotations[CalcLoopIndex(randomIndex, _randomAnimationRotations.Count)];
        }

        public void ManualUpdate(float time)
        {
            if (refreshRequired)
            {
                Refresh();
            }

            this.time = time;

            var patternCount = animationConfig.patternCount;
            var randomTime = animationConfig.randomTime;
            var randomTimePerCount = randomTime / patternCount;

            float timeRatio = animationConfig.timeRatio;
            float timeOffset = animationConfig.timeOffset;

            _animationTimes.Clear();
            _animationPositionsLeft.Clear();
            _animationPositionsRight.Clear();
            _animationRotationsLeft.Clear();
            _animationRotationsRight.Clear();

            var positionLeft1 = animationHandConfigLeft.position1 * barConfig.baseScale;
            var positionLeft2 = animationHandConfigLeft.position2 * barConfig.baseScale;
            var positionRight1 = animationHandConfigRight.position1 * barConfig.baseScale;
            var positionRight2 = animationHandConfigRight.position2 * barConfig.baseScale;
            var rotationLeft1 = Quaternion.Euler(animationHandConfigLeft.eulerAngles1);
            var rotationLeft2 = Quaternion.Euler(animationHandConfigLeft.eulerAngles2);
            var rotationRight1 = Quaternion.Euler(animationHandConfigRight.eulerAngles1);
            var rotationRight2 = Quaternion.Euler(animationHandConfigRight.eulerAngles2);

            for (int i = 0; i < patternCount; ++i)
            {
                float t = time + timeOffset - randomTime * 0.5f + i * randomTimePerCount;

                float normalizedTime = (t * 0.5f * animationConfig.bpm * BPM_TO_TIME) % 1;
                normalizedTime = (normalizedTime + 1) % 1;

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

                var positionLeft = Vector3.Lerp(positionLeft1, positionLeft2, t);
                _animationPositionsLeft.Add(positionLeft);

                var positionRight = Vector3.Lerp(positionRight1, positionRight2, t);
                _animationPositionsRight.Add(positionRight);

                var rotationLeft = Quaternion.Slerp(rotationLeft1, rotationLeft2, t);
                _animationRotationsLeft.Add(rotationLeft);

                var rotationRight = Quaternion.Slerp(rotationRight1, rotationRight2, t);
                _animationRotationsRight.Add(rotationRight);
            }

            CalcRandomAnimationParams(animationConfig.randomSeed, patternCount);

            _randomAnimationPositions1.Clear();
            _randomAnimationPositions2.Clear();
            _randomAnimationRotations.Clear();

            var randomPosition1Range = animationConfig.randomPosition1Range;
            var randomPosition2Range = animationConfig.randomPosition2Range;
            var randomEulerAnglesRange = animationConfig.randomEulerAnglesRange;

            for (int i = 0; i < patternCount; ++i)
            {
                var param = _randomAnimationPosition1Params[i];
                var position1 = new Vector3(
                    param.x * randomPosition1Range.x * barConfig.baseScale,
                    param.y * randomPosition1Range.y * barConfig.baseScale,
                    param.z * randomPosition1Range.z * barConfig.baseScale
                );
                _randomAnimationPositions1.Add(position1);

                param = Vector3.Lerp(
                    _randomAnimationPosition2Params[i],
                    _randomAnimationPosition1Params[i],
                    animationConfig.positionSyncRate);

                var position2 = new Vector3(
                    param.x * randomPosition2Range.x * barConfig.baseScale,
                    param.y * randomPosition2Range.y * barConfig.baseScale,
                    param.z * randomPosition2Range.z * barConfig.baseScale
                );
                _randomAnimationPositions2.Add(position2);

                param = _randomAnimationEulerAnglesParams[i];
                var eulerAngles = new Vector3(
                    param.x * randomEulerAnglesRange.x,
                    param.y * randomEulerAnglesRange.y,
                    param.z * randomEulerAnglesRange.z
                );
                _randomAnimationRotations.Add(Quaternion.Euler(eulerAngles));
            }

            foreach (var area in areas)
            {
                area.ManualUpdate();
            }
        }

        private void CalcRandomAnimationParams(int randomSeed, int patternCount)
        {
            if (randomSeed == _randomAnimationSeed &&
                patternCount == _randomAnimationPosition1Params.Count &&
                patternCount == _randomAnimationPosition2Params.Count &&
                patternCount == _randomAnimationEulerAnglesParams.Count)
            {
                return;
            }

#if COM3D2
            PluginUtils.LogDebug("CalcRandomAnimationParams");
#endif

            _randomAnimationSeed = randomSeed;
            _randomAnimationPosition1Params.Clear();
            _randomAnimationPosition2Params.Clear();
            _randomAnimationEulerAnglesParams.Clear();

            UnityEngine.Random.InitState(randomSeed);

            for (int i = 0; i < patternCount; ++i)
            {
                var position1 = GetRandomVector3(-1f, 1f);
                _randomAnimationPosition1Params.Add(position1);

                var position2 = GetRandomVector3(-1f, 1f);
                _randomAnimationPosition2Params.Add(position2);

                var eulerAngles = GetRandomVector3(-1f, 1f);
                _randomAnimationEulerAnglesParams.Add(eulerAngles);
            }

            UnityEngine.Random.InitState((int) (Time.realtimeSinceStartup * 1000));
        }

        private static Vector3 GetRandomVector3(float min, float max)
        {
            return new Vector3(
                UnityEngine.Random.Range(min, max),
                UnityEngine.Random.Range(min, max),
                UnityEngine.Random.Range(min, max)
            );
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
            animationHandConfigLeft.CopyFrom(src.animationHandConfigLeft);
            animationHandConfigRight.CopyFrom(src.animationHandConfigRight);
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