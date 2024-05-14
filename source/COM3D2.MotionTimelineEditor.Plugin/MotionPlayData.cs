using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public interface IMotionData
    {
        int stFrame { get; set; }
        int edFrame { get; set; }
    }

    public class MotionPlayData<T> where T : IMotionData
    {
        public int listIndex = -1;
        public float lerpFrame = 0f;
        public float prevPlayingFrame = 0f;
        public List<T> motions = new List<T>();
        public T current = default(T);

        public void ResetIndex()
        {
            listIndex = -1;
            lerpFrame = 0f;
            prevPlayingFrame = 0f;
            current = default(T);
        }

        public bool Update(float playingFrame)
        {
            if (motions.Count == 0)
            {
                return false;
            }

            if (playingFrame < prevPlayingFrame)
            {
                ResetIndex();
            }

            bool indexUpdated = false;

            while (listIndex + 1 < motions.Count &&
                playingFrame >= motions[listIndex + 1].stFrame)
            {
                listIndex++;
                indexUpdated = true;
            }

            if (listIndex < 0)
            {
                // 開始フレーム待ち
            }
            else if (listIndex < motions.Count)
            {
                current = motions[listIndex];
                lerpFrame = CalcLerpFrame(
                    playingFrame,
                    current.stFrame,
                    current.edFrame);
            }
            else
            {
                current = motions[motions.Count - 1];
                lerpFrame = 1f;
            }

            prevPlayingFrame = playingFrame;
            return indexUpdated;
        }

        public static float CalcLerpFrame(
            float frameFloat,
            int stFrame,
            int edFrame)
        {
            float deltaFrame = edFrame - stFrame;

            float lerpFrame;
            if (deltaFrame == 0)
            {
                lerpFrame = 0f;
            }
            else
            {
                lerpFrame = (float)(frameFloat - stFrame) / deltaFrame;
            }
            return Mathf.Clamp01(lerpFrame);
        }
    }
}