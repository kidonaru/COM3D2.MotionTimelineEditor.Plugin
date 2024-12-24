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
                && width == other.width
                && height == other.height
                && positionY == other.positionY
                && radius == other.radius
                && topThreshold == other.topThreshold
                && cutoffAlpha == other.cutoffAlpha;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            return Equals((PsylliumBarConfig)obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
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
        [Range(0, 1)]
        public float barCountWeight0 = 0.5f; // バーの数0個の重み
        [Range(0, 1)]
        public float barCountWeight1 = 0.7f; // バーの数1個の重み
        [Range(0, 1)]
        public float barCountWeight2 = 0.2f; // バーの数2個の重み
        [Range(0, 1)]
        public float barCountWeight3 = 0.0f; // バーの数3個の重み
        public Vector3 barOffsetPosition = new Vector3(0.03f, 0, 0); // バー毎の位置
        public Vector3 barOffsetRotation = new Vector3(0, 0, -20); // バー毎の回転
        [Range(0, 1)]
        public float colorWeight1 = 0.175f; // 色1の重み
        [Range(0, 1)]
        public float colorWeight2 = 0.5f; // 色2の重み

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

        public void CopyFrom(PsylliumHandConfig other)
        {
            handSpacing = other.handSpacing;
            barCountWeight0 = other.barCountWeight0;
            barCountWeight1 = other.barCountWeight1;
            barCountWeight2 = other.barCountWeight2;
            barCountWeight3 = other.barCountWeight3;
            barOffsetPosition = other.barOffsetPosition;
            barOffsetRotation = other.barOffsetRotation;
            colorWeight1 = other.colorWeight1;
            colorWeight2 = other.colorWeight2;
        }

        public bool Equals(PsylliumHandConfig other)
        {
            return handSpacing == other.handSpacing
                && barCountWeight0 == other.barCountWeight0
                && barCountWeight1 == other.barCountWeight1
                && barCountWeight2 == other.barCountWeight2
                && barCountWeight3 == other.barCountWeight3
                && barOffsetPosition == other.barOffsetPosition
                && barOffsetRotation == other.barOffsetRotation
                && colorWeight1 == other.colorWeight1
                && colorWeight2 == other.colorWeight2;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            return Equals((PsylliumHandConfig)obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
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
    public class PsylliumAnimationConfig
    {
        public int groupIndex;
        public string name;
        public string displayName;

        public Vector3 position1 = new Vector3(0, 0.3f, 0.5f);
        public Vector3 position2 = new Vector3(0, 0.1f, 0);
        public Vector3 rotation1 = new Vector3(30, 0, 0);
        public Vector3 rotation2 = new Vector3(-120, 0, 0);
        public Vector3 randomPositionRange = new Vector3(0.01f, 0.01f, 0.01f);
        public Vector3 randomRotationRange = new Vector3(5f, 0f, 10f);

        public bool mirrorRotationZ = true;
        public float cutoffHeight = 0f;

        public float bpm = 120f;
        public int randomTimeCount = 10;
        public float randomTime = 0.05f;
        public float timeRatio = 0.75f;
        public MoveEasingType easingType1 = MoveEasingType.SineInOut;
        public MoveEasingType easingType2 = MoveEasingType.SineOut;

        public void CopyFrom(PsylliumAnimationConfig other)
        {
            position1 = other.position1;
            position2 = other.position2;
            rotation1 = other.rotation1;
            rotation2 = other.rotation2;
            randomPositionRange = other.randomPositionRange;
            randomRotationRange = other.randomRotationRange;
            mirrorRotationZ = other.mirrorRotationZ;
            cutoffHeight = other.cutoffHeight;
            bpm = other.bpm;
            randomTimeCount = other.randomTimeCount;
            randomTime = other.randomTime;
            timeRatio = other.timeRatio;
            easingType1 = other.easingType1;
            easingType2 = other.easingType2;
        }

        public bool Equals(PsylliumAnimationConfig other)
        {
            return position1 == other.position1
                && position2 == other.position2
                && rotation1 == other.rotation1
                && rotation2 == other.rotation2
                && randomPositionRange == other.randomPositionRange
                && randomRotationRange == other.randomRotationRange
                && mirrorRotationZ == other.mirrorRotationZ
                && cutoffHeight == other.cutoffHeight
                && bpm == other.bpm
                && randomTimeCount == other.randomTimeCount
                && randomTime == other.randomTime
                && timeRatio == other.timeRatio
                && easingType1 == other.easingType1
                && easingType2 == other.easingType2;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            return Equals((PsylliumAnimationConfig)obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public void UpdateName(int groupIndex)
        {
            this.groupIndex = groupIndex;

            var suffix = " (" + groupIndex + ")";
            name = "PsylliumAnimationConfig" + suffix;
            displayName = "アニメ設定" + suffix;
        }
    }

    [System.Serializable]
    public class PsylliumAreaConfig
    {
        public bool visible = true;
        public Vector3 position = new Vector3(0, 0, 0);
        public Vector3 rotation = new Vector3(0, 0, 0);
        public Vector2 size = new Vector2(10f, 10f);
        public Vector2 seatDistance = new Vector2(1f, 1f);
        public Vector3 randomPositionRange = new Vector3(0.05f, 0.05f, 0.05f);
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

        public void CopyFrom(PsylliumAreaConfig other)
        {
            visible = other.visible;
            position = other.position;
            rotation = other.rotation;
            size = other.size;
            seatDistance = other.seatDistance;
            randomPositionRange = other.randomPositionRange;
            randomSeed = other.randomSeed;
        }

        public bool Equals(PsylliumAreaConfig other)
        {
            return visible == other.visible
                && position == other.position
                && rotation == other.rotation
                && size == other.size
                && seatDistance == other.seatDistance
                && randomPositionRange == other.randomPositionRange
                && randomSeed == other.randomSeed;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            return Equals((PsylliumAreaConfig)obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public PsylliumAreaConfig Clone()
        {
            var config = new PsylliumAreaConfig();
            config.CopyFrom(this);
            return config;
        }
    }
}