using net.narazaka.avatarmenucreator.value;

namespace net.narazaka.avatarmenucreator.valuecurve
{
    public class ValueCurve
    {
        public Value Inactive { get; }
        public Value Active { get; }
        public readonly float TransitionOffsetPercent;
        public ValueCurve(Value inactive, Value active, float transitionOffsetPercent)
        {
            Inactive = inactive;
            Active = active;
            TransitionOffsetPercent = transitionOffsetPercent;
        }
        protected float TransitionOffsetRate { get => TransitionOffsetPercent / 100f; }
    }
}
