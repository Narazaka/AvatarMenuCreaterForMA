#if UNITY_EDITOR
using net.narazaka.avatarmenucreator.animationcurve;

namespace net.narazaka.avatarmenucreator.valuecurve
{
    public abstract class ContinuousComplexToggleCurve<T, C, K> : ContinuousToggleCurve, INamedAnimationToggleCurve where C : ComplexAnimationCurve<T, K>, new() where K : ComplexKeyframe<T>, new()
    {
        string Prefix { get; }
        T ActiveValue { get; }
        T InactiveValue { get; }

        public ContinuousComplexToggleCurve(string prefix, T inactive, T active, float transitionOffsetPercent, float transitionDurationPercent) : base(transitionOffsetPercent, transitionDurationPercent)
        {
            ActiveValue = active;
            InactiveValue = inactive;
            Prefix = prefix;
        }

        public NamedAnimationCurve[] ActiveCurve()
        {
            var curve = new C();
            curve.AddKey(0, ActiveValue);
            return curve.GetCurves(Prefix);
        }

        public NamedAnimationCurve[] InactiveCurve()
        {
            var curve = new C();
            curve.AddKey(0, InactiveValue);
            return curve.GetCurves(Prefix);
        }

        public NamedAnimationCurve[] ActivateCurve(float transitionSeconds)
        {
            var curve = new C();
            curve.AddKey(transitionSeconds * ActivateStartRate, InactiveValue);
            curve.AddKey(transitionSeconds * ActivateEndRate, ActiveValue);
            if (NeedActivateEndKey) curve.AddKey(transitionSeconds, ActiveValue);
            return NamedAnimationCurve.SetTangentModes(curve.GetCurves(Prefix));
        }

        public NamedAnimationCurve[] InactivateCurve(float transitionSeconds)
        {
            var curve = new C();
            curve.AddKey(transitionSeconds * InactivateStartRate, ActiveValue);
            curve.AddKey(transitionSeconds * InactivateEndRate, InactiveValue);
            if (NeedInactivateEndKey) curve.AddKey(transitionSeconds, InactiveValue);
            return NamedAnimationCurve.SetTangentModes(curve.GetCurves(Prefix));
        }
    }
}
#endif
