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
        public readonly int MAX_PSYLLIUM_HAND_COUNT = 10000;

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

        public List<PsylliumHand> hands;
        public bool refreshRequired;
    
        private int _handCurrentIndex;

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
            hands = GetComponentsInChildren<PsylliumHand>().ToList();
            UpdateName();
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            refreshRequired = true;
        }
#endif

        public void Setup(PsylliumController controller)
        {
            this.controller = controller;
            areaConfig.randomSeed = Random.Range(1, int.MaxValue);
            Refresh();
        }

        public void ManualUpdate()
        {
            if (refreshRequired)
            {
                Refresh();
                return;
            }

            UpdateTime();
        }

        public void UpdateName()
        {
            var suffix = " (" + groupIndex + ", " + index + ")";
            name = "PsylliumArea" + suffix;
            displayName = "エリア" + suffix;
        }

        public void UpdateTime()
        {
            foreach (var hand in hands)
            {
                hand.UpdateTime();
            }
        }

        public PsylliumHand GetOrAddHand(int index)
        {
            if (index < 0)
            {
                Debug.LogError("Invalid index: " + index);
                return null;
            }

            while (hands.Count <= index)
            {
                if (hands.Count >= MAX_PSYLLIUM_HAND_COUNT)
                {
                    Debug.LogError("Too many hands: " + hands.Count);
                    return null;
                }

                var obj = new GameObject("PsylliumHand");
                obj.transform.SetParent(this.transform, false);

                var psyllium = obj.AddComponent<PsylliumHand>();
                psyllium.Setup(controller, this);
                hands.Add(psyllium);
            }

            return hands[index];
        }

        public PsylliumHand GetOrCreateHand()
        {
            return GetOrAddHand(++_handCurrentIndex);
        }

        public void RemoveUnusedHands()
        {
            while (hands.Count > _handCurrentIndex + 1)
            {
                var psyllium = hands[hands.Count - 1];
                hands.RemoveAt(hands.Count - 1);

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
            var halfHandSpacing = handConfig.handSpacing * barConfig.baseScale * 0.5f;

            Random.InitState(areaConfig.randomSeed);

            gameObject.SetActive(areaConfig.visible);
            transform.localPosition = areaConfig.position;
            transform.localEulerAngles = areaConfig.rotation;

            _handCurrentIndex = -1;

            var areaSize = areaConfig.size;
            var halfAreaSize = areaSize * 0.5f;
            var seatDistance = areaConfig.seatDistance * barConfig.baseScale;

            // 無限ループ回避
            seatDistance.x = Mathf.Max(0.01f, seatDistance.x);
            seatDistance.y = Mathf.Max(0.01f, seatDistance.y);

            for (float x = -halfAreaSize.x; x < halfAreaSize.x; x += seatDistance.x)
            {
                for (float z = -halfAreaSize.y; z < halfAreaSize.y; z += seatDistance.y)
                {
                    var randomValues = new PsylliumRandomValues(controller, areaConfig);

                    // 基準位置を計算
                    var basePosition = new Vector3(x, 0, z) + randomValues.basePosition * barConfig.baseScale;

                    // 左手と右手の位置を計算
                    var leftHandPos = basePosition + new Vector3(halfHandSpacing, 0f, 0f);
                    var rightHandPos = basePosition + new Vector3(-halfHandSpacing, 0f, 0f);

                    if (randomValues.leftCount > 0)
                    {
                        var hand = GetOrCreateHand();
                        if (hand == null) return;

                        hand.UpdatePsylliums(
                            leftHandPos,
                            randomValues.leftCount,
                            randomValues.patternIndex,
                            randomValues.timeIndex,
                            randomValues.timeShiftParam,
                            randomValues.leftColorIndexes,
                            randomValues.leftRandomPositionIndex,
                            randomValues.leftRandomRotationIndex,
                            true);
                    }

                    if (randomValues.rightCount > 0)
                    {
                        var hand = GetOrCreateHand();
                        if (hand == null) return;

                        hand.UpdatePsylliums(
                            rightHandPos,
                            randomValues.rightCount,
                            randomValues.patternIndex,
                            randomValues.timeIndex,
                            randomValues.timeShiftParam,
                            randomValues.rightColorIndexes,
                            randomValues.rightRandomPositionIndex,
                            randomValues.rightRandomRotationIndex,
                            false);
                    }
                }
            }

            RemoveUnusedHands();
            UpdateTime();

            Random.InitState((int) (Time.realtimeSinceStartup * 1000));

            refreshRequired = false;
        }

        public void CopyFrom(PsylliumArea src, bool ignoreTransform)
        {
            areaConfig.CopyFrom(src.areaConfig, ignoreTransform);
            Refresh();
        }
    }
}