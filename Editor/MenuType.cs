using UnityEngine;

namespace net.narazaka.avatarmenucreator.editor
{
    enum MenuType
    {
        [Japanese("ON／OFF")]
        [English("Toggle")]
        Toggle,
        [Japanese("選択式")]
        [English("Select")]
        Choose,
        [Japanese("無段階制御")]
        [English("Range")]
        Slider,
    }
}
