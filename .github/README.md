<!--
[![](https://github.com/wraikny/Altseed2Template/workflows/CI/badge.svg)](https://github.com/wraikny/Altseed2Template/actions?workflow=CI)
-->

# Altseed2Template

Altseed2 向けプロジェクトテンプレート。  

[ドキュメント](docs/template/)

## 機能

- ビルドスクリプト設定済み
  - 依存パッケージのライセンスファイル自動生成
  - `Resources.pack`生成

- GitHub Actions設定済み
  - 通常のプッシュとPRでは、フォーマットチェックとビルド
  - タグをプッシュすると、WindowsとMacOSそれぞれで配布用ファイル生成して、リリースページのドラフトからダウンロード可能
    - 生成されるファイル
      - Windows: exeを含むzip
      - MacOS: appを含むdmg
    - `Resources.pack`をクラウドストレージ等からダウンロードする設定も可

- [dist/contents](/dist/contents/)ディレクトリ以下のファイルを自動的に配布先に同梱
  - 注意: 日本語ファイル名を使うと、zipする際に文字化けします

- フォーマット用のビルドスクリプト・CIは記述済みなので、対応するツールをインストールしてビルドスクリプトのコメントアウトを外せば利用可能

## .NET ツール （開発環境）

- FAKE
- dotnet-project-licenses
- altseed2.tools

[dotnet-tools.json](/.config/dotnet-tools.json)に記載されている。

最初に次のコマンドを実行する必要がある。

```sh
dotnet tool restore
```

## ビルドコマンド

- [FAKE](./fake.md)
