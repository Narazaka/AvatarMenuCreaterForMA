using UnityEngine;

namespace net.narazaka.avatarmenucreator
{
    public static class T
    {
        static string L
        {
            get
            {
                var trace = new System.Diagnostics.StackTrace();
                var propertyName = trace.GetFrame(1).GetMethod().Name.Substring(4);
                return Localization.Get(typeof(T).GetProperty(propertyName));
            }
        }

        [English("Batch setting of parameters and material slots with the same name")]
        public static string 同名パラメーターや同マテリアルスロットを一括設定 => L;

        [English("Select the target avatar")]
        public static string 対象のアバターを選択して下さい => L;

        [English("Select the target object")]
        public static string 対象のオブジェクトを選択して下さい => L;

        [English("Hint: Multiple objects can be selected and set together.")]
        public static string ヒント_colon__複数のオブジェクトを選択して一緒に設定出来ます => L;

        [English("Save format")]
        public static string 保存形式 => L;

        [English("Create a menu for each selected object")]
        public static string 選択オブジェクト一つごとにメニューを作成 => L;

        [English("Name")]
        public static string 名前 => L;

        [English("Save as")]
        public static string 保存場所 => L;

        [English("If MA Merge Animator or MA Parameters are present, this component will not affect them and their settings will be used as is.")]
        public static string MA_Merge_AnimatorまたはMA_Parametersがある場合ヽ_このコンポーネントは影響せずそれらの設定がそのまま使われますゝ => L;

        [English("Use Component Settings instead")]
        public static string コンポーネントの設定を優先する => L;

        [English("Are you sure you want to delete it?")]
        public static string 本当に削除しますか_Q_ => L;

        [English("Remove MA Merge Animator and MA Parameters.\nReset the menu installed by MA Menu Installer.")]
        public static string MA_Merge_AnimatorとMA_Parametersを削除しますゝ_n_MA_Menu_Installerのインストールされるメニューをリセットしますゝ => L;

        [English("MA Menu Installer prefab developer settings/menus to be installed are set but ignored.")]
        public static string MA_Menu_Installerのプレハブ開発者向け設定_sl_インストールされるメニューが設定されていますがヽ無視されますゝ => L;

        [English("It is installed at the MA Menu Installer installation location.\nIf there is no MA Menu Installer, it behaves like MA Menu Item. (Useful for nested menus, etc.)")]
        public static string MA_Menu_Installerのインストール先にインストールされますゝ_n_MA_Menu_Installer_が無い場合は_MA_Menu_Item_のように振る舞いますゝ__start_ネストしたメニューなどに便利_end_ => L;

        [English("It is located under the submenu of MA Menu Item.\nIt can be installed without the MA Menu Installer.")]
        public static string MA_Menu_Itemのサブメニュー配下にありますゝ_n_MA_Menu_Installerがなくてもインストールできますゝ => L;

        [English("It is located under the MA Menu Group.\nIt can be installed without the MA Menu Installer.")]
        public static string MA_Menu_Groupの配下にありますゝ_n_MA_Menu_Installerがなくてもインストールできますゝ => L;

        [English("Remove MA Menu Installer")]
        public static string MA_Menu_Installerを削除 => L;

        [English("Remove MA Menu Installer")]
        public static string MA_Menu_Installerを削除します => L;

        [English("It is not under the submenu of MA Menu Item.\nMA Menu Installer is required for menu generation.\n(It will work without menu generation if you change the parameters by other means.)")]
        public static string MA_Menu_Itemのサブメニュー配下にありませんゝ_n_メニューの生成にはMA_Menu_Installerが必要ですゝ_n__start_メニュー生成無しでも他の手段でパラメーターを変更すれば動作します_end_ => L;

        [English("Add MA Menu Installer")]
        public static string MA_Menu_Installerを追加 => L;

        [English("Asset generation/restoration (optional)")]
        [Japanese("アセット生成/復元（オプショナル）")]
        public static string アセット生成_復元_オプショナル => L;

        [English("Asset generation (optional)")]
        [Japanese("アセット生成（オプショナル）")]
        public static string アセット生成_オプショナル => L;

        [English("The Restore from Asset function does not support advanced settings.\nIt is not possible to restore settings from generated assets.")]
        public static string アセットからの復元機能は高度な設定に対応していません_n_生成後のアセットから設定を復元することは出来ません => L;

        [English("Regenerate assets with this setting")]
        public static string この設定でアセットを再生成 => L;

        [English("Generate assets with this configuration")]
        public static string この設定でアセットを生成 => L;

        [English("Assets can be regenerated with different settings.")]
        public static string 設定を変えてアセットを再生成出来ます => L;

        [English("Assets can be generated for manual editing")]
        public static string 手動編集用にアセットを生成できます => L;

        [English("Restore settings from asset contents")]
        public static string アセット内容から設定を復元 => L;

        [English("If the assets were edited manually, the restoration may not be accurate.")]
        public static string アセットを手動で編集していた場合などは正確な復元にならない可能性があります => L;

        [English("This object can be prefabbed to generate assets for manual editing")]
        public static string このオブジェクトをprefabにすると手動編集用にアセットを生成できます => L;

        [English("This component needs to be placed inside your avatar to work properly.")]
        public static string このコンポーネントが正しく動作するにはヽアバター内に配置する必要がありますゝ => L;

        [English("Add object")]
        public static string オブジェクトを追加 => L;

        [English("Are you sure you want to change it?")]
        public static string 本当に変更しますか_Q_ => L;

