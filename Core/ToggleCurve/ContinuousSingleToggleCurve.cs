#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace net.narazaka.avatarmenucreator.valuecurve
{
    public abstract class ContinuousSingleToggleCurve : ContinuousToggleCurve, IAnimationToggleCurve
    {
        float ActiveValue { get; }
        float InactiveValue { get; }
        public ContinuousSingleToggleCurve(float inactive, float active, float transitionOffsetPercent, float transitionDurationPercent) : base(transitionOffsetPercent, transitionDurationPercent)
        {
            ActiveValue = active;
            InactiveValue = inactive;
        }

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
