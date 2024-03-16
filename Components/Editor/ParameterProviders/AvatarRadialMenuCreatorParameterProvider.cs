using nadena.dev.ndmf;
using net.narazaka.avatarmenucreator.components;

namespace net.narazaka.avatarmenucreator.editor
{
    [ParameterProviderFor(typeof(AvatarRadialMenuCreator))]
    internal class AvatarRadialMenuCreatorParameterProvider : AvatarMenuCreatorBaseParameterProvider
    {
        public AvatarRadialMenuCreatorParameterProvider(AvatarRadialMenuCreator c) : base(c) { }
    }
}
