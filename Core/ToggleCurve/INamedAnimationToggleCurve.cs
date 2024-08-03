using net.narazaka.avatarmenucreator.animationcurve;

namespace net.narazaka.avatarmenucreator.valuecurve
{
    public interface INamedAnimationToggleCurve
    {
        NamedAnimationCurve[] ActiveCurve();
        NamedAnimationCurve[] InactiveCurve();
        NamedAnimationCurve[] ActivateCurve(float transitionSeconds);
        NamedAnimationCurve[] InactivateCurve(float transitionSeconds);
    }
}
