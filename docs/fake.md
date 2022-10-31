# Fake

## Restore Fake

```sh
dotnet tool restore
```

## Format

```sh
dotnet fake build -t format
```

## Clean

```sh
dotnet fake build -t clean
```

## Build

```sh
dotnet fake build [-- <DEBUG|RELEASE>]
```

## Publish

```sh
dotnet fake build -t publish
```

## Pack Resource

パスワードは ``

```sh
dotnet fake build -t resources.pack
```

## Update .NET local tools

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
