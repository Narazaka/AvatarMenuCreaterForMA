using System;
using System.Reflection;
using UnityEngine;

namespace net.narazaka.avatarmenucreator.util
{
    [Serializable]
    public class TypeMember : INameAndDescription, ISerializationCallbackReceiver, IEquatable<TypeMember>
    {
        public static TypeMember FromName(string name)
        {
            var split = name.Split('\t');
            if (split.Length != 2)
            {
                return null;
            }
            var type = TypeUtil.GetType(split[0]);
            if (type == null)
            {
                return null;
            }
            return new TypeMember(type, split[1]);
        }

        public Type Type { get; private set; }
        public MemberInfo Member { get; private set; }
        [SerializeField] public string TypeName;
        [SerializeField] public string MemberName;
        public Type MemberType
        {
            get => Member.MemberType == MemberTypes.Field ? ((FieldInfo)Member).FieldType : ((PropertyInfo)Member).PropertyType;
        }
        public string Name { get => $"{TypeName}\t{MemberName}"; }
        public string Description { get => $"{Type.Name}.{MemberName}"; }
        public TypeMember() { } // for serialization

        public TypeMember(string typeName, string memberName) : this(TypeMemberUtil.GetMember(TypeUtil.GetType(typeName), memberName)) { }
        public TypeMember(Type type, string memberName) : this(TypeMemberUtil.GetMember(type, memberName)) { }

        public TypeMember(MemberInfo memberInfo)
        {
            Type = memberInfo.ReflectedType;
            Member = memberInfo;
            TypeName = TypeUtil.GetTypeName(Type);
            MemberName = Member.Name;
        }

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            if (Type == null || Member == null)
            {
                Type = TypeUtil.GetType(TypeName);
                Member = TypeMemberUtil.GetMember(Type, MemberName);
            }
        }

        public bool Equals(TypeMember other)
        {
            return Type == other.Type && Member == other.Member;
        }

        public override bool Equals(object obj)
        {
            return Equals((TypeMember)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Type, Member);
        }

        public static bool operator ==(TypeMember left, TypeMember right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (ReferenceEquals(left, null)) return false;
            return left.Equals(right);
        }

        public static bool operator !=(TypeMember left, TypeMember right)
        {
            return !(left == right);
        }
    }
}
