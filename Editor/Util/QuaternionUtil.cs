using UnityEngine;

namespace net.narazaka.avatarmenucreator.util
{
    public static class QuaternionUtil
    {
        public static Vector4 ToVector4(this Quaternion value)
        {
            return new Vector4(value.x, value.y, value.z, value.w);
        }

        public static Quaternion ToQuaternion(this Vector4 value)
        {
            return new Quaternion(value.x, value.y, value.z, value.w);
        }
    }
}
