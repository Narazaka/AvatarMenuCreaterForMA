using UnityEngine;

namespace net.narazaka.avatarmenucreator.animationcurve
{
    public class Vector3Keyframe : ComplexKeyframe<Vector3>
    {
        public Vector3Keyframe() : base() { }
        public Vector3Keyframe(float time, Vector3 value) : base(time, value) { }

        public override float ValueComponent(int index) => Value[index];
    }
}
