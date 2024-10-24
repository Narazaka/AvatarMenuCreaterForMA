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
        readonly float TransitionSeconds;

        public NamedAnimationToggleCurveSetup(
            INamedAnimationToggleCurve curve,
            ToggleAnimationClipSet clipSet,
            string path,
            Type type,
            bool useActive,
            bool useInactive,
            float transitionSeconds
            )
        {
            Curve = curve;
            ClipSet = clipSet;
            Path = path;
            Type = type;
            UseActive = useActive;
            UseInactive = useInactive;
            TransitionSeconds = transitionSeconds;
        }

        public void Setup()
        {
            if (UseActive) foreach (var c in Curve.ActiveCurve()) ClipSet.Active.SetCurve(Path, Type, c.propertyName, c.curve);
            if (UseInactive) foreach (var c in Curve.InactiveCurve()) ClipSet.Inactive.SetCurve(Path, Type, c.propertyName, c.curve);
            if (TransitionSeconds > 0)
            {
                foreach (var c in Curve.ActivateCurve(TransitionSeconds)) ClipSet.Activate.SetCurve(Path, Type, c.propertyName, c.curve);
                foreach (var c in Curve.InactivateCurve(TransitionSeconds)) ClipSet.Inactivate.SetCurve(Path, Type, c.propertyName, c.curve);
            }
        }
    }
}
#endif
