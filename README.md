# AvatarMenuCreaterForMA

AvatarMenuCreater for Modular Avatar

## 概要

Modular Avatarでアバターのメニューを構成出来るようにする補助アセットです。

アバターのメニュー1項目をModular Avatarの1プレハブとして作成します。

- オブジェクト・BlendShape・シェーダーパラメーターのトグル制御（Toggle）
- BlendShape・シェーダーパラメーターの無段階調整（Radial Puppet）

に対応しています。

## インストール

### VCCによる方法

1. https://vpm.narazaka.net/ から「Add to VCC」ボタンを押してリポジトリをVCCにインストールします。
2. VCCでSettings→Packages→Installed Repositoriesの一覧中で「Narazaka VPM Listing」にチェックが付いていることを確認します。
3. アバタープロジェクトの「Manage Project」から「AvatarMenuCreaterForMA」をインストールします。

### unitypackageによる方法

1. Modular Avatar https://modular-avatar.nadena.dev/ をインストールします。
2. [Releaseページ](https://github.com/Narazaka/AvatarMenuCreaterForMA/releases/latest) から net.narazaka.vrchat.avatar-menu-creater-for-ma-\*.\*.\***-novcc**.zip をダウンロード＆解凍し、unitypackageをアバタープロジェクトにインストールします。

## 使い方

1. 「Tools」→「Modular Avatar」→「AvatarMenuCreater for Modular Avatar」をクリックし、ツールを立ち上げます。

2. アバターをツールに設定し、アバター以下の制御したいオブジェクトを選択した状態でツールで処理を設定します。（オブジェクトを選択してもツールのウインドウにカーソルを合わせないと表示が変わらないかも）

3. 「Create!」ボタンを押して保存場所を選択すれば、Modular AvatarのPrefabが出来上がります。

4. Prefabをアバターの中（直下でなくても良い）に置けば、Modular Avatarによってメニューが統合されます。

## 更新履歴

- 1.1.3
  - VCCでアバター用に表示されるように
- 1.1.2
  - VPM化
- 1.1.1
  - 1メッシュにマテリアルが2つ以上ある場合にエラーになる問題を修正
- 1.1.0
  - シェーダーパラメーターを設定する機能追加
- 1.0.0
  - リリース

## License

[Zlib License](LICENSE.txt)
