# Fake

ビルドスクリプトに [FAKE](https://fake.build/) を使用している。

## Dev Targets

### Clean
ビルドした出力物を削除する

```sh
./build.fsx -t clean
```

### Build
ビルドする。

```sh
./build.fsx [-- <DEBUG|RELEASE>]
```

### Pack Resource
`Resources`ディレクトリから`Resources.pack`を作成する。

`ResourcesPassword.txt`ファイルからパスワードを読み込む。
実行プロジェクトに埋め込むことで、パスワードの指定を共通化して自動化している。

`.gitignore` に追加して、CIでは`Resources.pack`をクラウドストレージからダウンロードするという方法もある。
[build.fsx](/build.fsx)の`"Resources.CI"`ターゲットを参照。

```sh
./build.fsx -t resources.pack
```

### Format

各自でオートフォーマッターをインストールすることで有効になる。

[build.fsx](/build.fsx)の`"Format"`ターゲットを参照。

```sh
./build.fsx -t format
```

### Update .NET local tools

プロジェクトローカルの.NETツールをすべてアップデートする。

```sh
./build.fsx -t tool.update
```

## Update build.fsx.lock

`build.fsx`に新しいライブラリを追加した際や、バージョンの更新を行う場合に。

```sh
rm build.fsx.lock
rm -r ./.fake
./build.fsx
```

## 配布

Github Actionsに設定済みなので、タグをプッシュするだけでReleaseのDraftが作られてファイルをダウンロード可能になります。

### ライセンスファイルについて

```sh
./build.fsx -t licenses
```

を実行すると、`dotnet-project-licenses`を利用して`publish/licenses`以下に依存ライブラリのライセンスファイルが自動生成される。

## Publish

```sh
# Windows
./build.fsx -t publish.win

# MacOS
./build.fsx -t publish.osx

# まとめて
./build.fsx -t publish
```

## 配布用ファイル作成

これは各OSでしかできないので、CIでの実行を推奨。

```sh
# Windows
./build.fsx -t dist.win

# MacOS
./build.fsx -t dist.osx
```
