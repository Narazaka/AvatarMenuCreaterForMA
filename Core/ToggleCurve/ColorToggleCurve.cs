#if UNITY_EDITOR
using UnityEngine;
using net.narazaka.avatarmenucreator.animationcurve;

namespace net.narazaka.avatarmenucreator.valuecurve
{
    public class ColorToggleCurve : ContinuousComplexToggleCurve<Color, ColorAnimationCurve, ColorKeyframe>
    {
        public ColorToggleCurve(string prefix, Color inactive, Color active, float transitionOffsetPercent, float transitionDurationPercent) : base(prefix, inactive, active, transitionOffsetPercent, transitionDurationPercent) { }
    }
}
#endif
