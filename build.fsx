// --------------------------------------------------------------------------------------
// FAKE build script
// --------------------------------------------------------------------------------------

#r "packages/FAKE/tools/FakeLib.dll"
open Microsoft.FSharp.Reflection
open Fake
open System.Threading
open System
open System.IO

System.Environment.CurrentDirectory <- __SOURCE_DIRECTORY__

let solution = "arcane.space.sln"

type Configuration = Debug | Release

let configurations = FSharpType.GetUnionCases typeof<Configuration> |> Seq.map (fun u -> u.Name)

configurations
|> Seq.iter 
  (fun configuration ->
    let withConf value = value + configuration

    Target ("clean" |> withConf) (fun _ ->
      CleanDirs ["bin"; configuration]
    )

    Target ("build" |> withConf) (fun _ ->
      DotNetCli
        .Build 
        (fun p ->
                { 
                  p with 
                    Project = solution
                    Configuration = configuration
                } 
        )  
    )

    ("clean" |> withConf) ==> ("build" |> withConf)

    |> ignore
  )

let toString (x:'a) = 
    match FSharpValue.GetUnionFields(x, typeof<'a>) with
    | case, _ -> case.Name

// --------------------------------------------------------------------------------------
// DEBUG
// --------------------------------------------------------------------------------------

let port = "8111"
let rootUrl = String.Format("http://localhost:{0}", port)

let startCancellationTokenSource = new CancellationTokenSource()

let waitFor path condition waitingMsg =
  let mutable successed = false
  while not successed do
    try
      use webClient = new System.Net.WebClient()
      let response = webClient.DownloadString(rootUrl + "/" + path)
      successed <- condition response
    with _ ->
      System.Threading.Thread.Sleep(1000)
      printfn waitingMsg

let waitWhile path condition waitingMsg =
  let mutable successed = false
  while not successed do
    try
      use webClient = new System.Net.WebClient()
      let response = webClient.DownloadString(rootUrl + "/" + path)
      if condition response then
        System.Threading.Thread.Sleep(1000)
        printfn waitingMsg
      else
        successed <- true      
    with _ ->
      successed <- true

Target "start" (fun _ ->
  let startTask = 
    async { 
      return DotNetCli
        .RunCommand
        (fun p -> 
            {p with
              WorkingDir = Path.Combine(__SOURCE_DIRECTORY__, "backend")
              ToolPath = "dotnet"
            })
        ("run " + port)
      } 

  Async.Start (startTask, startCancellationTokenSource.Token)
  
  waitFor "ping" (fun response -> response = "pong") "Waiting for server to start..."
  traceImportant "Servers started...."
)

Target "run" (fun _ ->
  traceImportant "Press any key to stop!"
  Console.ReadKey() 
  |> (fun _ -> 
        waitWhile "shutdown" (fun response -> response = "shutdown") "Waiting for server to shutdown..."
        startCancellationTokenSource.Cancel()
     )
  |> ignore
)

"start" ==> "run"

Target "rerun" (fun _ ->
  "build" + (Debug |> toString) |> Run
  "run" |> Run
)

// --------------------------------------------------------------------------------------
// RELEASE
// --------------------------------------------------------------------------------------

//https://docs.microsoft.com/ru-ru/dotnet/core/rid-catalog
let runtime = "win10-x64"

Target "deploy" (fun _ ->
  DotNetCli
    .Publish
    (fun p -> 
      { 
        p with 
          Project = solution
          Configuration = Release |> toString
          Runtime = runtime
      }
    )
)

"build" + (Release |> toString) ==> "deploy"



// DEBUG DEFAULT
RunTargetOrDefault "run"