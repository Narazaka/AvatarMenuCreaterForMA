#if UNITY_EDITOR
using net.narazaka.avatarmenucreator.animationcurve;

namespace net.narazaka.avatarmenucreator.valuecurve
{
    public class DiscreteComplexToggleCurve<T, C, K> : DiscreteToggleCurve, INamedAnimationToggleCurve where C : ComplexAnimationCurve<T, K>, new() where K : ComplexKeyframe<T>, new()
    {
        string Prefix { get; }
        T ActiveValue { get; }
        T InactiveValue { get; }

        public DiscreteComplexToggleCurve(string prefix, T inactive, T active, float transitionOffsetPercent) : base(transitionOffsetPercent)
        {
            Prefix = prefix;
            ActiveValue = active;
            InactiveValue = inactive;
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
            if (NeedActivateStartKey) curve.AddKey(0, InactiveValue);
            curve.AddKey(transitionSeconds * ActivateChangeRate, ActiveValue);
            if (NeedActivateEndKey) curve.AddKey(transitionSeconds, ActiveValue);
            return NamedAnimationCurve.SetTangentModesConstant(curve.GetCurves(Prefix));
        }

        public NamedAnimationCurve[] InactivateCurve(float transitionSeconds)
        {
            var curve = new C();
            if (NeedInactivateStartKey) curve.AddKey(0, ActiveValue);
            curve.AddKey(transitionSeconds * InactivateChangeRate, InactiveValue);
            if (NeedInactivateEndKey) curve.AddKey(transitionSeconds, InactiveValue);
            return NamedAnimationCurve.SetTangentModesConstant(curve.GetCurves(Prefix));
        }
    }
}
#endif
