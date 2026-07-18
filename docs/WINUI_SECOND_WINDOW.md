# WinUI 第2ウィンドウ・フルスクリーン

<!-- pair: .cursor/rules/playbook-winui-second-window-borderless.mdc -->

WinUI 3 で第2モニターに `Window` を出すときの確定パターン（DWM 白枠対策含む）。

## 原因（典型）

`AppWindow.MoveAndResize` / `OverlappedPresenter` が Win32 `WS_POPUP` を上書きし、  
Win11 DWM がモニター端に白い 1px 枠を再描画する。

## フルスクリーン ON（確定）

**WinUI の `MoveAndResize` / `OverlappedPresenter` と Win32 `WS_POPUP` を同時に使わない。**

```
Show:
  1. フルスクリーン時 … タイトルバー折りたたみ等のみ（Presenter 枠消しは避ける）
  2. AppWindow.Show()
  3. Win32 のみ … WS_POPUP + SetWindowPos（モニター全面）
  4. 必要なら ExpandBounds(+2px) で DWM 枠を画面外へ
```

フルスクリーン OFF + 自動配置 … `MoveAndResize` → 最後に Win32 仕上げで可。

## 禁止

- フルスクリーン時の `AppWindow.MoveAndResize`
- Win32 適用後に `MoveAndResize` をループで再実行
- `DwmExtendFrameIntoClientArea` + 不透明背景（白 1px 枠の原因になりうる）
- コンテンツ `ScaleTransform` で OS 枠を隠そうとする（効かない）

## ロゴ / ミラー

- cover は `Border` + `ImageBrush`（`UniformToFill`）が扱いやすい
- ミラー描画のクリア色は **Transparent**（白はレターボックスに見える）

## KM Timer での適用

- `OutputWindow.ShowOnDisplay()` — 第2モニターがあれば上記フルスクリーン、無ければ通常ウィンドウ
- `Helpers/MonitorHelper.cs` / `Helpers/OutputWindowLayout.cs`
