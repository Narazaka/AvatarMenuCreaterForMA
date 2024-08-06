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

        /// <summary>member class</summary>
        public Type Type { get; private set; }
        /// <summary>MemberInfo</summary>
        public MemberInfo Member { get; private set; }
        /// <summary>member class FullName</summary>
        [SerializeField] public string TypeName;
        /// <summary>member Name</summary>
        [SerializeField] public string MemberName;
        public string AnimationMemberName => TypeMemberUtil.GetMemberField(Member);
        /// <summary>type of the member (bool|int|float|Vector3 etc.)</summary>
        public Type MemberType
        {
            get => Member.MemberType == MemberTypes.Field ? ((FieldInfo)Member).FieldType : ((PropertyInfo)Member).PropertyType;
        }
        public bool MemberTypeIsPrimitive => MemberType.IsPrimitive || MemberType.IsSubclassOf(typeof(Enum));
        /// <summary>INameAndDescription.Name</summary>
        public string Name { get => $"{TypeName}\t{MemberName}"; }
        /// <summary>INameAndDescription.Description</summary>
        public string Description
        {
            get
            {
                if (DescriptionCache != null) return DescriptionCache;
                if (MemberName == "enabled")
                {
                    return DescriptionCache = $"{Type.Name}.enabled";
                }
                var label = TypeMemberUtil.GetMemberLabel(Member);
#if UNITY_EDITOR
                label = UnityEditor.ObjectNames.NicifyVariableName(label);
#endif
                return DescriptionCache = $"{Type.Name}.{label}";
            }
        }
        string DescriptionCache = null;
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
#if NET_UNITY_4_8 || UNITY_2021_2_OR_NEWER
            return HashCode.Combine(Type, Member);
#else
            return (Type.GetHashCode() * 397) ^ Member.GetHashCode();
#endif
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
