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
        public int timeIndex;
        public float timeShiftParam;
        public Vector3 randomPositionParam;
        public Vector3 randomRotationParam;
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

        public PsylliumAnimationConfig animationConfig
        {
            get
            {
                return controller.animationConfig;
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
            if (controller == null || area == null)
            {
                return;
            }

            var timeShift = animationConfig.timeShiftMin + (animationConfig.timeShiftMax - animationConfig.timeShiftMin) * timeShiftParam;
            var timeIndex = this.timeIndex + (int)(controller.time * timeShift);
            var position = controller.GetAnimationPosition(timeIndex, isLeftHand);
            var rotation = controller.GetAnimationRotation(timeIndex, isLeftHand);

            position += basePosition;

            transform.localPosition = position;
            transform.localRotation = rotation;
        }

        public void UpdatePsylliums(
            Vector3 handPos,
            int count,
            int timeIndex,
            float timeShiftParam,
            int[] colorIndexes,
            Vector3 positionParam,
            Vector3 rotationParam,
            bool isLeftHand)
        {
            this.basePosition = handPos;
            this.isLeftHand = isLeftHand;
            this.timeIndex = timeIndex;
            this.timeShiftParam = timeShiftParam;
            this.randomPositionParam = positionParam;
            this.randomRotationParam = rotationParam;

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
