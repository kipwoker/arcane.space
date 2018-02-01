module Program

open Freya.Core
open Suave
open System

open Infrastructure

[<EntryPoint>]
let main args =
    let port = if args.Length > 0 then Int32.Parse(args.[0]) else 8080
    try
        startWebServer
            {
                defaultConfig with 
                    bindings = [ HttpBinding.createSimple Protocol.HTTP "127.0.0.1" port ] 
                    cancellationToken = serverCancellationTokenSource.Token
            }
            (Owin.OwinApp.ofAppFunc "/" (OwinAppFunc.ofFreya Api.root))
    with
    | :? OperationCanceledException  -> printfn "Server shutdown"
    0