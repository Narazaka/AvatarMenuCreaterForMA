using UnityEngine;

namespace net.narazaka.avatarmenucreator.editor
{
    enum MenuType
    {
        [Japanese("ON／OFF")]
        [English("Toggle")]
        [Icon("Icons/AvatarMenuCreatorToggleIcon.png")]
        Toggle,
        [Japanese("選択式")]
        [English("Select")]
        [Icon("Icons/AvatarMenuCreatorChooseIcon.png")]
        Choose,
        [Japanese("無段階制御")]
        [English("Range")]
        [Icon("Icons/AvatarMenuCreatorRadialIcon.png")]
        Slider,
    }
}
