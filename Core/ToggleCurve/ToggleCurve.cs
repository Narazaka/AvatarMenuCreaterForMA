namespace net.narazaka.avatarmenucreator.valuecurve
{
    public abstract class ToggleCurve
    {
        public readonly float TransitionOffsetPercent;
        public ToggleCurve(float transitionOffsetPercent)
        {
            TransitionOffsetPercent = transitionOffsetPercent;
        }
        protected float TransitionOffsetRate { get => TransitionOffsetPercent / 100f; }
    }
}
