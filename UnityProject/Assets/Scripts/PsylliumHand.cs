using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace COM3D2.MotionTimelineEditor.Plugin
{
    [ExecuteInEditMode]
    public class PsylliumHand : MonoBehaviour
    {
        public const int MAX_PSYLLIUM_COUNT = 3;

        public PsylliumController controller;
        public PsylliumArea area;
        public int patternIndex;
        public int timeIndex;
        public float timeShiftParam;
        public int randomPositionIndex;
        public int randomRotationIndex;
        public Vector3 basePosition;
        public bool isLeftHand;

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

        public PsylliumPattern pattern
        {
            get
            {
                return controller.GetPattern(patternIndex);
            }
        }

        public PsylliumPatternConfig patternConfig
        {
            get
            {
                if (pattern == null) return null;
                return pattern.patternConfig;
            }
        }

        public PsylliumAreaConfig areaConfig
        {
            get
            {
                return area.areaConfig;
            }
        }

        public List<Psyllium> psylliums = new List<Psyllium>();

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
        }

        public void Setup(PsylliumController controller, PsylliumArea area)
        {
            this.controller = controller;
            this.area = area;
        }

        public void UpdateTime()
        {
            if (controller == null || area == null || pattern == null)
            {
                return;
            }

            var timeShift = patternConfig.timeShiftMin + (patternConfig.timeShiftMax - patternConfig.timeShiftMin) * timeShiftParam;
            var timeIndex = this.timeIndex + (int)(controller.time * timeShift);

            var position = pattern.GetAnimationPosition(timeIndex, isLeftHand);
            var rotation = pattern.GetAnimationRotation(timeIndex, isLeftHand);

            var randomPosition = pattern.GetRandomAnimationPosition(randomPositionIndex);
            var randomRotation = pattern.GetRandomAnimationRotation(randomRotationIndex);

            position += basePosition + randomPosition;
            rotation *= randomRotation;

            transform.localPosition = position;
            transform.localRotation = rotation;
        }

        public void UpdatePsylliums(
            Vector3 handPos,
            int count,
            int patternIndex,
            int timeIndex,
            float timeShiftParam,
            int[] colorIndexes,
            int randomPositionIndex,
            int randomRotationIndex,
            bool isLeftHand)
        {
            this.basePosition = handPos;
            this.isLeftHand = isLeftHand;
            this.patternIndex = patternIndex;
            this.timeIndex = timeIndex;
            this.timeShiftParam = timeShiftParam;
            this.randomPositionIndex = randomPositionIndex;
            this.randomRotationIndex = randomRotationIndex;

            CreatePsylliums(count);

            for (int j = 0; j < count; j++)
            {
                var barPosition = (j - (count - 1) * 0.5f) * handConfig.barOffsetPosition * barConfig.baseScale;
                var barEulerAngles = (j - (count - 1) * 0.5f) * handConfig.barOffsetRotation;

                if (!isLeftHand)
                {
                    barPosition.x = -barPosition.x;
                    barEulerAngles.y = -barEulerAngles.y;
                    barEulerAngles.z = -barEulerAngles.z;
                }

                var psyllium = psylliums[j];
                if (psyllium == null) return;

                psyllium.Setup(controller, colorIndexes[j]);

                psyllium.transform.localPosition = barPosition;
                psyllium.transform.localEulerAngles = barEulerAngles;
            }
        }

        private void CreatePsylliums(int count)
        {
            while (psylliums.Count < count)
            {
                var obj = new GameObject("Psyllium");
                obj.transform.SetParent(this.transform, false);

                var psyllium = obj.AddComponent<Psyllium>();
                psyllium.controller = controller;
                psylliums.Add(psyllium);
            }

            for (int i = psylliums.Count - 1; i >= count; i--)
            {
                var psyllium = psylliums[i];
                psylliums.RemoveAt(i);

#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    DestroyImmediate(psyllium.gameObject);
                }
                else
#endif
                {
                    Destroy(psyllium.gameObject);
                }
            }
        }
    }
}
