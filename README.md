# UnityPointBuilder
Mesh上に(なるべく)均等に点を並べるUnityEditorツールです。

## Description
- UV座標上に等間隔に点を配置し、Mesh上の座標に変換して実現しています。(三角形外の点は無視)
- スキンメッシュはまだ未対応
- unity 2018.3.3f1
- 手元のメッシュでしか試していないのでうまくいかないかもしれません

## ScreenShot
<img src="https://user-images.githubusercontent.com/15842130/53150251-79e6ba80-35f3-11e9-9832-9f7d44681682.png" width="500" height="341">

<img src="https://user-images.githubusercontent.com/15842130/53150212-60de0980-35f3-11e9-8e4b-31e06f85445b.gif">

## Usage

1. 「PointBuilder」→「Open Editor」
1. メニュー各種設定/操作
    - Point Settings
        - Mesh
        - UV Type: UV2(Light Map用のUV)のほうが比較的きれいに並ぶと思います。
            - meshのinspectorの「Generate Lightmap UV」で生成可能
        - UV Division: 分割数
        - UV Offset: 開始位置(左下から)のoffset
    - Preview
        - Wire Size/Color/Point Color: Previewの各種設定値
        - 「Preview Camera Reset」ボタン: 上下Previewカメラのリセット
        - 「Build Preview」ボタン: Mesh上(下画面)にプレビュー
    - Export
        - ExportDir/Export Name Prefix: 出力先・ファイル名prefix
        - 「Export Point Mesh」ボタン： ポイントデータを生成し出力
    - その他
        - プレビュー上下: 上部がUV座標上の点表示/下部がmesh上での点表示 (真ん中の線ドラッグでリサイズ可)
        - プレビューカメラの移動・回転：スクロールボタン or 右クリック + ドラッグ操作で一応行えます
1. 生成したデータ(PointData)にpointとnormalが入っているので良しなに(sample参考)

## Reference Sites
- 座標計算
    - http://esprog.hatenablog.com/entry/2016/10/10/002656
    - http://sampo.hatenadiary.jp/entry/20070626/p1
    - http://www.iot-kyoto.com/satoh/2016/01/29/tangent-003/
- Editor
    - https://anchan828.github.io/editor-manual/web/index.html
    
## License
[MIT](LICENSE.txt)
