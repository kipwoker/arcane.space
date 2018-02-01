module Api

open Freya.Core
open Freya.Machines.Http
open Freya.Types.Http
open Freya.Routers.Uri.Template

let name' = Route.atom_ "name"

let name =
    freya {
        let! nameO = Freya.Optic.get name'

        match nameO with
        | Some name -> return name
        | None -> return "World" }

let sayHello =
    freya {
        let! name = name

        return Represent.text (sprintf "Hello, %s!" name) }

let helloMachine =
    freyaMachine {
        methods [GET; HEAD; OPTIONS]
        handleOk sayHello }

let ping =
    freya {
        return Represent.text "pong"        
    }

let pingMachine =
    freyaMachine {
        methods [GET]
        handleOk ping
    }


let shutdown = freya {
        Infrastructure.serverCancellationTokenSource.CancelAfter(1000)
        return Represent.text "shutdown"
    }

let shutdownMachine =
    freyaMachine {
        methods [GET]
        handleOk shutdown
    }

let root =
    freyaRouter {
        resource "/hello{/name}" helloMachine
        resource "/ping" pingMachine
        #if DEBUG
        resource "/shutdown" shutdownMachine
        #endif
    }
