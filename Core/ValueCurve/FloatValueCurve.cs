#if UNITY_EDITOR
using net.narazaka.avatarmenucreator.value;

namespace net.narazaka.avatarmenucreator.valuecurve
{
    public class FloatValueCurve : ContinuousOneValueCurve
    {
        protected override float ActiveValue => Active.AsFloat();
        protected override float InactiveValue => Inactive.AsFloat();
        public FloatValueCurve(FloatValue inactive, FloatValue active, float transitionOffsetPercent, float transitionDurationPercent) : base(inactive, active, transitionOffsetPercent, transitionDurationPercent) { }
    }
}
#endif
