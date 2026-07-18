# KM Timer

IC_Timer（Electron）相当の **WinUI 3** タイマー／カンペアプリ。

## 機能

- 制御パネル + 出力ウィンドウ（第2モニターがあればフルスクリーン）
- カウントダウン／0秒後カウントアップ、警告色・終了色、点滅
- カンペ 4 枠（フォーカス時プレビュー、出力ボタン、背景点滅）
- 現在時刻表示、背景色、サイズ調整
- プリセット 5 スロット（`%LocalAppData%\KmTimer`）
- オンライン更新（GitHub Release の Setup）

## 開発

```powershell
Stop-Process -Name KmTimer -Force -ErrorAction SilentlyContinue
dotnet build "KmTimer\KmTimer.csproj" -p:Platform=x64
dotnet run --project "KmTimer\KmTimer.csproj" -p:Platform=x64
```

## リリース

```powershell
.\scripts\Build-Release.ps1          # Setup + ZIP + GitHub Release
.\scripts\Build-Release.ps1 -DryRun  # 成果物のみ
```

詳細は [BUILD_RULES.md](BUILD_RULES.md)。
