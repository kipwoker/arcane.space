module App.View

open Elmish
open Elmish.Browser.Navigation
open Elmish.Browser.UrlParser
open Fable.Core.JsInterop
open App.State
open Global

importAll "../sass/main.sass"

open Fable.Helpers.React
open Fable.Helpers.React.Props

let menuItem label resource =
    li
      [ ]
      [ a
          [ classList [ "is-active", false ]
            Href (getUrl resource); Target "_blank" ]
          [ str label ] ]

let menu =
  aside
    [ ClassName "menu" ]
    [ p
        [ ClassName "menu-label" ]
        [ str "Kirill Ivanov" ]
      ul
        [ ClassName "menu-list" ]
        [ menuItem "Github" Github
          menuItem "Flickr" Flickr
          menuItem "Soundcloud" Soundcloud ] ]

let root model dispatch =

  div
    []
    [ div
        [ ClassName "section" ]
        [ div
            [ ClassName "container" ]
            [ div
                [ ClassName "columns" ]
                [ div
                    [ ClassName "column is-3 menu-container" ]
                    [ menu ] ] ] ] ]

open Elmish.React
open Elmish.Debug
open Elmish.HMR

// App
Program.mkProgram init update root
|> Program.toNavigable (parseHash pageParser) urlUpdate
#if DEBUG
|> Program.withDebugger
|> Program.withHMR
#endif
|> Program.withReact "elmish-app"
|> Program.run
