module Program

open Freya.Core
open Suave
open System

[<EntryPoint>]
let main args =
    let port = if args.Length > 0 then Int32.Parse(args.[0]) else 8080
    startWebServer
        {defaultConfig with bindings = [ HttpBinding.createSimple Protocol.HTTP "127.0.0.1" port ] }
        (Owin.OwinApp.ofAppFunc "/" (OwinAppFunc.ofFreya Api.root))

    0