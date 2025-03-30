using nadena.dev.ndmf;
using net.narazaka.avatarmenucreator.components;
using net.narazaka.avatarmenucreator.components.editor;
using net.narazaka.avatarmenucreator.util;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace net.narazaka.avatarmenucreator.editor
{
    [ParameterProviderFor(typeof(AvatarMenuCreatorBase))]
    internal class AvatarMenuCreatorBaseParameterProvider : IParameterProvider
    {
        readonly AvatarMenuCreatorBase Menu;

        public AvatarMenuCreatorBaseParameterProvider(AvatarMenuCreatorBase c)
        {
            Menu = c;
        }

        public IEnumerable<ProvidedParameter> GetSuppliedParameters(BuildContext context = null)
        {
            return Menu.GetParameterNameAndTypes().Select(p =>
                new ProvidedParameter(p.name, ParameterNamespace.Animator, Menu, AvatarMenuCreatorPlugin.Instance, p.valueType.ToAnimatorControllerParameterType())
                {
                    WantSynced = p.networkSynced,
                    IsHidden = Menu.AvatarMenu.InternalParameter,
#if HAS_NDMF_ParameterProvider_DefaultValue
                    DefaultValue = p.defaultValue,
#endif
                }
                );
        }

        public void RemapParameters(ref ImmutableDictionary<(ParameterNamespace, string), ParameterMapping> nameMap,
            BuildContext context)
        {
            // no-op
        }
    }
}
