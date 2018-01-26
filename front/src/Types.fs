module App.Types

open Global

type Msg =
  | HomeMsg

type Model = {
    currentPage: Page
}
