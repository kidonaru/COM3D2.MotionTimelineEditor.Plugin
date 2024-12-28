using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public struct PsylliumRandomValues
    {
        public int timeIndex;
        public int leftRandomPositionIndex;
        public int rightRandomPositionIndex;
        public int leftRandomRotationIndex;
        public int rightRandomRotationIndex;
        public Vector3 basePosition;
        public int leftCount;
        public int rightCount;
        public int[] leftColorIndexes;
        public int[] rightColorIndexes;
        public float timeShiftParam;

        public PsylliumRandomValues(PsylliumAreaConfig areaConfig)
        {
            timeIndex = Random.Range(1, int.MaxValue);
            leftRandomPositionIndex = Random.Range(1, int.MaxValue);
            rightRandomPositionIndex = Random.Range(1, int.MaxValue);
            leftRandomRotationIndex = Random.Range(1, int.MaxValue);
            rightRandomRotationIndex = Random.Range(1, int.MaxValue);
            basePosition = areaConfig.randomPosition;

            // 各手のPsylliumの本数をランダムに決定
            leftCount = LotteryByWeight(areaConfig.barCountWeights);
            rightCount = LotteryByWeight(areaConfig.barCountWeights);

            // 何も持っていない場合右手に1本持たせる
            if (leftCount == 0 && rightCount == 0)
            {
                rightCount = 1;
            }

            leftColorIndexes = new int[leftCount];
            rightColorIndexes = new int[rightCount];
            
            // 色を決定
            for (int j = 0; j < leftCount; j++)
            {
                leftColorIndexes[j] = LotteryByWeight(areaConfig.colorWeights);
            }
            for (int j = 0; j < rightCount; j++)
            {
                rightColorIndexes[j] = LotteryByWeight(areaConfig.colorWeights);
            }

            timeShiftParam = Random.value;

            StepRandom(8); // 乱数の予約
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