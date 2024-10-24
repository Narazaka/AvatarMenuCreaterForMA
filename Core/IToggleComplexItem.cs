#if UNITY_EDITOR
using net.narazaka.avatarmenucreator.valuecurve;
#endif

namespace net.narazaka.avatarmenucreator
{
    public interface IToggleComplexItem
    {
#if UNITY_EDITOR
        INamedAnimationToggleCurve ComplexAnimationToggleCurve(string prefix);
#endif
    }
}
