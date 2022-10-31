#if FAKE
#r "paket:
source https://api.nuget.org/v3/index.json
storage: none
nuget FSharp.Core >= 6.0
nuget Fake.Core.Target
nuget Fake.IO.FileSystem
nuget Fake.DotNet.Cli
nuget Fake.Net.Http
nuget FSharp.Json //"

#endif

#load ".fake/build.fsx/intellisense.fsx"

#r "netstandard"

open Fake.Core
open Fake.Core.TargetOperators
open Fake.IO
open Fake.IO.Globbing.Operators

module private Params =
  // ターゲットのプロジェクト名を参照する
  let ProjectName = "Altseed2Template"

  // ターゲットのアセンブリ名を参照する
  let AssemblyName = ProjectName

  // ターゲットのプロジェクトを参照する
  let PublishTarget = $"src/%s{ProjectName}/%s{ProjectName}.csproj"

  module Resources =

    // 指定したファイルの内容をリソースパッケージのパスワードとして利用する
    let PasswordPath = $"src/%s{ProjectName}/ResourcesPassword.txt"

    // リソースパッケージのパス
    let PackagePath = "Resources.pack"

    // リソースパッケージのダウンロード先に指定するURLを格納した環境変数
    let DownloadUrlEnv = "RESOURCES_DOWNLOAD_URL"

  module Dist =
    let WindowsZipName = $"%s{ProjectName}.win-x64.zip"

    let MacOSDmgName = $"%s{ProjectName}.dmg"

  /// オートフォーマッターを使って自動整形する場合の対象を指定する
  module FormatTargets =
    let csharp =
      !! "src/**/*.csproj"
      ++ "tests/**/*.csproj"

    let fsharp =
      !! "src/**/*.fs" ++ "build.fsx"
      -- "src/*/obj/**/*.fs"
      -- "src/*/bin/**/*.fs"


open Fake.DotNet
open Fake.Net
open FSharp.Json

[<AutoOpen>]
module private Utils =
  let runtimeToPubDir (runtime: string) = sprintf "publish/%s" runtime

  let getConfiguration (input: string option) =
    input
    |> Option.map (fun s -> s.ToLower())
    |> function
      | Some "debug" -> DotNet.BuildConfiguration.Debug
      | Some "release" -> DotNet.BuildConfiguration.Release
      | Some c -> failwithf "Invalid configuration '%s'" c
      | None -> DotNet.BuildConfiguration.Debug

  let dotnet cmd =
    Printf.kprintf (fun arg ->
      let res = DotNet.exec id cmd arg

      if not res.OK then
        let msg = res.Messages |> String.concat "\n"

        failwithf "Failed to run 'dotnet %s %s' due to: %A" cmd arg msg
    )

  let shell (dir: string option) cmd =
    Printf.kprintf (fun arg ->
      let dir = dir |> Option.defaultValue "."

      let res = Shell.Exec(cmd, arg, dir)

      if res <> 0 then
        failwithf "Failed to run '%s %s' at '%A'" cmd arg dir
    )

module Zip =
  open System.IO.Compression
  open System.Text

  let zipDirectory src dest =
    ZipFile.CreateFromDirectory(src, dest, CompressionLevel.Optimal, true)


Target.initEnvironment ()

let args = Target.getArguments ()

(* クリーンする *)

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

(* ビルドする *)

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

Target.create "Publish.win" (fun _ -> publishForRuntime "win-x64")

Target.create "Publish.osx" (fun _ -> publishForRuntime "osx-x64")

Target.create "Publish" ignore

"Publish.win" ==> "Publish"

"Publish.osx" ==> "Publish"



(* 配布用にファイルをまとめる *)

Target.create
  "LICENSES"
  (fun _ ->
    Directory.ensure "publish/licenses"
    dotnet "dotnet-project-licenses" "--input %s --output-directory publish/licenses -e" Params.PublishTarget
  )

