# Fake

ビルドスクリプトに [FAKE](https://fake.build/) を使用している。

## Dev Targets
### Format

```sh
dotnet fake build -t format
```

### Clean

```sh
dotnet fake build -t clean
```

### Build

```sh
dotnet fake build [-- <DEBUG|RELEASE>]
```

### Pack Resource

`ResourcesPassword.txt`ファイルからパスワードを読み込む。
実行プロジェクトに埋め込むことで、パスワードの指定を共通化して自動化している。

実際は `.gitignore` に追加して、CIでは`Resources.pack`をクラウドストレージからダウンロードするという方法もある。

```sh
dotnet fake build -t resources.pack
```

### Update .NET local tools

```sh
dotnet fake build -t tool.update
```

## Update build.fsx.lock

`build.fsx`上部で新しいライブラリを追加した際などに。

```sh
rm build.fsx.lock
rm -r ./.fake
dotnet fake build
```

## 配布

### ライセンスファイルについて

```sh
dotnet fake build -t licenses
```

を実行すると、`dotnet-project-licenses`を利用して`publish/licenses`以下に依存ライブラリのライセンスファイルが自動生成される。

## Publish

```sh
# Windows
dotnet fake build -t publish.win

# MacOS
dotnet fake build -t publish.osx

# まとめて
dotnet fake build -t publish
```

## 配布用ファイル作成

これは各OSでしかできないので、CIでの実行を推奨。
なお、あらかじめライセンスを生成しておく。

```sh
# Windows
dotnet fake build -t dist.win

# MacOS
dotnet fake build -t dist.osx
```
