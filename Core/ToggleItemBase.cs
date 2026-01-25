using System;
using System.Collections.Generic;
using UnityEngine;

namespace net.narazaka.avatarmenucreator
{
    [Serializable]
    public abstract class ToggleItemBase<Item> : System.IEquatable<ToggleItemBase<Item>>, IUseActive
    {
        public Item Inactive;
        public Item Active;
        public float TransitionOffsetPercent;
        public float TransitionDurationPercent = 100;
        [SerializeField]
        bool OmitInactive;
        [SerializeField]
        bool OmitActive;
        [SerializeField]
        bool OmitTransitionToInactive;
        [SerializeField]
        bool OmitTransitionToActive;

        public bool UseInactive
        {
            get => !OmitInactive;
            set => OmitInactive = !value;
        }
        public bool UseActive
        {
            get => !OmitActive;
            set => OmitActive = !value;
        }
        public bool UseTransitionToInactive
        {
            get => !OmitTransitionToInactive;
            set => OmitTransitionToInactive = !value;
        }
        public bool UseTransitionToActive
        {
            get => !OmitTransitionToActive;
            set => OmitTransitionToActive = !value;
        }
        public bool HasAdvanced => OmitInactive || OmitActive || OmitTransitionToInactive || OmitTransitionToActive;
        public void ResetAdvanced()
        {
            OmitInactive = OmitActive = OmitTransitionToInactive = OmitTransitionToActive = false;
        }

        public bool Equals(ToggleItemBase<Item> other)
        {
            return other != null && Inactive.Equals(other.Inactive) && Active.Equals(other.Active) && TransitionOffsetPercent == other.TransitionOffsetPercent && TransitionDurationPercent == other.TransitionDurationPercent && UseInactive == other.UseInactive && UseActive == other.UseActive && UseTransitionToInactive == other.UseTransitionToInactive && UseTransitionToActive == other.UseTransitionToActive;
        }

        public IEnumerable<string> ChangedProps(ToggleItemBase<Item> other)
        {
            var changed = new List<string>();
            if (!Inactive.Equals(other.Inactive)) changed.Add(nameof(Inactive));
            if (!Active.Equals(other.Active)) changed.Add(nameof(Active));
            if (TransitionOffsetPercent != other.TransitionOffsetPercent) changed.Add(nameof(TransitionOffsetPercent));
            if (TransitionDurationPercent != other.TransitionDurationPercent) changed.Add(nameof(TransitionDurationPercent));
            if (UseInactive != other.UseInactive) changed.Add(nameof(UseInactive));
            if (UseActive != other.UseActive) changed.Add(nameof(UseActive));
            if (UseTransitionToInactive != other.UseTransitionToInactive) changed.Add(nameof(UseTransitionToInactive));
            if (UseTransitionToActive != other.UseTransitionToActive) changed.Add(nameof(UseTransitionToActive));
            return changed;
        }

        public object GetProp(string name)
        {
            if (name == nameof(Inactive)) return Inactive;
            if (name == nameof(Active)) return Active;
            if (name == nameof(TransitionOffsetPercent)) return TransitionOffsetPercent;
            if (name == nameof(TransitionDurationPercent)) return TransitionDurationPercent;
            if (name == nameof(UseInactive)) return UseInactive;
            if (name == nameof(UseActive)) return UseActive;
            if (name == nameof(UseTransitionToInactive)) return UseTransitionToInactive;
            if (name == nameof(UseTransitionToActive)) return UseTransitionToActive;
            return 0;
        }

        public void SetProp(string name, object value)
        {
            if (name == nameof(Inactive)) Inactive = (Item)value;
            if (name == nameof(Active)) Active = (Item)value;
            if (name == nameof(TransitionOffsetPercent)) TransitionOffsetPercent = (float)value;
            if (name == nameof(TransitionDurationPercent)) TransitionDurationPercent = (float)value;
            if (name == nameof(UseInactive)) UseInactive = (bool)value;
            if (name == nameof(UseActive)) UseActive = (bool)value;
            if (name == nameof(UseTransitionToInactive)) UseTransitionToInactive = (bool)value;
            if (name == nameof(UseTransitionToActive)) UseTransitionToActive = (bool)value;
        }

        public void AdjustTransitionValues()
        {
            if (TransitionOffsetPercent < 0) TransitionOffsetPercent = 0;
            if (TransitionOffsetPercent > 100) TransitionOffsetPercent = 100;
            if (TransitionDurationPercent <= 0) TransitionDurationPercent = 1;
            if (TransitionDurationPercent > 100) TransitionDurationPercent = 100;
            if (TransitionOffsetPercent + TransitionDurationPercent > 100) TransitionDurationPercent = 100 - TransitionOffsetPercent;
        }
    }
}
