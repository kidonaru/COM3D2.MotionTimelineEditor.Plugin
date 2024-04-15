namespace COM3D2.MotionTimelineEditor.Plugin
{
    public struct TangentPair
    {
        public float outTangent;
        public float inTangent;
        public bool isSmooth;

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            var other = (TangentPair)obj;
            return outTangent == other.outTangent
                && inTangent == other.inTangent
                && isSmooth == other.isSmooth;
        }

        public override int GetHashCode()
        {
            return outTangent.GetHashCode() ^ inTangent.GetHashCode() ^ isSmooth.GetHashCode();
        }

        public static TangentPair GetDefault(TangentType tangentType)
        {
            switch (tangentType)
            {
                case TangentType.EaseInOut:
                    return new TangentPair
                    {
                        outTangent = 0f,
                        inTangent = 0f,
                    };
                case TangentType.EaseIn:
                    return new TangentPair
                    {
                        outTangent = 1f,
                        inTangent = 0f,
                    };
                case TangentType.EaseOut:
                    return new TangentPair
                    {
                        outTangent = 0f,
                        inTangent = 1f,
                    };
                case TangentType.Linear:
                    return new TangentPair
                    {
                        outTangent = 1f,
                        inTangent = 1f,
                    };
                case TangentType.Smooth:
                    return new TangentPair
                    {
                        isSmooth = true,
                    };
            }

            return new TangentPair();
        }
    }
}