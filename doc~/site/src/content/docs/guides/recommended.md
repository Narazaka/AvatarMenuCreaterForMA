---
title: おすすめ服着せワークフロー
sidebar:
  order: 31
---

とりあえず服prefabをアバターの中に突っ込んで、Modular Avatar→Setup Outfitとかをやって服を着せます。

![](../../../assets/imgs/wf1.png)

Avatar Menu Creatorを開いてアバターを設定した状態で、服の全メッシュを選択します。

![](../../../assets/imgs/wf2.png)

- 「同名パラメーターや同マテリアルスロットを一括設定」をON
- パラメーター初期値をON

にします。

![](../../../assets/imgs/wf3.png)

どれでもいいので「ON=表示」をクリックすると、全ての項目が「ON=表示」になります。

![](../../../assets/imgs/wf4.png)

不要なMA Menu Installerは付けない設定にします。（後で親メニューを作るので）

![](../../../assets/imgs/wf4_1.png)

全部のパーツが別々のメニューで良いなら「選択オブジェクト一つごとにメニューを作成」にチェックを付けて「Create!」

![](../../../assets/imgs/wf5.png)

まとめてON/OFFしたいパーツがあるならそこだけ選び直して名前を付けて「Create!」していきます。

![](../../../assets/imgs/wf6.png)

選択が変わってもさっき設定した値はウインドウに保持されているので、途中で選択を変えても大丈夫です。

![](../../../assets/imgs/wf7.png)

メニューを作り終わったら

![](../../../assets/imgs/wf8.png)

メニューの親オブジェクトを作ってその中にメニューを突っ込んで

![](../../../assets/imgs/wf9.png)

サブメニューのためのMA Menu Itemを設定して（参考: [メニューをサブメニューにまとめたい](/guides/submenu)）

![](../../../assets/imgs/wf10.png)

そこにMA Menu Installerを付ければ

![](../../../assets/imgs/wf11.png)

完成です！

![](../../../assets/imgs/wf12.png)![](../../../assets/imgs/wf13.png)