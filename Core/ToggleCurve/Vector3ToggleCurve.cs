#if UNITY_EDITOR
using UnityEngine;
using net.narazaka.avatarmenucreator.animationcurve;

namespace net.narazaka.avatarmenucreator.valuecurve
{
    public class Vector3ToggleCurve : ContinuousToggleCurve, INamedAnimationToggleCurve
    {
        string Prefix { get; }
        Vector3 ActiveValue { get; }
        Vector3 InactiveValue { get; }
        public Vector3ToggleCurve(string prefix, Vector3 inactive, Vector3 active, float transitionOffsetPercent, float transitionDurationPercent) : base(transitionOffsetPercent, transitionDurationPercent)
        {
            ActiveValue = active;
            InactiveValue = inactive;
            Prefix = prefix;
        }

        public NamedAnimationCurve[] ActiveCurve() => new Vector3AnimationCurve(new Vector3Keyframe(0, ActiveValue)).GetCurves(Prefix);
        public NamedAnimationCurve[] InactiveCurve() => new Vector3AnimationCurve(new Vector3Keyframe(0, InactiveValue)).GetCurves(Prefix);
        public NamedAnimationCurve[] ActivateCurve(float transitionSeconds)
        {
            var curve = new Vector3AnimationCurve(new Vector3Keyframe(transitionSeconds * ActivateStartRate, InactiveValue), new Vector3Keyframe(transitionSeconds * ActivateEndRate, ActiveValue));
            if (NeedActivateEndKey) curve.AddKey(transitionSeconds, ActiveValue);
            return NamedAnimationCurve.SetTangentModes(curve.GetCurves(Prefix));
        }
        public NamedAnimationCurve[] InactivateCurve(float transitionSeconds)
        {
            var curve = new Vector3AnimationCurve(new Vector3Keyframe(transitionSeconds * InactivateStartRate, ActiveValue), new Vector3Keyframe(transitionSeconds * InactivateEndRate, InactiveValue));
            if (NeedInactivateEndKey) curve.AddKey(transitionSeconds, InactiveValue);
            return NamedAnimationCurve.SetTangentModes(curve.GetCurves(Prefix));
        }
    }
}
#endif
