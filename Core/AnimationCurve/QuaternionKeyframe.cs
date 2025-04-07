using UnityEngine;

namespace net.narazaka.avatarmenucreator.animationcurve
{
    public class QuaternionKeyframe : ComplexKeyframe<Quaternion>
    {
        public QuaternionKeyframe() : base() { }
        public QuaternionKeyframe(float time, Quaternion value) : base(time, value) { }

        public override float ValueComponent(int index) => Value[index];
    }
}
