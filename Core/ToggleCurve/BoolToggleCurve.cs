#if UNITY_EDITOR
namespace net.narazaka.avatarmenucreator.valuecurve
{
    public class BoolToggleCurve : DiscreteSingleToggleCurve
    {
        public BoolToggleCurve(bool inactive, bool active, float transitionOffsetPercent) : base(inactive ? 1 : 0, active ? 1 : 0, transitionOffsetPercent) { }
    }
}
#endif
