#if UNITY_EDITOR
using System;

namespace net.narazaka.avatarmenucreator.valuecurve
{
    public class AnimationToggleCurveSetup
    {
        readonly IAnimationToggleCurve Curve;
        readonly ToggleAnimationClipSet ClipSet;
        readonly string Path;
        readonly Type Type;
        readonly string PropertyName;
        readonly bool UseActive;
        readonly bool UseInactive;
        readonly bool UseTransitionToActive;
        readonly bool UseTransitionToInactive;
        readonly float TransitionSeconds;

        public AnimationToggleCurveSetup(
            IAnimationToggleCurve curve,
            ToggleAnimationClipSet clipSet,
            string path,
            Type type,
            string propertyName,
            bool useActive,
            bool useInactive,
            bool useTransitionToActive,
            bool useTransitionToInactive,
            float transitionSeconds
            )
        {
            Curve = curve;
            ClipSet = clipSet;
            Path = path;
            Type = type;
            PropertyName = propertyName;
            UseActive = useActive;
            UseInactive = useInactive;
            UseTransitionToActive = useTransitionToActive;
            UseTransitionToInactive = useTransitionToInactive;
            TransitionSeconds = transitionSeconds;
        }

        public void Setup()
        {
            if (UseActive) ClipSet.Active.SetCurve(Path, Type, PropertyName, Curve.ActiveCurve());
            if (UseInactive) ClipSet.Inactive.SetCurve(Path, Type, PropertyName, Curve.InactiveCurve());
            if (TransitionSeconds > 0)
            {
                if (UseTransitionToActive) ClipSet.Activate.SetCurve(Path, Type, PropertyName, Curve.ActivateCurve(TransitionSeconds));
                if (UseTransitionToInactive) ClipSet.Inactivate.SetCurve(Path, Type, PropertyName, Curve.InactivateCurve(TransitionSeconds));
            }
        }
    }
}
#endif
