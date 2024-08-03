using UnityEngine;

namespace net.narazaka.avatarmenucreator.valuecurve
{
    public interface IAnimationToggleCurve
    {
        AnimationCurve ActiveCurve();
        AnimationCurve InactiveCurve();
        AnimationCurve ActivateCurve(float transitionSeconds);
        AnimationCurve InactivateCurve(float transitionSeconds);
    }
}
