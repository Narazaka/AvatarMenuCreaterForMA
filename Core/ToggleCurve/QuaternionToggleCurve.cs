#if UNITY_EDITOR
using UnityEngine;
using net.narazaka.avatarmenucreator.animationcurve;

namespace net.narazaka.avatarmenucreator.valuecurve
{
    public class QuaternionToggleCurve : ContinuousComplexToggleCurve<Quaternion, QuaternionAnimationCurve, QuaternionKeyframe>
    {
        public QuaternionToggleCurve(string prefix, Quaternion inactive, Quaternion active, float transitionOffsetPercent, float transitionDurationPercent) : base(prefix, inactive, active, transitionOffsetPercent, transitionDurationPercent) { }
    }
}
#endif
