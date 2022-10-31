<!--
[![](https://github.com/wraikny/Altseed2Template/workflows/CI/badge.svg)](https://github.com/wraikny/Altseed2Template/actions?workflow=CI)
-->

# Altseed2Template

Altseed2 向けプロジェクトテンプレート。

## 機能

[FAKE](https://fake.build/)を利用してビルドスクリプトを記述している。
詳細は[fake.md](/docs//template/fake.md)を参照。

### Github Actions

プッシュ・PR時にフォーマットチェックとビルドを行う。

フォーマッターの有効化は[build.fsx#L304](/build.fsx#L304)を参照。

### Github Actionsでの配布用zip・dmgファイル自動生成
タグをプッシュすると、
WindowsとMacOSそれぞれで配布用ファイル生成して、
リリースページのDraftからダウンロード可能。

生成されるファイル
- Windows: `Project.exe`を含む`Project.win-x64.zip`
- MacOS: `Project.app`を含む`Project.osx-x64.dmg`

`Resources.pack`をCIで生成せず、
クラウドストレージ等からダウンロードしたい場合は
[build.fsx](/build.fsx#L367)を参照。

[dist/contents](/dist/contents/)ディレクトリ以下のファイルを自動的に同梱
（日本語ファイル名はzip化の際に文字化けします）


## .NET ツール （開発環境）

- FAKE
- dotnet-project-licenses
- altseed2.tools

[dotnet-tools.json](/.config/dotnet-tools.json)に記載されている。

最初に次のコマンドを実行する必要がある。

```sh
dotnet tool restore
```

