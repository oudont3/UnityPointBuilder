# UnityPointBuilder
Mesh上に(なるべく)均等に点を並べるUnityEditorツールです。

## Description
- UV座標上に等間隔に点を配置し、Mesh上の座標に変換して実現しています。
- SkinnedMeshRendererはまだ未対応
- unity 2018.3.3f1
- 手元のメッシュでしか試していないのでうまくいかないかもしれません

## ScreenShot

## Usage

1. 「PointBuilder」→「Open Editor」
1. メニュー各種設定/操作
    - Point Settings
        - Mesh
        - UV Type: UV2(Light Map用のUV)のほうが比較的きれいに並びます
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
        - プレビューカメラの移動・回転：スクロールボタンもしくは右クリック + ドラッグ操作で一応行えます
1. 生成したデータ(PointData)にpointとnormalが入っているので良しなに(sample参考)

## Reference Sites
- 座標計算
    - http://esprog.hatenablog.com/entry/2016/10/10/002656
    - http://sampo.hatenadiary.jp/entry/20070626/p1
    - http://www.iot-kyoto.com/satoh/2016/01/29/tangent-003/
- shader(sample)
    - https://gist.github.com/mattatz/40a91588d5fb38240403f198a938a593

## License
[MIT](LICENSE.txt)
