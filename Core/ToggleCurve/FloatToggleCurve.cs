#if UNITY_EDITOR
namespace net.narazaka.avatarmenucreator.valuecurve
{
    public class FloatToggleCurve : ContinuousSingleToggleCurve
    {
        public FloatToggleCurve(float inactive, float active, float transitionOffsetPercent, float transitionDurationPercent) : base(inactive, active, transitionOffsetPercent, transitionDurationPercent) { }
    }
}
#endif
