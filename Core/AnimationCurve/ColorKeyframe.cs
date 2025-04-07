using UnityEngine;

namespace net.narazaka.avatarmenucreator.animationcurve
{
    public class ColorKeyframe : ComplexKeyframe<Color>
    {
        public ColorKeyframe() : base() { }
        public ColorKeyframe(float time, Color value) : base(time, value) { }

        public override float ValueComponent(int index) => Value[index];
    }
}
