using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace COM3D2.MotionTimelineEditor.Plugin
{
    [System.Serializable]
    public struct RotationCache
    {
        private Vector3 _eulerAngles;
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
                _rotation = Quaternion.Euler(value);
            }
        }

        private Quaternion _rotation;
        public Quaternion rotation
        {
            get
            {
                return _rotation;
            }
        }
    }

    [System.Serializable]
    public class PsylliumTransformData
    {
        public Vector3 positionLeft = new Vector3(0, 0.3f, -0.5f);
        public Vector3 positionRight = new Vector3(0, 0.3f, -0.5f);

        private RotationCache rotationCacheLeft;
        private RotationCache rotationCacheRight;

        public Vector3 eulerAnglesLeft
        {
            get
            {
                return rotationCacheLeft.eulerAngles;
            }
            set
            {
                rotationCacheLeft.eulerAngles = value;
            }
        }

        public Vector3 eulerAnglesRight
        {
            get
            {
                return rotationCacheRight.eulerAngles;
            }
            set
            {
                rotationCacheRight.eulerAngles = value;
            }
        }

        public Quaternion rotationLeft
        {
            get
            {
                return rotationCacheLeft.rotation;
            }
        }

        public Quaternion rotationRight
        {
            get
            {
                return rotationCacheRight.rotation;
            }
        }

        public PsylliumTransformData()
        {
            rotationCacheLeft.eulerAngles = new Vector3(-10, 0, 0);
            rotationCacheRight.eulerAngles = new Vector3(-10, 0, 0);
        }

        public void CopyFrom(PsylliumTransformData other)
        {
            positionLeft = other.positionLeft;
            positionRight = other.positionRight;
            eulerAnglesLeft = other.eulerAnglesLeft;
            eulerAnglesRight = other.eulerAnglesRight;
        }

        public void CopyFrom(PsylliumTransformConfig config)
        {
            positionLeft = config.positionLeft;
            positionRight = config.positionRight;
            eulerAnglesLeft = config.eulerAnglesLeft;
            eulerAnglesRight = config.eulerAnglesRight;
        }

        public bool Equals(PsylliumTransformData other)
        {
            return positionLeft == other.positionLeft &&
                   positionRight == other.positionRight &&
                   eulerAnglesLeft == other.eulerAnglesLeft &&
                   eulerAnglesRight == other.eulerAnglesRight;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            return Equals((PsylliumTransformData)obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    [System.Serializable]
    public class PsylliumPattern
    {
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

        public PsylliumPatternConfig patternConfig = new PsylliumPatternConfig();
        public PsylliumTransformConfig transformConfig = new PsylliumTransformConfig();

        private int _transformCount = 0;
        public List<PsylliumTransformData> transforms = new List<PsylliumTransformData>();

        private int _randomAnimationSeed = 0;
        private List<Vector3> _randomAnimationPositionParams = new List<Vector3>();
        private List<Vector3> _randomAnimationEulerAnglesParams = new List<Vector3>();
        private List<Vector3> _randomAnimationPositions = new List<Vector3>();
        private List<Quaternion> _randomAnimationRotations = new List<Quaternion>();

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

        public PsylliumPattern()
        {
        }

        public void Setup(PsylliumController controller)
        {
            this.controller = controller;
            patternConfig.randomSeed = Random.Range(1, int.MaxValue);
            UpdateName();
            ApplyTransformData(transformConfig);
            ApplyTransformData(transformConfig);
        }

        public void CopyFrom(PsylliumPattern other)
        {
            patternConfig.CopyFrom(other.patternConfig);
            transformConfig.CopyFrom(other.transformConfig);
        }

        public PsylliumTransformData GetTransformData(int randomIndex)
        {
            if (_transformCount == 0) return null;
            return transforms[CalcLoopIndex(randomIndex, _transformCount)];
        }

        public void ClearTransformData()
        {
            _transformCount = 0;
        }

        public void ApplyTransformData(PsylliumTransformConfig config)
        {
            _transformCount++;

            if (_transformCount > transforms.Count)
            {
                var transform = new PsylliumTransformData();
                transforms.Add(transform);
            }

            transforms[_transformCount - 1].CopyFrom(config);
        }

        public void UpdateName()
        {
            patternConfig.UpdateName(groupIndex, index);
            transformConfig.UpdateName(groupIndex, index);
        }

        public static int CalcLoopIndex(int index, int count)
        {
            var loopIndex = index % (count * 2);
            if (loopIndex >= count) loopIndex = count * 2 - 1 - loopIndex;
            return loopIndex;
        }

        public Vector3 GetAnimationPosition(int timeIndex, bool isLeftHand)
        {
            var trans = GetTransformData(timeIndex);
            if (trans == null) return Vector3.zero;

            if (isLeftHand)
            {
                return trans.positionLeft * barConfig.baseScale;
            }
            else
            {
                return trans.positionRight * barConfig.baseScale;
            }
        }

        public Quaternion GetAnimationRotation(int timeIndex, bool isLeftHand)
        {
            var trans = GetTransformData(timeIndex);
            if (trans == null) return Quaternion.identity;

            if (isLeftHand)
            {
                return trans.rotationLeft;
            }
            else
            {
                return trans.rotationRight;
            }
        }

        public Vector3 GetRandomAnimationPosition(int randomIndex)
        {
            if (_randomAnimationPositions.Count == 0) return Vector3.zero;
            return _randomAnimationPositions[CalcLoopIndex(randomIndex, _randomAnimationPositions.Count)];
        }

        public Quaternion GetRandomAnimationRotation(int randomIndex)
        {
            if (_randomAnimationRotations.Count == 0) return Quaternion.identity;
            return _randomAnimationRotations[CalcLoopIndex(randomIndex, _randomAnimationRotations.Count)];
        }

        public void ManualUpdate()
        {
            var patternCount = patternConfig.timeCount;

            CalcRandomAnimationParams(patternConfig.randomSeed, patternCount);

            _randomAnimationPositions.Clear();
            _randomAnimationRotations.Clear();

            var randomPositionRange = patternConfig.randomPositionRange;
            var randomEulerAnglesRange = patternConfig.randomEulerAnglesRange;

            for (int i = 0; i < patternCount; ++i)
            {
                var param = _randomAnimationPositionParams[i];
                var position = new Vector3(
                    param.x * randomPositionRange.x * barConfig.baseScale,
                    param.y * randomPositionRange.y * barConfig.baseScale,
                    param.z * randomPositionRange.z * barConfig.baseScale
                );
                _randomAnimationPositions.Add(position);

                param = _randomAnimationEulerAnglesParams[i];
                var eulerAngles = new Vector3(
                    param.x * randomEulerAnglesRange.x,
                    param.y * randomEulerAnglesRange.y,
                    param.z * randomEulerAnglesRange.z
                );
                _randomAnimationRotations.Add(Quaternion.Euler(eulerAngles));
            }
        }

        private void CalcRandomAnimationParams(int randomSeed, int patternCount)
        {
            if (randomSeed == _randomAnimationSeed &&
                patternCount == _randomAnimationPositionParams.Count &&
                patternCount == _randomAnimationEulerAnglesParams.Count)
            {
                return;
            }

#if COM3D2
            PluginUtils.LogDebug("CalcRandomAnimationParams");
#endif

            _randomAnimationSeed = randomSeed;
            _randomAnimationPositionParams.Clear();
            _randomAnimationEulerAnglesParams.Clear();

            UnityEngine.Random.InitState(randomSeed);

            for (int i = 0; i < patternCount; ++i)
            {
                var position1 = GetRandomVector3(-1f, 1f);
                _randomAnimationPositionParams.Add(position1);

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
    }
}