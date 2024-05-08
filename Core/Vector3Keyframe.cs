using UnityEngine;

namespace net.narazaka.avatarmenucreator
{
    public class Vector3Keyframe
    {
        public float Time;
        public Vector3 Value;

        public Vector3Keyframe(float time, Vector3 value)
        {
            Time = time;
            Value = value;
        }
    }
}
