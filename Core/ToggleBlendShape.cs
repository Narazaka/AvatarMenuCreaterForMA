using System;
using UnityEngine;
using net.narazaka.avatarmenucreator.valuecurve;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace net.narazaka.avatarmenucreator
{
    [Serializable]
    public class ToggleBlendShape : ToggleItemBase<float>, IToggleSingleItem
    {
#if UNITY_EDITOR
        public IAnimationToggleCurve AnimationToggleCurve() => new FloatToggleCurve(Inactive, Active, TransitionOffsetPercent, TransitionDurationPercent);
#endif
    }
}
