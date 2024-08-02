using System;
using System.Collections.Generic;
using System.Reflection;
using VRC.SDK3.Dynamics.PhysBone.Components;

namespace net.narazaka.avatarmenucreator.util
{
    public static class TypeMemberUtil
    {
        static Dictionary<Type, MemberInfo[]> AvailableMembers = new Dictionary<Type, MemberInfo[]>
        {
            {
                typeof(VRCPhysBone), new MemberInfo[]
                {
                    typeof(VRCPhysBone).GetField(nameof(VRCPhysBone.pull)),
                    typeof(VRCPhysBone).GetField(nameof(VRCPhysBone.spring)),
                    typeof(VRCPhysBone).GetField(nameof(VRCPhysBone.stiffness)),
                    typeof(VRCPhysBone).GetField(nameof(VRCPhysBone.gravity)),
                    typeof(VRCPhysBone).GetField(nameof(VRCPhysBone.gravityFalloff)),
                }
            },
        };

        static Dictionary<Type, TypeMember[]> AvailableMembersCache = new Dictionary<Type, TypeMember[]>();

        public static IEnumerable<TypeMember> GetAvailableMembers(this Type type)
        {
            if (AvailableMembersCache.TryGetValue(type, out var availableMembers))
            {
                return availableMembers;
            }
            var hasEnabled = TypeUtil.GetHasEnabled(type);

            if (AvailableMembers.TryGetValue(type, out var members))
            {
                var array = new TypeMember[members.Length + (hasEnabled ? 1 : 0)];
                var offset = 0;
                if (hasEnabled)
                {
                    array[0] = new TypeMember(type, "enabled");
                    offset = 1;
                }
                for (var i = 0; i < members.Length; ++i)
                {
                    array[i + offset] = new TypeMember(members[i]);
                }
                return AvailableMembersCache[type] = array;
            }

            if (hasEnabled)
            {
                return AvailableMembersCache[type] = new TypeMember[] { new TypeMember(type, "enabled") };
            }

            return AvailableMembersCache[type] = new TypeMember[0];
        }

        static Dictionary<(Type, string), MemberInfo> MemberInfoCache = new Dictionary<(Type, string), MemberInfo>();

        public static MemberInfo GetMember(Type type, string memberName)
        {
            if (MemberInfoCache.TryGetValue((type, memberName), out var memberInfo))
            {
                return memberInfo;
            }
            var field = type.GetField(memberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null) return MemberInfoCache[(type, memberName)] = field;
            var property = type.GetProperty(memberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (property != null) return MemberInfoCache[(type, memberName)] = property;
            return null;
        }
    }
}
