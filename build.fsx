#r "paket:
source https://api.nuget.org/v3/index.json
nuget FSharp.Core >= 6.0
nuget Fake.Core.Target
nuget Fake.IO.FileSystem
nuget Fake.DotNet.Cli
nuget FAKE.IO.Zip
nuget Fake.Net.Http
nuget FSharp.Json //"

#load ".fake/build.fsx/intellisense.fsx"

open Fake.Core
open Fake.Core.TargetOperators
open Fake.IO
open Fake.IO.Globbing.Operators
open Fake.DotNet
open Fake.Net

open FSharp.Json

module private Params =
  // ターゲットのプロジェクト名を参照する
  let ProjectName = "Altseed2Template"

  // ターゲットのアセンブリ名を参照する
  let AssemblyName = ProjectName

  // ターゲットのプロジェクトを参照する
  let PublishTarget = $"src/%s{ProjectName}/%s{ProjectName}.csproj"

  // 指定したファイルの内容をリソースパッケージのパスワードとして利用する
  let PasswordFilename = $"src/%s{ProjectName}/ResourcesPassword.txt"

  // リソースパッケージのパス
  let ResourcesPackagePath = "Resources.pack"

  // リソースパッケージのダウンロード先に指定するURL
  let ResourcesPackageURL =
    // Some @"https://example.com/foo/bar/ExampleResources.pack"
    None

  let PublishDirectory = "publish"

  /// オートフォーマッターを使って自動整形する場合の対象を指定する
  module FormatTargets =
    let csharp =
      !! "src/**/*.csproj"
      ++ "tests/**/*.csproj"

    let fsharp =
      !! "src/**/*.fs"
      ++ "build.fsx"
      -- "src/*/obj/**/*.fs"
      -- "src/*/bin/**/*.fs"


[<AutoOpen>]
module private Utils =
  let dotnet cmd =
    Printf.kprintf (fun arg ->
      let res = DotNet.exec id cmd arg

      if not res.OK then
        let msg =
          res.Messages
          |> String.concat "\n"

        failwithf "Failed to run 'dotnet %s %s' due to: %A" cmd arg msg
    )

  let shell (dir: string option) cmd =
    Printf.kprintf (fun arg ->
      let dir = dir |> Option.defaultValue "."
      let res = Shell.Exec(cmd, arg, dir)

      if res <> 0 then
        failwithf "Failed to run '%s %s' at '%A'" cmd arg dir
    )

  let getConfiguration (input: string option) =
    input
    |> Option.map (fun s -> s.ToLower())
    |> function
      | Some "debug" -> DotNet.BuildConfiguration.Debug
      | Some "release" -> DotNet.BuildConfiguration.Release
      | Some c -> failwithf "Invalid configuration '%s'" c
      | None -> DotNet.BuildConfiguration.Debug

  let runtimeToPubDir (runtime: string) =
    sprintf "%s/%s" Params.PublishDirectory runtime

Target.initEnvironment ()

let args = Target.getArguments ()



Target.create
  "Clean"
  (fun _ ->
    !! "src/**/bin"
    ++ "src/**/obj"
    ++ "tests/**/bin"
    ++ "tests/**/obj"
    ++ "lib/**/bin"
    ++ "lib/**/obj"
    ++ "publish"
    |> Shell.cleanDirs
  )



Target.create
  "Build"
  (fun _ ->
    let conf =
      args
      |> Option.bind Array.tryHead
      |> getConfiguration

    !! "src/**/*.*proj"
    ++ "tests/**/*.*proj"
    |> Seq.iter (DotNet.build (fun p -> { p with Configuration = conf }))
  )



(* runtime 向けの単一実行ファイルにpublishビルドする *)

let publishForRuntime runtime =
  Params.PublishTarget
  |> DotNet.publish (fun p ->
    { p with
        Runtime = Some runtime
        Configuration = DotNet.BuildConfiguration.Release
        SelfContained = Some true
        OutputPath = Some(runtimeToPubDir runtime)
        MSBuildParams =
          { p.MSBuildParams with
              DisableInternalBinLog = true
              Properties =
                List.append
                  [ "OutputType", "WinExe"
                    "PublishSingleFile", "true"
                    "PublishTrimmed", "true"
                    "DebugSymbols", "false"
                    "DebugType", "None" ]
                  p.MSBuildParams.Properties } }
  )

Target.create "Publish.win-x64" (fun _ -> publishForRuntime "win-x64")

Target.create "Publish.osx-x64" (fun _ -> publishForRuntime "osx-x64")

Target.create "Publish" ignore

"Publish.win-x64"
==> "Publish"

"Publish.osx-x64"
==> "Publish"



