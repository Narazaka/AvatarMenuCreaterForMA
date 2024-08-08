using System.Runtime.CompilerServices;

namespace net.narazaka.avatarmenucreator.value
{
    public static class ValueExtension
    {
        public static BoolValue AsBool(this Value value) => Unsafe.As<Value, BoolValue>(ref value);
        public static FloatValue AsFloat(this Value value) => Unsafe.As<Value, FloatValue>(ref value);
        public static IntValue AsInt(this Value value) => Unsafe.As<Value, IntValue>(ref value);
        public static Vector3Value AsVector3(this Value value) => Unsafe.As<Value, Vector3Value>(ref value);
        public static PermissionFilterValue AsPermissionFilterValue(this Value value) => Unsafe.As<Value, PermissionFilterValue>(ref value);
    }
}
