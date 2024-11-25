using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class PlayDataBase<T> where T : class, IMotionData
    {
        private const float FRAME_TOLERANCE = 0.001f; // 誤差許容値

        public int listIndex = -1;
        public float lerpFrame = 0f;
        public float prevPlayingFrame = 0f;
        public List<T> motions = new List<T>();
        public T current = null;

        public void ResetIndex()
        {
            listIndex = -1;
            lerpFrame = 0f;
            prevPlayingFrame = 0f;
            current = null;
        }

        public bool Update(float playingFrame)
        {
            if (motions.Count == 0)
            {
                return false;
            }

            if (current != null && (int) playingFrame < current.stFrameActive - FRAME_TOLERANCE)
            {
                ResetIndex();
            }

            if (playingFrame < prevPlayingFrame)
            {
                ResetIndex();
            }

            bool indexUpdated = false;

            while (listIndex + 1 < motions.Count &&
                playingFrame >= motions[listIndex + 1].stFrameActive - FRAME_TOLERANCE)
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
                    current.stFrameActive,
                    current.edFrameActive);
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

        public void Setup(SingleFrameType singleFrameType)
        {
            foreach (var motion in motions)
            {
                motion.stFrameInEdit = motion.stFrame;
                motion.edFrameInEdit = motion.edFrame;
            }

            if (singleFrameType == SingleFrameType.None)
            {
                return;
            }

            for (var i = 0; i < motions.Count - 1; i++)
            {
                var prev = i > 1 ? motions[i - 1] : null;
                var current = motions[i];
                var next = motions[i + 1];

                if (current.stFrame + 1 == next.stFrame)
                {
                    if (singleFrameType == SingleFrameType.Delay)
                    {
                        if (prev != null)
                        {
                            prev.edFrame = current.edFrame;
                        }
                    }
                    if (singleFrameType == SingleFrameType.Advance)
                    {
                        next.stFrame = current.stFrame;
                    }
                    motions.Remove(current);
                    i--;
                }
            }
        }
    }

    public class MotionPlayData : PlayDataBase<MotionData>
    {
        public MotionPlayData(int capacity)
        {
            motions = new List<MotionData>(capacity);
        }
    }
}