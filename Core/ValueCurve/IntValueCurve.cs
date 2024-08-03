#if UNITY_EDITOR
using net.narazaka.avatarmenucreator.value;

namespace net.narazaka.avatarmenucreator.valuecurve
{
    public class IntValueCurve : DiscreteOneValueCurve
    {
        protected override float ActiveValue => (int)Active;
        protected override float InactiveValue => (int)Inactive;
        public IntValueCurve(IntValue inactive, IntValue active, float transitionOffsetPercent) : base(inactive, active, transitionOffsetPercent) { }
    }
}
#endif
