#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace net.narazaka.avatarmenucreator.valuecurve
{
    public abstract class DiscreteSingleToggleCurve : DiscreteToggleCurve, IAnimationToggleCurve
    {
        float ActiveValue { get; }
        float InactiveValue { get; }
        public DiscreteSingleToggleCurve(float inactive, float active, float transitionOffsetPercent) : base(transitionOffsetPercent)
        {
            ActiveValue = active;
            InactiveValue = inactive;
        }

        public AnimationCurve ActiveCurve() => new AnimationCurve(new Keyframe(0 / 60.0f, ActiveValue));
        public AnimationCurve InactiveCurve() => new AnimationCurve(new Keyframe(0 / 60.0f, InactiveValue));
        public AnimationCurve ActivateCurve(float transitionSeconds)
        {
            var curve = new AnimationCurve();
            if (NeedActivateStartKey) curve.AddKey(new Keyframe(0, InactiveValue));
            curve.AddKey(new Keyframe(transitionSeconds * ActivateChangeRate, ActiveValue));
            if (NeedActivateEndKey) curve.AddKey(new Keyframe(transitionSeconds, ActiveValue));
            for (var i = 0; i < curve.length; i++)
            {
                AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.Constant);
                AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.Constant);
            }
            return curve;
        }
        public AnimationCurve InactivateCurve(float transitionSeconds)
        {
            var curve = new AnimationCurve();
            if (NeedInactivateStartKey) curve.AddKey(new Keyframe(0, ActiveValue));
            curve.AddKey(new Keyframe(transitionSeconds * InactivateChangeRate, InactiveValue));
            if (NeedInactivateEndKey) curve.AddKey(new Keyframe(transitionSeconds, InactiveValue));
            for (var i = 0; i < curve.length; i++)
            {
                AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.Constant);
                AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.Constant);
            }
            return curve;
        }
    }
}
#endif
