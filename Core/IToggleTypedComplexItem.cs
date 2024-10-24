#if UNITY_EDITOR
using net.narazaka.avatarmenucreator.valuecurve;
using System;
#endif

namespace net.narazaka.avatarmenucreator
{
    public interface IToggleTypedComplexItem
    {
#if UNITY_EDITOR
        INamedAnimationToggleCurve ComplexAnimationToggleCurve(Type type, string prefix);
#endif
    }
}
