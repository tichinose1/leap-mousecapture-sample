# 内容
- Leap Motionを使って指でマウスカーソルを動かす
- Leap Motionの座標を画面座標に変換
- .NETのプラットフォーム呼び出しでカーソル移動、クリック
- ディスプレイに触れた指の位置とカーソルの位置が（大体）一致するように特定PCに最適化
- 他PCで試す場合はSettingsでパラメータ要調整

## LeapMouseCursorConsole
- コンソールアプリケーション
- バックグラウンドでLeapサーバに問い合わせ

## LeapCursorWpf
- 上記のWPF実装
- WPFウィンドウ上のみカーソル画像が変更される

# 環境
- Visual Studio 2013
- .NET Framework 4.5
- C# 5.0
- Leap Motion SDK 2.3.1
- Ractive Extensions 3.0

# TODO
- [ ] 指定位置でのカーソル画像の変更（クリックのガイドのため）
- [ ] Rx無し版の実装

# 課題
## マウスカーソルはいじれない
- WPFで実装してもWPFウインドウ上のみ有効