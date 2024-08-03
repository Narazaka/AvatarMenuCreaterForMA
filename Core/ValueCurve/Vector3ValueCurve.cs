#if UNITY_EDITOR
using UnityEngine;
using net.narazaka.avatarmenucreator.value;

namespace net.narazaka.avatarmenucreator.valuecurve
{
    public class Vector3ValueCurve : ContinuousValueCurve
    {
        public string Prefix { get; }
        protected Vector3 ActiveValue { get => Active.AsVector3(); }
        protected Vector3 InactiveValue { get => Inactive.AsVector3(); }
        public Vector3ValueCurve(Vector3Value inactive, Vector3Value active, float transitionOffsetPercent, float transitionDurationPercent, string prefix) : base(inactive, active, transitionOffsetPercent, transitionDurationPercent)
        {
            Prefix = prefix;
        }

        public NamedAnimationCurve[] ActiveCurve() => new Vector3AnimationCurve(new Vector3Keyframe(0, ActiveValue)).GetCurves(Prefix);
        public NamedAnimationCurve[] InactiveCurve() => new Vector3AnimationCurve(new Vector3Keyframe(0, InactiveValue)).GetCurves(Prefix);
        public NamedAnimationCurve[] ActivateCurve(float transitionSeconds)
        {
            var curve = new Vector3AnimationCurve(new Vector3Keyframe(transitionSeconds * ActivateStartRate, InactiveValue), new Vector3Keyframe(transitionSeconds * ActivateEndRate, ActiveValue));
            if (NeedActivateEndKey) curve.AddKey(transitionSeconds, ActiveValue);
            return Vector3AnimationCurve.SetTangentModes(curve.GetCurves(Prefix));
        }
        public NamedAnimationCurve[] InactivateCurve(float transitionSeconds)
        {
            var curve = new Vector3AnimationCurve(new Vector3Keyframe(transitionSeconds * InactivateStartRate, ActiveValue), new Vector3Keyframe(transitionSeconds * InactivateEndRate, InactiveValue));
            if (NeedInactivateEndKey) curve.AddKey(transitionSeconds, InactiveValue);
            return Vector3AnimationCurve.SetTangentModes(curve.GetCurves(Prefix));
        }
    }
}
#endif
