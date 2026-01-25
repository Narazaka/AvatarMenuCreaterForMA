namespace net.narazaka.avatarmenucreator
{
    public abstract class RadialValueBase 
    {
        public float StartOffsetPercent;
        public float EndOffsetPercent = 100;

        public float StartFactor(float rate) => EndOffsetPercent - rate * 100;
        public float EndFactor(float rate) => rate * 100 - StartOffsetPercent;
        public float TotalFactor => EndOffsetPercent - StartOffsetPercent;
        public bool IsPreStart(float rate) => rate * 100 < StartOffsetPercent;
        public bool IsPostEnd(float rate) => rate * 100 > EndOffsetPercent;
    }
}
