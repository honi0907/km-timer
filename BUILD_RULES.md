# KM Timer — Build Rules

共通運用は [cursor-playbook `docs/COMMON_APP_RULES.md`](https://github.com/honi0907/cursor-playbook/blob/master/docs/COMMON_APP_RULES.md) を参照。  
ここは KM Timer 固有の成果物パスとコマンド。

## ビルド前

```powershell
Stop-Process -Name KmTimer -Force -ErrorAction SilentlyContinue
```

起動中は exe / DLL がロックされビルド・publish が失敗するため、**必ず先に終了**する。

## 開発ビルド

```powershell
dotnet build "KmTimer\KmTimer.csproj" -p:Platform=x64
```

実行（Unpackaged）:

```powershell
dotnet run --project "KmTimer\KmTimer.csproj" -p:Platform=x64
```

## リリース（Setup + ポータブル ZIP）

```powershell
.\scripts\Build-Release.ps1
```

内部で `Build-Portable.ps1` → `installer/build-installer.ps1` を実行する。

ビルドのみ（GitHub Release なし）:

```powershell
.\scripts\Build-Release.ps1 -DryRun
```

### 前提

- **Inno Setup 6**（`ISCC.exe`）。未インストール時は `installer/build-installer.ps1` が winget 導入を試みる
- 手動: `winget install --id JRSoftware.InnoSetup -e`

| 成果物 | 出力先 |
|--------|--------|
| Setup（GitHub / オンライン更新） | `dist\KmTimer-{version}-x64-Setup.exe` |
| ポータブル ZIP | `dist\KmTimer-{version}-x64-portable.zip` |
| publish フォルダ | `dist\publish\win-x64\` |

- **毎回** Setup とポータブル ZIP の両方を生成する。
- `dist` には当該バージョンの成果物のみ残す（旧 `*-Setup.exe` / `*-portable.zip` は削除）。

### GitHub Release に載せるもの

| 層 | 載せるもの |
|----|-----------|
| **GitHub Release** | Setup.exe + portable.zip |
| **オンライン更新** | Setup.exe のみをダウンロード → インストーラー起動 |

- リポジトリ: `honi0907/km-timer`
- tag: `v{version}`（例: `v1.0.0`）
- 開発ビルド（`bin/`）では自己更新の適用をブロック

## バージョン

- `KmTimer.csproj` の `<Version>` / `<AssemblyVersion>` / `<FileVersion>` を揃える。
- パッチは **0〜9**（例: `1.0.0` … `1.0.9`）。**`1.0.9` の次は `1.1.0`**（`1.0.10` にはしない）。
- リリースのたびに必ず上げ、同じ tag / ファイル名の再利用はしない。

## オンライン更新（アプリ内）

システム設定 → **オンライン更新を確認**  
→ GitHub Release の最新 Setup を取得 → インストーラー起動 → アプリ終了。

トークン（プライベート repo 用）: 環境変数 `KMTIMER_GITHUB_TOKEN`

## 第2ウィンドウ

出力ウィンドウのフルスクリーンは [docs/WINUI_SECOND_WINDOW.md](docs/WINUI_SECOND_WINDOW.md) に従う。
