using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using VRC.SDKBase.Validation;

namespace net.narazaka.avatarmenucreator.util
{
    public static class ComponentWhitelistUtil
    {
        static ImmutableHashSet<Type> Cache;

        public static IEnumerable<Type> FilterByVRCWhitelist(this IEnumerable<Type> types)
        {
            return types.Where(ComponentWhitelist.Contains);
        }

        static ImmutableHashSet<Type> ComponentWhitelist
        {
            get
            {
                if (Cache != null) return Cache;
                return Cache = AvatarValidation.ComponentTypeWhiteListCommon.Concat(AvatarValidation.ComponentTypeWhiteListSdk3).Select(TypeUtil.GetType).ToImmutableHashSet();
            }
        }
    }
}
