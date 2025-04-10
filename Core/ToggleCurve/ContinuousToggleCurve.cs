﻿namespace net.narazaka.avatarmenucreator.valuecurve
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
        protected bool NeedActivateEndKey { get => 1f - ActivateEndRate >= 1f / 60; }
        protected bool NeedInactivateEndKey { get => 1f - InactivateEndRate >= 1f / 60; }
    }
}
