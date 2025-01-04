using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace COM3D2.MotionTimelineEditor.Plugin
{
    [System.Serializable]
    public class PsylliumBarConfig
    {
        public int groupIndex;
        public string name;
        public string displayName;

        public Color color1a = new Color(246 / 255f, 250 / 255f, 59 / 255f, 1);
        public Color color1b = new Color(255 / 255f, 161 / 255f, 45 / 255f, 1);
        public Color color1c = new Color(246 / 255f, 250 / 255f, 59 / 255f, 127 / 255f);
        public Color color2a = new Color(229 / 255f, 107 / 255f, 252 / 255f, 1);
        public Color color2b = new Color(101 / 255f, 39 / 255f, 163 / 255f, 1);
        public Color color2c = new Color(229 / 255f, 107 / 255f, 252 / 255f, 127 / 255f);
        public float baseScale = 1.0f;
        public float width = 0.14f;
        public float height = 0.2f;
        public float positionY = 0.1f;
        public float radius = 0.07f;
        public float topThreshold = 0.23f;
        public float cutoffAlpha = 0.5f;

        public Color GetColorA(int index)
        {
            return index % 2 == 0 ? color1a : color2a;
        }

        public Color GetColorB(int index)
        {
            return index % 2 == 0 ? color1b : color2b;
        }

        public Color GetColorC(int index)
        {
            return index % 2 == 0 ? color1c : color2c;
        }

        public void CopyFrom(PsylliumBarConfig other)
        {
            color1a = other.color1a;
            color1b = other.color1b;
            color1c = other.color1c;
            color2a = other.color2a;
            color2b = other.color2b;
            color2c = other.color2c;
            baseScale = other.baseScale;
            width = other.width;
            height = other.height;
            positionY = other.positionY;
            radius = other.radius;
            topThreshold = other.topThreshold;
            cutoffAlpha = other.cutoffAlpha;
        }

        public bool Equals(PsylliumBarConfig other)
        {
            return color1a == other.color1a
                && color1b == other.color1b
                && color1c == other.color1c
                && color2a == other.color2a
                && color2b == other.color2b
                && color2c == other.color2c
                && baseScale == other.baseScale
                && width == other.width
                && height == other.height
                && positionY == other.positionY
                && radius == other.radius
                && topThreshold == other.topThreshold
                && cutoffAlpha == other.cutoffAlpha;
        }

        public void UpdateName(int groupIndex)
        {
            this.groupIndex = groupIndex;

            var suffix = " (" + groupIndex + ")";
            name = "PsylliumBarConfig" + suffix;
            displayName = "バー設定" + suffix;
        }
    }

    [System.Serializable]
    public class PsylliumHandConfig
    {
        public int groupIndex;
        public string name;
        public string displayName;

        public float handSpacing = 0.37f; // 両手間の距離
        public Vector3 barOffsetPosition = new Vector3(0.03f, 0, 0); // バー間の位置
        public Vector3 barOffsetRotation = new Vector3(0, 0, -20); // バー間の回転

        public void CopyFrom(PsylliumHandConfig other)
        {
            handSpacing = other.handSpacing;
            barOffsetPosition = other.barOffsetPosition;
            barOffsetRotation = other.barOffsetRotation;
        }

        public bool Equals(PsylliumHandConfig other)
        {
            return handSpacing == other.handSpacing
                && barOffsetPosition == other.barOffsetPosition
                && barOffsetRotation == other.barOffsetRotation;
        }

        public void UpdateName(int groupIndex)
        {
            this.groupIndex = groupIndex;

            var suffix = " (" + groupIndex + ")";
            name = "PsylliumHandConfig" + suffix;
            displayName = "持ち手設定" + suffix;
        }
    }

    [System.Serializable]
    public class PsylliumPatternConfig
    {
        public int groupIndex;
        public int patternIndex;
        public string name;
        public string displayName;

        public Vector3 randomPositionRange = new Vector3(0.1f, 0.1f, 0.1f);
        public Vector3 randomEulerAnglesRange = new Vector3(5f, 0f, 10f);

        public int timeCount = 10;
        public float timeRange = 0.05f;
        public float timeShiftMin = 0.5f;
        public float timeShiftMax = 1.5f;
        public int randomSeed;

        public void CopyFrom(PsylliumPatternConfig other)
        {
            randomPositionRange = other.randomPositionRange;
            randomEulerAnglesRange = other.randomEulerAnglesRange;
            timeCount = other.timeCount;
            timeRange = other.timeRange;
            timeShiftMin = other.timeShiftMin;
            timeShiftMax = other.timeShiftMax;
            randomSeed = other.randomSeed;
        }

        public bool Equals(PsylliumPatternConfig other)
        {
            return randomPositionRange == other.randomPositionRange
                && randomEulerAnglesRange == other.randomEulerAnglesRange
                && timeCount == other.timeCount
                && timeRange == other.timeRange
                && timeShiftMin == other.timeShiftMin
                && timeShiftMax == other.timeShiftMax
                && randomSeed == other.randomSeed;
        }

        public void UpdateName(int groupIndex, int patternIndex)
        {
            this.groupIndex = groupIndex;
            this.patternIndex = patternIndex;

            var suffix = " (" + groupIndex + ", " + patternIndex + ")";
            name = "PsylliumPattern" + suffix;
            displayName = "パターン" + suffix;
        }
    }

    [System.Serializable]
    public class PsylliumTransformConfig
    {
        public int groupIndex;
        public int patternIndex;
        public string name;
        public string displayName;

        public Vector3 positionLeft = new Vector3(0, 0.3f, -0.5f);
        public Vector3 positionRight = new Vector3(0, 0.3f, -0.5f);
        public Vector3 eulerAnglesLeft = new Vector3(-10, 0, 0);
        public Vector3 eulerAnglesRight = new Vector3(-10, 0, 0);

        public void CopyFrom(PsylliumTransformConfig other)
        {
            positionLeft = other.positionLeft;
            positionRight = other.positionRight;
            eulerAnglesLeft = other.eulerAnglesLeft;
            eulerAnglesRight = other.eulerAnglesRight;
        }

        public bool Equals(PsylliumTransformConfig other)
        {
            return positionLeft == other.positionLeft
                && positionRight == other.positionRight
                && eulerAnglesLeft == other.eulerAnglesLeft
                && eulerAnglesRight == other.eulerAnglesRight;
        }

        public void UpdateName(int groupIndex, int patternIndex)
        {
            this.groupIndex = groupIndex;
            this.patternIndex = patternIndex;

            var suffix = " (" + groupIndex + ", " + patternIndex + ")";
            name = "PsylliumTransform" + suffix;
            displayName = "移動回転" + suffix;
        }
    }

    [System.Serializable]
    public class PsylliumAreaConfig
    {
        public bool visible = true;
        public Vector3 position = new Vector3(0, 0, 11);
        public Vector3 rotation = new Vector3(0, 0, 0);
        public Vector2 size = new Vector2(10f, 10f);
        public Vector2 seatDistance = new Vector2(1f, 1f);
        public Vector3 randomPositionRange = new Vector3(0.05f, 0.05f, 0.05f);
        [Range(0, 1)]
        public float barCountWeight0 = 0.5f; // バーの数0個の重み
        [Range(0, 1)]
        public float barCountWeight1 = 0.7f; // バーの数1個の重み
        [Range(0, 1)]
        public float barCountWeight2 = 0.05f; // バーの数2個の重み
        [Range(0, 1)]
        public float barCountWeight3 = 0.05f; // バーの数3個の重み
        [Range(0, 1)]
        public float colorWeight1 = 0.175f; // 色1の重み
        [Range(0, 1)]
        public float colorWeight2 = 0.5f; // 色2の重み
        [Range(0, 1)]
        public float patternWeight0 = 0.5f; // パターン0の重み
        [Range(0, 1)]
        public float patternWeight1 = 0.5f; // パターン1の重み
        [Range(0, 1)]
        public float patternWeight2 = 0.5f; // パターン2の重み
        [Range(0, 1)]
        public float patternWeight3 = 0.5f; // パターン3の重み
        [Range(0, 1)]
        public float patternWeight4 = 0.5f; // パターン4の重み
        [Range(0, 1)]
        public float patternWeight5 = 0.5f; // パターン5の重み
        [Range(0, 1)]
        public float patternWeight6 = 0.5f; // パターン6の重み
        [Range(0, 1)]
        public float patternWeight7 = 0.5f; // パターン7の重み
        [Range(0, 1)]
        public float patternWeight8 = 0.5f; // パターン8の重み
        [Range(0, 1)]
        public float patternWeight9 = 0.5f; // パターン9の重み

        public int randomSeed;

        public Vector3 randomPosition
        {
            get
            {
                return new Vector3(
                    Random.Range(-randomPositionRange.x, randomPositionRange.x),
                    Random.Range(-randomPositionRange.y, randomPositionRange.y),
                    Random.Range(-randomPositionRange.z, randomPositionRange.z)
                );
            }
        }

        public float[] barCountWeights
        {
            get
            {
                return new float[]
                {
                    barCountWeight0,
                    barCountWeight1,
                    barCountWeight2,
                    barCountWeight3
                };
            }
        }

        public float[] colorWeights
        {
            get
            {
                return new float[]
                {
                    colorWeight1,
                    colorWeight2
                };
            }
        }

        public float[] patternWeights
        {
            get
            {
                return new float[]
                {
                    patternWeight0,
                    patternWeight1,
                    patternWeight2,
                    patternWeight3,
                    patternWeight4,
                    patternWeight5,
                    patternWeight6,
                    patternWeight7,
                    patternWeight8,
                    patternWeight9
                };
            }
        }

        public void CopyFrom(PsylliumAreaConfig other, bool ignoreTransform)
        {
            visible = other.visible;
            seatDistance = other.seatDistance;
            randomPositionRange = other.randomPositionRange;
            barCountWeight0 = other.barCountWeight0;
            barCountWeight1 = other.barCountWeight1;
            barCountWeight2 = other.barCountWeight2;
            barCountWeight3 = other.barCountWeight3;
            colorWeight1 = other.colorWeight1;
            colorWeight2 = other.colorWeight2;
            patternWeight0 = other.patternWeight0;
            patternWeight1 = other.patternWeight1;
            patternWeight2 = other.patternWeight2;
            patternWeight3 = other.patternWeight3;
            patternWeight4 = other.patternWeight4;
            patternWeight5 = other.patternWeight5;
            patternWeight6 = other.patternWeight6;
            patternWeight7 = other.patternWeight7;
            patternWeight8 = other.patternWeight8;
            patternWeight9 = other.patternWeight9;

            if (!ignoreTransform)
            {
                position = other.position;
                rotation = other.rotation;
                size = other.size;
                randomSeed = other.randomSeed;
            }
        }

        public bool Equals(PsylliumAreaConfig other)
        {
            return visible == other.visible
                && position == other.position
                && rotation == other.rotation
                && size == other.size
                && seatDistance == other.seatDistance
                && randomPositionRange == other.randomPositionRange
                && barCountWeight0 == other.barCountWeight0
                && barCountWeight1 == other.barCountWeight1
                && barCountWeight2 == other.barCountWeight2
                && barCountWeight3 == other.barCountWeight3
                && colorWeight1 == other.colorWeight1
                && colorWeight2 == other.colorWeight2
                && patternWeight0 == other.patternWeight0
                && patternWeight1 == other.patternWeight1
                && patternWeight2 == other.patternWeight2
                && patternWeight3 == other.patternWeight3
                && patternWeight4 == other.patternWeight4
                && patternWeight5 == other.patternWeight5
                && patternWeight6 == other.patternWeight6
                && patternWeight7 == other.patternWeight7
                && patternWeight8 == other.patternWeight8
                && patternWeight9 == other.patternWeight9
                && randomSeed == other.randomSeed;
        }

        public PsylliumAreaConfig Clone()
        {
            var config = new PsylliumAreaConfig();
            config.CopyFrom(this, false);
            return config;
        }
    }
}