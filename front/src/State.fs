module App.State

open Elmish
open Elmish.Browser.Navigation
open Elmish.Browser.UrlParser
open Fable.Import.Browser
open Global
open App.Types

let pageParser: Parser<Page->Page,Page> =
    oneOf [
        map Home (s "home")
    ]

let urlUpdate (result: Option<Page>) model =
  match result with
  | None ->
    console.error("Error parsing url")
    model,Navigation.modifyUrl (toHash model.currentPage)
  | Some page ->
      { model with currentPage = page }, []

let init result =
  let (model, cmd) =
    urlUpdate result
      { currentPage = Home }
  model, Cmd.batch [ cmd ]

let update msg model =
    match msg with
    | HomeMsg -> model, []
