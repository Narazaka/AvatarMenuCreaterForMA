---
title: メニューをサブメニューにまとめたい
sidebar:
  order: 21
---

Modular Avatarの機能を使ってメニューをサブメニューにまとめることが出来ます。

### 空オブジェクトを作る

アバターの下に親メニューとなる空オブジェクトを作ります。
（アバターのオブジェクトを右クリックして「Create Empty」）

![](../../../assets/imgs/menu-parent.png)

# Modular Avatarのコンポーネントを追加する

親メニューオブジェクトのInspector上の「Add Component」から`MA Menu Installer`と`MA Menu Item`を付けて、以下のように設定します。

![](../../../assets/imgs/menu-parent-inspector.png)

# メニューを移動する

親メニューオブジェクトの子階層にAvatarMenuCreatorで作ったメニューをもってきます

![](../../../assets/imgs/menu-child.png)

# 不要なコンポーネントを削除

AvatarMenuCreatorでメニューオブジェクトに付いている`MA Menu Installer`を削除します。

（削除して良いというヘルプと削除ボタンが出ているので押して下さい）

![](../../../assets/imgs/menu-child-inspector.png)

# 完成

これでこの階層通りのメニューが出来ます。

![](../../../assets/imgs/menu-child.png)
