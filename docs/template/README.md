# Altseed2Template

Altseed2 向けプロジェクトテンプレート。  
CIで配布用にWindows/MacOSでビルドする。

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

## CI

GitHub Actionsが設定済み。

Windows/MacOSでビルドが走る。

タグの場合は、配布用のファイルの生成を行う。

Windowsでは、exeが格納されたzipファイル

MacOSでは、appが格納されたdmgファイル

アーティファクトとしてダウンロードできる。