Target.create
  "Dist.win"
  (fun _ ->
    File.checkExists Params.Resources.PackagePath

    let tempDirToZip = $"publish/temp/win-x64/%s{Params.ProjectName}"

    let targetZipName = $"publish/output/%s{Params.Dist.WindowsZipName}"

    Trace.tracefn "Cleaning"
    do
      File.delete targetZipName
      Directory.delete tempDirToZip

    Trace.tracefn "Ensuring"
    do
      Directory.ensure tempDirToZip
      Directory.ensure "publish/output"

    Trace.tracefn "Copying files"
    do
      Shell.cp_r $"dist/contents" $"%s{tempDirToZip}/"
      Shell.cp_r "publish/licenses" $"%s{tempDirToZip}/licenses"
      Shell.cp_r (runtimeToPubDir "win-x64") tempDirToZip

    Trace.tracefn "Creating zip"
    do Zip.zipDirectory tempDirToZip targetZipName
  )

Target.create
  "Dist.osx"
  (fun _ ->
    if not
       <| File.exists Params.Resources.PackagePath then
      failwithf "リソースパッケージ '%s' が見つかりません" Params.Resources.PackagePath

    let tempDirToDmg = $"publish/temp/osx-x64/%s{Params.ProjectName}"

    let tempDirToApp = $"%s{tempDirToDmg}/%s{Params.ProjectName}.app"
    let scriptDir = $"%s{tempDirToApp}/Contents/MacOS"
    let resourceDir = $"%s{tempDirToApp}/Contents/Resources"
    let scriptPath = $"%s{scriptDir}/script.sh"

    let targetDmgName = $"publish/output/%s{Params.Dist.MacOSDmgName}"

    Trace.tracefn "Cleaning"
    do
      Directory.delete tempDirToApp
      Directory.delete tempDirToDmg
      File.delete targetDmgName

    Trace.tracefn "Ensuring"
    do
      Directory.ensure tempDirToApp
      Directory.ensure "publish/output"

    Trace.tracefn "Copying files"
    do
      Shell.cp_r $"dist/contents" $"%s{tempDirToDmg}/"
      Shell.cp_r "publish/licenses" $"%s{resourceDir}/licenses"
      Shell.cp_r "dist/App" tempDirToApp

    Trace.tracefn "Creating script.sh"
    do
      if not <| File.exists scriptPath then
        File.create scriptPath

      $"""#!/bin/bash
cd `dirname $0`
./%s{Params.AssemblyName}
"""
      |> File.writeString false scriptPath

      Shell.cp_r (runtimeToPubDir "osx-x64") scriptDir

    Trace.tracefn "Creating dmg"
    do shell None "hdiutil" "create %s -volname \"%s\" -srcfolder \"%s\"" targetDmgName Params.ProjectName tempDirToDmg
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

Target.create "Format" ignore

Target.create "Format.Check" ignore

(* dotnet-format を使用してC#コードをフォーマットする場合 *)
// "Format.CSharp" ==> "Format"
// "Format.Check.CSharp" ==> "Format.Check"

(* fantomas を使用してF#コードをフォーマットする場合 *)
// "Format.FSharp" ==> "Format"
// "Format.Check.FSharp" ==> "Format.Check"



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
    let password = File.readAsString Params.Resources.PasswordPath

    // Altseed2.Tools (.NETツール) http://altseed.github.io/Manual/CLITool.html
    dotnet "altseed2" "file -s ./Resources -o %s -p %s" Params.Resources.PackagePath password
  )

// パッケージファイルをダウンロードする

Target.create
  "Resources.Download"
  (fun _ ->
    let url = Environment.environVar Params.Resources.DownloadUrlEnv
    Http.downloadFile Params.Resources.PackagePath url
    |> ignore
  )

Target.create "Resources.CI" ignore

(* CIで直接 `Resources.pack` を作る場合 *)
"Resources.Pack" ==> "Resources.CI"

(* CIでリソースパッケージをダウンロードする場合 *)
// "Resources.Download" ==> "Resources.CI"


// 何もしないターゲット
Target.create "Nothing" ignore


Target.runOrDefaultWithArguments "Build"