        [English("A non-existent object name is specified for the avatar")]
        public static string アバターに存在しないオブジェクト名が指定されています => L;

        [English("Restore failed.")]
        public static string 復元に失敗しました => L;

        [English("I can't find a suitable restoration method.")]
        public static string 適切な復元法が見つかりません => L;

        [English("Success")]
        public static string 成功 => L;

        [English("Warning")]
        public static string 警告 => L;

        [English("Successfully restored.")]
        public static string 復元に成功しました => L;

        [English("It will replace the current component with another component. Do you wish to continue?")]
        public static string 今のコンポーネントと別のコンポーネントに置き換わりますゝ_n_続行しますか_Q_ => L;

        [English("Unexpected error. Please contact the tool author @narazaka with a screenshot")]
        [Japanese("想定外エラーです\nツール作者 @narazaka にスクリーンショットを添えて連絡して下さい")]
        public static string 想定外エラーです_ツール作者_narazaka_にスクリーンショットを添えて連絡して下さい => L;

        [English("Gradual change (in seconds)")]
        public static string 徐々に変化_start_秒数_end_ => L;

        [English("There is no specification for gradual change")]
        public static string 徐々に変化するものの指定が有りません => L;

        [English("Changes over a specified time.")]
        public static string 指定時間かけて変化します => L;

        [English("Options")]
        public static string オプション => L;

        [English("Saved Parameter")]
        public static string パラメーター保存 => L;

        [English("Parameter name (optional)")]
        public static string パラメーター名_start_オプショナル_end_ => L;

        [English("Parameter Auto Rename")]
        public static string パラメーター自動リネーム => L;

        [English("Internal Parameter")]
        public static string パラメーター内部値 => L;

        [English("Icon")]
        public static string アイコン => L;

        [English("Parameter Default")]
        public static string パラメーター初期値 => L;

        [English("Advanced")]
        public static string 高度な設定 => L;

        [English("When advanced settings are enabled, you can set the control to ON or OFF one side only.")]
        public static string 高度な設定を有効にするとヽONまたはOFF片方だけ制御する設定ができますゝ => L;

        [English("batch setting")]
        public static string 一括設定 => L;

        [English("No control")]
        public static string 制御しない => L;

        [English("ON=show")]
        public static string ON_eq_表示 => L;

        [English("ON=hide")]
        public static string ON_eq_非表示 => L;

        [English("ON=")]
        public static string ON_eq_ => L;

        [English("ctrl both")]
        public static string 両方制御 => L;

        [English("ON only")]
        public static string ONのみ制御 => L;

        [English("OFF only")]
        public static string OFFのみ制御 => L;

        [English("Wait%")]
        public static string 変化待機_per_ => L;

        [English("Control ON")]
        public static string ONを制御 => L;

        [English("Control OFF")]
        public static string OFFを制御 => L;

        [English("Duration%")]
        public static string 変化時間_per_ => L;

        [English("Multiple materials are selected for OFF")]
        public static string OFFに複数のマテリアルが選択されています => L;

        [English("Multiple materials are selected for ON")]
        public static string ONに複数のマテリアルが選択されています => L;

        [English("Multiple change waiting times are set")]
        public static string 複数の変化待機_per_が設定されています => L;

        [English("Multiple settings are made to control ON")]
        public static string ONを制御に複数の設定がされています => L;

        [English("Multiple settings are made to control OFF")]
        public static string OFFを制御に複数の設定がされています => L;

        [English("Change duration % should be set greater than 0")]
        public static string 変化時間_per_は0より大きく設定して下さい => L;

        [English("Delete advanced settings")]
        public static string 高度な設定の削除 => L;

        [English("Advanced settings will be reset.\nDo you really want to disable advanced settings?")]
        public static string 高度な設定がリセットされます_n_本当に高度な設定を無効にしますか_Q_ => L;

        [English("Option")]
        public static string 選択肢 => L;

        [English("Parent Menu Icon")]
        public static string 親メニューアイコン => L;

        [English("Number of options")]
        public static string 選択肢の数 => L;

        [English("Control this")]
        public static string 制御 => L;

        [English("Multiple materials are selected")]
        public static string 複数のマテリアルが選択されています => L;

        [English("Set Invalid Area")]
        public static string 無効領域を設定 => L;

        [English("Sets parameter regions that are not affected by the animation")]
        public static string アニメーションが影響しないパラメーター領域を設定します => L;

        [English("greater than")]
        public static string これより大きい場合 => L;

        [English("smaller than")]
        public static string これより小さい場合 => L;

        [English("Init")]
        public static string 初期 => L;

        [English("Start")]
        public static string 始 => L;

        [English("End")]
        public static string 終 => L;

        [English("Start offset%")]
        public static string 始offset_per_ => L;

        [English("End offset%")]
        public static string 終offset_per_ => L;

        [English("There is nothing to control.")]
        public static string BlendShape_sl_Shader_Parameterなし => L;

        [English("PhysBone auto-reset")]
        public static string PhysBone自動リセット => L;

        [English("Values are set that are not reflected unless PhysBone is reset; if PhysBone auto-reset is enabled, the reset (OFF/ON) process is inserted.")]
        public static string PhysBoneをリセットしないと反映されない値が設定されていますゝPhysBone自動リセットを有効にするとリセット_start_OFF_sl_ON_end_処理を挿入しますゝ => L;

        [English("To enable PhysBone auto-reset, delete the PhysBone.enabled setting.")]
        public static string PhysBone自動リセットを有効にするには_PhysBone_dot_enabled_設定を削除して下さいゝ => L;
    }
}
