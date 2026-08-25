namespace net.narazaka.avatarmenucreator.valuecurve
{
    public abstract class ContinuousToggleCurve : ToggleCurve
    {
        public readonly float TransitionDurationPercent;
        public ContinuousToggleCurve(float transitionOffsetPercent, float transitionDurationPercent) : base(transitionOffsetPercent)
        {
            TransitionDurationPercent = transitionDurationPercent;
        }

        protected float TransitionDurationRate { get => TransitionDurationPercent / 100f; }
        protected float ActivateStartRate { get => TransitionOffsetRate; }
        protected float ActivateEndRate { get => TransitionOffsetRate + TransitionDurationRate; }
        protected float InactivateStartRate { get => 1f - ActivateEndRate; }
        protected float InactivateEndRate { get => 1f - ActivateStartRate; }
        protected bool NeedActivateEndKey(float transitionSeconds) => (1f - ActivateEndRate) * transitionSeconds >= 1f / 60;
        protected bool NeedInactivateEndKey(float transitionSeconds) => (1f - InactivateEndRate) * transitionSeconds >= 1f / 60;
    }
}
