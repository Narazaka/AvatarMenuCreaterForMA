#if UNITY_EDITOR
namespace net.narazaka.avatarmenucreator.valuecurve
{
    public class IntToggleCurve : DiscreteSingleToggleCurve
    {
        public IntToggleCurve(int inactive, int active, float transitionOffsetPercent) : base(inactive, active, transitionOffsetPercent) { }
    }
}
#endif
