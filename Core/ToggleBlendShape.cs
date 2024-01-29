using System;

namespace net.narazaka.avatarmenucreator
{
    [Serializable]
    public struct ToggleBlendShape : System.IEquatable<ToggleBlendShape>
    {
        public float Inactive;
        public float Active;
        public float TransitionOffsetPercent;
        public float TransitionDurationPercent;

        public bool Equals(ToggleBlendShape other)
        {
            return Inactive == other.Inactive && Active == other.Active && TransitionOffsetPercent == other.TransitionOffsetPercent && TransitionDurationPercent == other.TransitionDurationPercent;
        }

        public string ChangedProp(ToggleBlendShape other)
        {
            if (Inactive != other.Inactive) return nameof(Inactive);
            if (Active != other.Active) return nameof(Active);
            if (TransitionOffsetPercent != other.TransitionOffsetPercent) return nameof(TransitionOffsetPercent);
            if (TransitionDurationPercent != other.TransitionDurationPercent) return nameof(TransitionDurationPercent);
            return "";
        }

        public float GetProp(string name)
        {
            if (name == nameof(Inactive)) return Inactive;
            if (name == nameof(Active)) return Active;
            if (name == nameof(TransitionOffsetPercent)) return TransitionOffsetPercent;
            if (name == nameof(TransitionDurationPercent)) return TransitionDurationPercent;
            return 0;
        }

        public ToggleBlendShape SetProp(string name, float value)
        {
            if (name == nameof(Inactive)) Inactive = value;
            if (name == nameof(Active)) Active = value;
            if (name == nameof(TransitionOffsetPercent)) TransitionOffsetPercent = value;
            if (name == nameof(TransitionDurationPercent)) TransitionDurationPercent = value;
            return this;
        }

        public float TransitionOffsetRate { get => TransitionOffsetPercent / 100f; }
        public float TransitionDurationRate { get => TransitionDurationPercent / 100f; }
        public float ActivateStartRate { get => TransitionOffsetRate; }
        public float ActivateEndRate { get => TransitionOffsetRate + TransitionDurationRate; }
        public float InactivateStartRate { get => 1f - ActivateEndRate; }
        public float InactivateEndRate { get => 1f - ActivateStartRate; }
    }
}
