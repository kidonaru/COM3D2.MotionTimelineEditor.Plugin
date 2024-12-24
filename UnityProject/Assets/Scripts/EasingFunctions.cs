using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public static class EasingFunctions
    {
        public static float MoveEasing(float t, MoveEasingType type)
        {
            t = Mathf.Clamp01(t);

            switch (type)
            {
                case MoveEasingType.Linear: return Linear(t);
                case MoveEasingType.SineIn: return InSine(t);
                case MoveEasingType.SineOut: return OutSine(t);
                case MoveEasingType.SineInOut: return InOutSine(t);
                case MoveEasingType.QuadIn: return InQuad(t);
                case MoveEasingType.QuadOut: return OutQuad(t);
                case MoveEasingType.QuadInOut: return InOutQuad(t);
                case MoveEasingType.CubicIn: return InCubic(t);
                case MoveEasingType.CubicOut: return OutCubic(t);
                case MoveEasingType.CubicInOut: return InOutCubic(t);
                case MoveEasingType.QuartIn: return InQuart(t);
                case MoveEasingType.QuartOut: return OutQuart(t);
                case MoveEasingType.QuartInOut: return InOutQuart(t);
                case MoveEasingType.QuintIn: return InQuint(t);
                case MoveEasingType.QuintOut: return OutQuint(t);
                case MoveEasingType.QuintInOut: return InOutQuint(t);
                case MoveEasingType.ExpIn: return InExpo(t);
                case MoveEasingType.ExpOut: return OutExpo(t);
                case MoveEasingType.ExpInOut: return InOutExpo(t);
                case MoveEasingType.CircIn: return InCirc(t);
                case MoveEasingType.CircOut: return OutCirc(t);
                case MoveEasingType.CircInOut: return InOutCirc(t);
                default: return Linear(t);
            }
        }

        public static float Linear(float t)
        {
            return t;
        }

        public static float InQuad(float t)
        {
            return t * t;
        }
        public static float OutQuad(float t)
        {
            return 1 - InQuad(1 - t);
        }
        public static float InOutQuad(float t)
        {
            if (t < 0.5f) return InQuad(t * 2) * 0.5f;
            return 1 - InQuad((1 - t) * 2) * 0.5f;
        }

        public static float InCubic(float t)
        {
            return t * t * t;
        }
        public static float OutCubic(float t)
        {
            return 1 - InCubic(1 - t);
        }
        public static float InOutCubic(float t)
        {
            if (t < 0.5f) return InCubic(t * 2) * 0.5f;
            return 1 - InCubic((1 - t) * 2) * 0.5f;
        }

        public static float InQuart(float t)
        {
            return t * t * t * t;
        }
        public static float OutQuart(float t)
        {
            return 1 - InQuart(1 - t);
        }
        public static float InOutQuart(float t)
        {
            if (t < 0.5f) return InQuart(t * 2) * 0.5f;
            return 1 - InQuart((1 - t) * 2) * 0.5f;
        }

        public static float InQuint(float t)
        {
            return t * t * t * t * t;
        }
        public static float OutQuint(float t)
        {
            return 1 - InQuint(1 - t);
        }
        public static float InOutQuint(float t)
        {
            if (t < 0.5f) return InQuint(t * 2) * 0.5f;
            return 1 - InQuint((1 - t) * 2) * 0.5f;
        }

        public static float InSine(float t)
        {
            return 1 - (float)Mathf.Cos(t * Mathf.PI * 0.5f);
        }
        public static float OutSine(float t)
        {
            return Mathf.Sin(t * Mathf.PI * 0.5f);
        }
        public static float InOutSine(float t)
        {
            return -(Mathf.Cos(t * Mathf.PI) - 1) * 0.5f;
        }

        public static float InExpo(float t)
        {
            return Mathf.Pow(2, 10 * (t - 1));
        }
        public static float OutExpo(float t)
        {
            return 1 - InExpo(1 - t);
        }
        public static float InOutExpo(float t)
        {
            if (t < 0.5f) return InExpo(t * 2) * 0.5f;
            return 1 - InExpo((1 - t) * 2) * 0.5f;
        }

        public static float InCirc(float t)
        {
            return -(Mathf.Sqrt(1 - t * t) - 1);
        }
        public static float OutCirc(float t)
        {
            return 1 - InCirc(1 - t);
        }
        public static float InOutCirc(float t)
        {
            if (t < 0.5f) return InCirc(t * 2) * 0.5f;
            return 1 - InCirc((1 - t) * 2) * 0.5f;
        }

        public static float InElastic(float t)
        {
            return 1 - OutElastic(1 - t);
        }
        public static float OutElastic(float t)
        {
            float p = 0.3f;
            return Mathf.Pow(2, -10 * t) * Mathf.Sin((t - p / 4) * (2 * Mathf.PI) / p) + 1;
        }
        public static float InOutElastic(float t)
        {
            if (t < 0.5f) return InElastic(t * 2) * 0.5f;
            return 1 - InElastic((1 - t) * 2) * 0.5f;
        }

        public static float InBack(float t)
        {
            float s = 1.70158f;
            return t * t * ((s + 1) * t - s);
        }
        public static float OutBack(float t)
        {
            return 1 - InBack(1 - t);
        }
        public static float InOutBack(float t)
        {
            if (t < 0.5f) return InBack(t * 2) * 0.5f;
            return 1 - InBack((1 - t) * 2) * 0.5f;
        }

        public static float InBounce(float t)
        {
            return 1 - OutBounce(1 - t);
        }
        public static float OutBounce(float t)
        {
            float div = 2.75f;
            float mult = 7.5625f;

            if (t < 1 / div)
            {
                return mult * t * t;
            }
            else if (t < 2 / div)
            {
                t -= 1.5f / div;
                return mult * t * t + 0.75f;
            }
            else if (t < 2.5f / div)
            {
                t -= 2.25f / div;
                return mult * t * t + 0.9375f;
            }
            else
            {
                t -= 2.625f / div;
                return mult * t * t + 0.984375f;
            }
        }
        public static float InOutBounce(float t)
        {
            if (t < 0.5f) return InBounce(t * 2) * 0.5f;
            return 1 - InBounce((1 - t) * 2) * 0.5f;
        }
    }
}
