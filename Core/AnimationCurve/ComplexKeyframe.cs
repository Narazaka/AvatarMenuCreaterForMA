namespace net.narazaka.avatarmenucreator.animationcurve
{
    public abstract class ComplexKeyframe<T>
    {
        public float Time;
        public T Value;

        public ComplexKeyframe() { }

        public ComplexKeyframe(float time, T value)
        {
            Time = time;
            Value = value;
        }

        public abstract float ValueComponent(int index);
    }
}
