using nadena.dev.ndmf;
using net.narazaka.avatarmenucreator.components;

namespace net.narazaka.avatarmenucreator.editor
{
    [ParameterProviderFor(typeof(AvatarChooseMenuCreator))]
    internal class AvatarChooseMenuCreatorParameterProvider : AvatarMenuCreatorBaseParameterProvider
    {
        public AvatarChooseMenuCreatorParameterProvider(AvatarChooseMenuCreator c) : base(c) { }
    }
}
