# Fake

ビルドスクリプトに [FAKE](https://fake.build/) を使用している。

## Dev Targets

### Clean
ビルドした出力物を削除する

```sh
dotnet fake build -t clean
```

### Build
ビルドする。

```sh
dotnet fake build [-- <DEBUG|RELEASE>]
```

### Pack Resource
`Resources`ディレクトリから`Resources.pack`を作成する。

`ResourcesPassword.txt`ファイルからパスワードを読み込む。
実行プロジェクトに埋め込むことで、パスワードの指定を共通化して自動化している。

`.gitignore` に追加して、CIでは`Resources.pack`をクラウドストレージからダウンロードするという方法もある。
[build.fsx](/build.fsx)の`"Resources.CI"`ターゲットを参照。

```sh
dotnet fake build -t resources.pack
```

### Format

各自でオートフォーマッターをインストールすることで有効になる。

[build.fsx](/build.fsx)の`"Format"`ターゲットを参照。

```sh
dotnet fake build -t format
```

### Update .NET local tools

プロジェクトローカルの.NETツールをすべてアップデートする。

```sh
dotnet fake build -t tool.update
```

## Update build.fsx.lock

`build.fsx`に新しいライブラリを追加した際や、バージョンの更新を行う場合に。

```sh
rm build.fsx.lock
rm -r ./.fake
dotnet fake build
```

## 配布

Github Actionsに設定済みなので、タグをプッシュするだけでReleaseのDraftが作られてファイルをダウンロード可能になります。

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

```sh
# Windows
dotnet fake build -t dist.win

# MacOS
dotnet fake build -t dist.osx
```
