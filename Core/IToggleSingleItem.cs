#if UNITY_EDITOR
using net.narazaka.avatarmenucreator.valuecurve;
#endif

namespace net.narazaka.avatarmenucreator
{
    public interface IToggleSingleItem
    {
#if UNITY_EDITOR
        IAnimationToggleCurve AnimationToggleCurve();
#endif
    }
}
