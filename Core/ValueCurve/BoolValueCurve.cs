#if UNITY_EDITOR
using net.narazaka.avatarmenucreator.value;

namespace net.narazaka.avatarmenucreator.valuecurve
{
    public class BoolValueCurve : DiscreteOneValueCurve
    {
        protected override float ActiveValue => (bool)Active ? 1 : 0;
        protected override float InactiveValue => (bool)Inactive ? 1 : 0;
        public BoolValueCurve(BoolValue inactive, BoolValue active, float transitionOffsetPercent) : base(inactive, active, transitionOffsetPercent) { }
    }
}
#endif
