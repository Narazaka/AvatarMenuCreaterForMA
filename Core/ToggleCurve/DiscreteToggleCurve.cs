namespace net.narazaka.avatarmenucreator.valuecurve
{
    public abstract class DiscreteToggleCurve : ToggleCurve
    {
        public DiscreteToggleCurve(float transitionOffsetPercent) : base(transitionOffsetPercent) { }

        protected float ActivateChangeRate { get => TransitionOffsetRate; }
        protected float InactivateChangeRate { get => 1f - TransitionOffsetRate; }
        protected bool NeedActivateStartKey { get => ActivateChangeRate > 0; }
        protected bool NeedActivateEndKey { get => 1f - ActivateChangeRate > 0; }
        protected bool NeedInactivateStartKey { get => InactivateChangeRate > 0; }
        protected bool NeedInactivateEndKey { get => 1f - InactivateChangeRate > 0; }
    }
}
