#if UNITY_EDITOR
using UnityEngine;
using net.narazaka.avatarmenucreator.value;
using UnityEditor;

namespace net.narazaka.avatarmenucreator.valuecurve
{
    public abstract class ContinuousOneValueCurve : ContinuousValueCurve, IAnimationValueCurve
    {
        protected abstract float ActiveValue { get; }
        protected abstract float InactiveValue { get; }
        public ContinuousOneValueCurve(Value inactive, Value active, float transitionOffsetPercent, float transitionDurationPercent) : base(inactive, active, transitionOffsetPercent, transitionDurationPercent) { }

        public AnimationCurve ActiveCurve() => new AnimationCurve(new Keyframe(0, ActiveValue));
        public AnimationCurve InactiveCurve() => new AnimationCurve(new Keyframe(0, InactiveValue));
        public AnimationCurve ActivateCurve(float transitionSeconds)
        {
            var curve = new AnimationCurve(new Keyframe(transitionSeconds * ActivateStartRate, InactiveValue), new Keyframe(transitionSeconds * ActivateEndRate, ActiveValue));
            if (NeedActivateEndKey) curve.AddKey(transitionSeconds, ActiveValue);
            AnimationUtility.SetKeyLeftTangentMode(curve, 0, AnimationUtility.TangentMode.Constant);
            AnimationUtility.SetKeyRightTangentMode(curve, 0, AnimationUtility.TangentMode.Linear);
            AnimationUtility.SetKeyLeftTangentMode(curve, 1, AnimationUtility.TangentMode.Linear);
            AnimationUtility.SetKeyRightTangentMode(curve, 1, AnimationUtility.TangentMode.Constant);
            return curve;
        }
        public AnimationCurve InactivateCurve(float transitionSeconds)
        {
            var curve = new AnimationCurve(new Keyframe(transitionSeconds * InactivateStartRate, ActiveValue), new Keyframe(transitionSeconds * InactivateEndRate, InactiveValue));
            if (NeedInactivateEndKey) curve.AddKey(transitionSeconds, InactiveValue);
            AnimationUtility.SetKeyLeftTangentMode(curve, 0, AnimationUtility.TangentMode.Constant);
            AnimationUtility.SetKeyRightTangentMode(curve, 0, AnimationUtility.TangentMode.Linear);
            AnimationUtility.SetKeyLeftTangentMode(curve, 1, AnimationUtility.TangentMode.Linear);
            AnimationUtility.SetKeyRightTangentMode(curve, 1, AnimationUtility.TangentMode.Constant);
            return curve;
        }
    }
}
#endif
