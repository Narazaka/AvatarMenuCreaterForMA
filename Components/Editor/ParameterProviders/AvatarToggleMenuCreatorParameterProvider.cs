using nadena.dev.ndmf;
using net.narazaka.avatarmenucreator.components;

namespace net.narazaka.avatarmenucreator.editor
{
    [ParameterProviderFor(typeof(AvatarToggleMenuCreator))]
    internal class AvatarToggleMenuCreatorParameterProvider : AvatarMenuCreatorBaseParameterProvider
    {
        public AvatarToggleMenuCreatorParameterProvider(AvatarToggleMenuCreator c) : base(c) { }
    }
}
