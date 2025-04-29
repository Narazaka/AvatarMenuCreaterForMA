#if UNITY_EDITOR
using System;

namespace net.narazaka.avatarmenucreator.valuecurve
{
    public class NamedAnimationToggleCurveSetup
    {
        readonly INamedAnimationToggleCurve Curve;
        readonly ToggleAnimationClipSet ClipSet;
        readonly string Path;
        readonly Type Type;
        readonly bool UseActive;
        readonly bool UseInactive;
        readonly bool UseTransitionToActive;
        readonly bool UseTransitionToInactive;
        readonly float TransitionSeconds;

        public NamedAnimationToggleCurveSetup(
            INamedAnimationToggleCurve curve,
            ToggleAnimationClipSet clipSet,
            string path,
            Type type,
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
            UseActive = useActive;
            UseInactive = useInactive;
            UseTransitionToActive = useTransitionToActive;
            UseTransitionToInactive = useTransitionToInactive;
            TransitionSeconds = transitionSeconds;
        }

        public void Setup()
        {
            if (UseActive) foreach (var c in Curve.ActiveCurve()) ClipSet.Active.SetCurve(Path, Type, c.propertyName, c.curve);
            if (UseInactive) foreach (var c in Curve.InactiveCurve()) ClipSet.Inactive.SetCurve(Path, Type, c.propertyName, c.curve);
            if (TransitionSeconds > 0)
            {
                if (UseTransitionToActive)
                {
                    foreach (var c in Curve.ActivateCurve(TransitionSeconds)) ClipSet.Activate.SetCurve(Path, Type, c.propertyName, c.curve);
                }
                if (UseTransitionToInactive)
                {
                    foreach (var c in Curve.InactivateCurve(TransitionSeconds)) ClipSet.Inactivate.SetCurve(Path, Type, c.propertyName, c.curve);
                }
            }
        }
    }
}
#endif
