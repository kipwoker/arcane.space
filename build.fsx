// --------------------------------------------------------------------------------------
// FAKE build script
// --------------------------------------------------------------------------------------

#r "packages/FAKE/tools/FakeLib.dll"
open Fake
open System
open System.IO
open FSharp.Data

System.Environment.CurrentDirectory <- __SOURCE_DIRECTORY__

// --------------------------------------------------------------------------------------
// For deployed run - compile as an executable
// --------------------------------------------------------------------------------------

Target "clean" (fun _ ->
  CleanDirs ["bin"]
)

Target "build" (fun _ ->
  [ "arcane.space.sln" ]
  |> MSBuildRelease "" "Rebuild"
  |> Log ""
)

"clean" ==> "build"

// --------------------------------------------------------------------------------------
// For local run - automatically reloads scripts
// --------------------------------------------------------------------------------------

Target "start" (fun _ ->
  async { return startServers() } 
  |> Async.Ignore
  |> Async.Start
  
  let mutable started = false
  while not started do
    try
      use wc = new System.Net.WebClient()
      started <- wc.DownloadString("http://localhost:8080/").Contains("running")
    with _ ->
      System.Threading.Thread.Sleep(1000)
      printfn "Waiting for servers to start...."
  traceImportant "Servers started...."
)

Target "run" (fun _ ->
  traceImportant "Press any key to stop!"
  Console.ReadKey() |> ignore
)

"start" ==> "run"

// --------------------------------------------------------------------------------------
// Azure - deploy copies the binary to wwwroot/bin
// --------------------------------------------------------------------------------------

let newName prefix f = 
  Seq.initInfinite (sprintf "%s_%d" prefix) |> Seq.skipWhile (f >> not) |> Seq.head

Target "deploy" (fun _ ->
  // Pick a subfolder that does not exist
  let wwwroot = "../wwwroot"
  let subdir = newName "deploy" (fun sub -> not (Directory.Exists(wwwroot </> sub)))
  
  // Deploy everything into new empty folder
  let deployroot = wwwroot </> subdir
  CleanDir deployroot
  CleanDir (deployroot </> "bin")
  CopyRecursive "bin" (deployroot </> "bin") false |> ignore
  let config = File.ReadAllText("backend/web.config").Replace("%DEPLOY_SUBDIRECTORY%", subdir)
  File.WriteAllText(wwwroot </> "web.config", config)

  // Try to delete previous folders, but ignore failures
  for dir in Directory.GetDirectories(wwwroot) do
    if Path.GetFileName(dir) <> subdir then 
      try CleanDir dir; DeleteDir dir with _ -> ()
)

"build" ==> "deploy"

RunTargetOrDefault "run"
