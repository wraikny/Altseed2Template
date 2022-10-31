<!--
[![](https://github.com/wraikny/Altseed2Template/workflows/CI/badge.svg)](https://github.com/wraikny/Altseed2Template/actions?workflow=CI)
-->

# Altseed2Template

Altseed2 向けプロジェクトテンプレート。  

[ドキュメント](docs/template/)

## 機能

- ビルドスクリプト設定済み
  - ライセンスファイル自動生成
  - `Resources.pack`生成
  - CIでは`Resources.pack`をクラウドストレージ等からダウンロードする設定も可

- GitHub Actions設定済み
  - タグをプッシュすると、WindowsとMacOSそれぞれで配布用ファイル生成して、リリースページのドラフトからダウンロード可能
    - Windows: exeが含むzip
    - MacOS: appを含むdmg

- [dist/contents](/dist/contents/)ディレクトリ以下のファイルを自動的に配布先に同梱
  - 日本語ファイル名使えないですごめんなさい
