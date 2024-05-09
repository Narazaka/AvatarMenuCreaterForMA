using UnityEngine;

namespace net.narazaka.avatarmenucreator
{
    public class NamedAnimationCurve
    {
        public string propertyName;
        public AnimationCurve curve;

        public NamedAnimationCurve(string propertyName, AnimationCurve curve)
        {
            this.propertyName = propertyName;
            this.curve = curve;
        }
    }
}