(* 配布用にファイルをまとめる *)

Target.create
  "Dist.win-x64"
  (fun _ ->
    if not
       <| File.exists Params.ResourcesPackagePath then
      failwithf "リソースパッケージ '%s' が見つかりません" Params.ResourcesPackagePath
    
    Directory.ensure Params.PublishDirectory

    Shell.cp "dist/README.md" (runtimeToPubDir "win-x64")

    // TODO
  )

Target.create
  "Dist.osx-x64"
  (fun _ ->
    if not
       <| File.exists Params.ResourcesPackagePath then
      failwithf "リソースパッケージ '%s' が見つかりません" Params.ResourcesPackagePath

    let appDir = $"%s{Params.PublishDirectory}/%s{Params.AssemblyName}"
    let scriptDir = $"%s{appDir}/Contents/MacOS"
    let scriptPath = $"%s{scriptDir}/script.sh"

    Directory.ensure appDir

    Shell.cp_r "dist/App" appDir

    if not
       <| File.exists scriptPath then
      File.create scriptPath

    $"""#!/bin/bash
cd `dirname $0`
$"./%s{Params.AssemblyName}
"""
    |> File.writeString false scriptPath

    Shell.cp_r (runtimeToPubDir "osx-x64") scriptDir

    !! $"%s{appDir}/**/.gitkeep"
    |> Seq.iter Shell.rm

    shell None "chmod" "+x %s/%s" scriptDir Params.AssemblyName
    shell None "chmod" "+x %s" scriptPath

    let appPath = appDir + ".app"

    Shell.mv appDir appPath

    // TODO: make dmg

    

    Shell.rm_rf appPath
  )



(* オートフォーマッターを利用してソースコードを整形する *)

Target.create
  "Format.CSharp"
  (fun _ ->
    Params.FormatTargets.csharp
    |> Seq.iter (fun proj -> dotnet "format" "%s -v diag" proj)
  )

Target.create
  "Format.Check.CSharp"
  (fun _ ->
    Params.FormatTargets.csharp
    |> Seq.iter (fun proj -> dotnet "format" "%s -v diag --verify-no-changes" proj)
  )

Target.create
  "Format.FSharp"
  (fun _ ->
    Params.FormatTargets.fsharp
    |> String.concat " "
    |> dotnet "fantomas" "%s"
  )

Target.create
  "Format.Check.FSharp"
  (fun _ ->
    Params.FormatTargets.fsharp
    |> String.concat " "
    |> dotnet "fantomas" "--check %s"
  )

Target.create "Format"

Target.create "Format.Check"

(* dotnet-format を使用してC#コードをフォーマットする場合は以下をコメントアウトする *)
// "Format.CSharp"
// ==> "Format"
// "Format.Check.CSharp"
// ==> "Format.Check"

(* fantomas を使用してF#コードをフォーマットする場合は以下をコメントアウトする *)
// "Format.FSharp"
// ==> "Format"
// "Format.Check.FSharp"
// ==> "Format.Check"



(* localにインストールされている.NET CLI Toolのバージョンをまとめて更新する *)

Target.create
  "Tool.Update"
  (fun _ ->
    let content = File.readAsString ".config/dotnet-tools.json"

    let manifest: {| tools: Map<string, {| version: string |}> |} =
      Json.deserialize content

    for x in manifest.tools do
      dotnet "tool" "update %s" x.Key
  )



(* Altseed2用のパッケージファイルを扱う *)

// Altseed2 .NETツールを利用してリソースフォルダをパッケージ化する
Target.create
  "Resources.Pack"
  (fun _ ->
    let password = File.readAsString Params.PasswordFilename

    // Altseed2.Tools (.NETツール) http://altseed.github.io/Manual/CLITool.html
    dotnet "altseed2" "file -s ./Resources -o %s -p %s" Params.ResourcesPackagePath password
  )

// パッケージファイルをダウンロードする
Target.create
  "Resources.Download"
  (fun _ ->
    Params.ResourcesPackageURL
    |> Option.filter (
      String.isNotNullOrEmpty
      >> not
    )
    |> Option.iter (fun url ->
      Http.downloadFile Params.ResourcesPackagePath url
      |> ignore
    )
  )

// CIでリソースファイルをどのように入手するか指定したい。
Target.create "Resources.CI" ignore

(* CIで直接 `Resources.pack` を作る場合は以下をコメントアウトする *)
// "Resources.Pack" ==> "Resources.CI"

(* CIでリソースパッケージをダウンロードする場合は以下をコメントアウトする *)
// "Resources.Download" ==> "Resources.CI"


// 何もしないターゲット
Target.create "Nothing" ignore


Target.runOrDefaultWithArguments "Build"
