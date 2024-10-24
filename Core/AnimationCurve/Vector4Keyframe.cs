using UnityEngine;

namespace net.narazaka.avatarmenucreator.animationcurve
{
    public class Vector4Keyframe : ComplexKeyframe<Vector4>
    {
        public Vector4Keyframe() : base() { }
        public Vector4Keyframe(float time, Vector4 value) : base(time, value) { }

        public override float ValueComponent(int index) => Value[index];
    }
}
