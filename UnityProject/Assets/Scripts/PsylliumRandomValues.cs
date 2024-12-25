using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public struct PsylliumRandomValues
    {
        public int timeIndex;
        public Vector3 leftPositionParam;
        public Vector3 rightPositionParam;
        public Vector3 leftRotationParam;
        public Vector3 rightRotationParam;
        public Vector3 basePosition;
        public int leftCount;
        public int rightCount;
        public int[] leftColorIndexes;
        public int[] rightColorIndexes;
        public float timeShift;

        public PsylliumRandomValues(PsylliumHandConfig handConfig, PsylliumAreaConfig areaConfig)
        {
            timeIndex = Random.value.GetHashCode();
            leftPositionParam = GetRandomVector3(-1f, 1f);
            rightPositionParam = GetRandomVector3(-1f, 1f);
            leftRotationParam = GetRandomVector3(-1f, 1f);
            rightRotationParam = GetRandomVector3(-1f, 1f);
            basePosition = areaConfig.randomPosition;

            // 各手のPsylliumの本数をランダムに決定
            leftCount = LotteryByWeight(handConfig.barCountWeights);
            rightCount = LotteryByWeight(handConfig.barCountWeights);
            
            // 最低どちらかに1本は配置
            if (leftCount == 0 && rightCount == 0)
            {
                if (timeIndex < 0f)
                {
                    leftCount = 1;
                }
                else
                {
                    rightCount = 1;
                }
            }

            leftColorIndexes = new int[leftCount];
            rightColorIndexes = new int[rightCount];
            
            // 色を決定
            for (int j = 0; j < leftCount; j++)
            {
                leftColorIndexes[j] = LotteryByWeight(handConfig.colorWeights);
            }
            for (int j = 0; j < rightCount; j++)
            {
                rightColorIndexes[j] = LotteryByWeight(handConfig.colorWeights);
            }

            timeShift = Random.Range(handConfig.timeShiftMin, handConfig.timeShiftMax);

            StepRandom(9); // 乱数の予約
        }

        private static Vector3 GetRandomVector3(float min, float max)
        {
            return new Vector3(
                Random.Range(min, max),
                Random.Range(min, max),
                Random.Range(min, max)
            );
        }

        private static int LotteryByWeight(float[] weights)
        {
            float sum = 0;
            foreach (var weight in weights)
            {
                sum += weight;
            }

            float value = Random.value * sum;
            for (int i = 0; i < weights.Length; i++)
            {
                if (value < weights[i])
                {
                    return i;
                }

                value -= weights[i];
            }

            return weights.Length - 1;
        }

        private static void StepRandom(int step)
        {
            for (var i = 0; i < step; i++)
            {
                var t = Random.value;
            }
        }
    }
}