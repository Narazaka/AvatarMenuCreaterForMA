#if UNITY_EDITOR
using net.narazaka.avatarmenucreator.valuecurve;
using System;
#endif

namespace net.narazaka.avatarmenucreator
{
    public interface IToggleTypedSingleItem
    {
#if UNITY_EDITOR
        IAnimationToggleCurve AnimationToggleCurve(Type type);
#endif
    }
}
