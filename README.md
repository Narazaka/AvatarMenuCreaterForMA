# AvatarMenuCreaterForMA

AvatarMenuCreater for Modular Avatar

## 概要

Modular Avatarでアバターのメニューを構成出来るようにする補助アセットです。

アバターのメニュー1項目をModular Avatarの1プレハブとして作成します。

- オブジェクト・BlendShape・シェーダーパラメーターのトグル制御（Toggle）
- BlendShape・シェーダーパラメーターの無段階調整（Radial Puppet）

に対応しています。

Modular Avatar ( https://modular-avatar.nadena.dev/ja/ ) を導入したあとでこれを導入して下さい。

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
