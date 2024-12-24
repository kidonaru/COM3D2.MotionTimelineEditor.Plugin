using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace COM3D2.MotionTimelineEditor.Plugin
{

    public class PsylliumArea : MonoBehaviour
    {
        public readonly int MAX_PSYLLIUM_COUNT = 10000;

        [SerializeField]
        public PsylliumController _controller;
        public PsylliumController controller
        {
            get
            {
                return _controller;
            }
            set
            {
                if (_controller == value) return;
                _controller = value;
                UpdateName();
            }
        }

        [SerializeField]
        private int _index = 0;
        public int index
        {
            get
            {
                return _index;
            }
            set
            {
                if (_index == value) return;
                _index = value;
                UpdateName();
            }
        }

        public string displayName;

        public PsylliumAreaConfig areaConfig = new PsylliumAreaConfig();

        public List<Psyllium> psylliums;
        public bool refreshRequired;
    
        private int _psylliumCurrentIndex;

        public int groupIndex
        {
            get
            {
                if (_controller != null)
                {
                    return _controller.groupIndex;
                }
                return 0;
            }
        }

        public PsylliumBarConfig barConfig
        {
            get
            {
                return controller.barConfig;
            }
        }

        public PsylliumHandConfig handConfig
        {
            get
            {
                return controller.handConfig;
            }
        }

        void OnEnable()
        {
            Initialize();
        }

        void Reset()
        {
            Initialize();
        }

        public void Initialize()
        {
            psylliums = GetComponentsInChildren<Psyllium>().ToList();
            UpdateName();
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            refreshRequired = true;
        }
#endif

        public void Setup(PsylliumController scatter)
        {
            this.controller = scatter;
            areaConfig.randomSeed = Random.value.GetHashCode();
            Refresh();
        }

        public void ManualUpdate()
        {
            if (refreshRequired)
            {
                Refresh();
            }

            UpdateTime();
        }

        private void UpdateName()
        {
            var suffix = " (" + groupIndex + ", " + index + ")";
            name = "PsylliumArea" + suffix;
            displayName = "エリア" + suffix;
        }

        public void UpdateMaterial()
        {
            foreach (var psyllium in psylliums)
            {
                psyllium.UpdateMaterial();
            }
        }

        public void UpdateTime()
        {
            foreach (var psyllium in psylliums)
            {
                psyllium.UpdateTime();
            }
        }

        public Psyllium GetOrAddPsyllium(int index)
        {
            if (index < 0)
            {
                Debug.LogError("Invalid index: " + index);
                return null;
            }

            while (psylliums.Count <= index)
            {
                if (psylliums.Count >= MAX_PSYLLIUM_COUNT)
                {
                    Debug.LogError("Too many Psylliums: " + psylliums.Count);
                    return null;
                }

                var obj = new GameObject("Psyllium");
                obj.transform.SetParent(this.transform, false);

                var psyllium = obj.AddComponent<Psyllium>();
                psyllium.controller = controller;
                psylliums.Add(psyllium);
            }

            return psylliums[index];
        }

        public Psyllium GetOrAddPsyllium()
        {
            return GetOrAddPsyllium(++_psylliumCurrentIndex);
        }

        public void RemoveUnusedPsylliums()
        {
            while (psylliums.Count > _psylliumCurrentIndex + 1)
            {
                var psyllium = psylliums[psylliums.Count - 1];
                psylliums.RemoveAt(psylliums.Count - 1);
                if (Application.isPlaying)
                {
                    Destroy(psyllium.gameObject);
                }
                else
                {
                    DestroyImmediate(psyllium.gameObject);
                }
            }
        }

        public void Refresh()
        {
            var halfDistance = handConfig.handSpacing / 2f;

            // 0割り回避
            areaConfig.seatDistance.x = Mathf.Max(0.01f, areaConfig.seatDistance.x);
            areaConfig.seatDistance.y = Mathf.Max(0.01f, areaConfig.seatDistance.y);

            Random.InitState(areaConfig.randomSeed);

            gameObject.SetActive(areaConfig.visible);
            transform.localPosition = areaConfig.position;
            transform.localEulerAngles = areaConfig.rotation;

            _psylliumCurrentIndex = -1;

            var areaSize = areaConfig.size;
            var halfAreaSize = areaSize * 0.5f;
            var seatDistance = areaConfig.seatDistance;

            for (float x = -halfAreaSize.x; x < halfAreaSize.x; x += seatDistance.x)
            {
                for (float z = -halfAreaSize.y; z < halfAreaSize.y; z += seatDistance.y)
                {
                    var randomValues = new PsylliumRandomValues(handConfig, areaConfig);

                    // 基準位置を計算
                    var basePosition = new Vector3(x, 0, z) + randomValues.basePosition;
                    
                    // 左手と右手の位置を計算
                    var leftHandPos = basePosition + new Vector3(-halfDistance, 0f, 0f);
                    var rightHandPos = basePosition + new Vector3(halfDistance, 0f, 0f);

                    // 左手のPsylliumを配置
                    UpdatePsylliums(
                        leftHandPos,
                        randomValues.leftCount,
                        randomValues.timeIndex,
                        randomValues.leftColorIndexes,
                        randomValues.leftPositionParam,
                        randomValues.leftRotationParam,
                        true);

                    // 右手のPsylliumを配置
                    UpdatePsylliums(
                        rightHandPos,
                        randomValues.rightCount,
                        randomValues.timeIndex,
                        randomValues.rightColorIndexes,
                        randomValues.rightPositionParam,
                        randomValues.rightRotationParam,
                        false);
                }
            }

            RemoveUnusedPsylliums();

            refreshRequired = false;
        }

        public void CopyFrom(PsylliumArea src)
        {
            areaConfig.CopyFrom(src.areaConfig);
            Refresh();
        }

        private void UpdatePsylliums(
            Vector3 handPos,
            int count,
            int timeIndex,
            int[] colorIndexes,
            Vector3 positionParam,
            Vector3 rotationParam,
            bool isLeftHand)
        {
            for (int j = 0; j < count; j++)
            {
                var barPosition = (j - (count - 1) * 0.5f) * handConfig.barOffsetPosition;
                var barRotation = (j - (count - 1) * 0.5f) * handConfig.barOffsetRotation;
                var position = handPos;
                var rotation = new Vector3();

                if (!isLeftHand)
                {
                    barPosition.x = -barPosition.x;
                    barRotation.z = -barRotation.z;
                }

                var psyllium = GetOrAddPsyllium();
                if (psyllium == null) return;

                psyllium.randomTimeIndex = timeIndex;
                psyllium.colorIndex = colorIndexes[j];
                psyllium.randomPositionParam = positionParam;
                psyllium.randomRotationParam = rotationParam;
                psyllium.barPosition = barPosition;
                psyllium.barRotation = barRotation;
                psyllium.isLeftHand = isLeftHand;
                psyllium.transform.localPosition = position;
                psyllium.transform.localEulerAngles = rotation;

                psyllium.UpdateMesh();
                psyllium.UpdateMaterial();
            }
        }
    }
}