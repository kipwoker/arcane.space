module Global

type Page =
    | Home

let toHash page =
    match page with
    | Home -> "#home"

type Resource =
    | Github
    | Flickr
    | Soundcloud

let getUrl resource =
    match resource with
    | Github -> "https://github.com/kipwoker"
    | Flickr -> "https://www.flickr.com/photos/155206640@N04/albums"
    | Soundcloud -> "https://soundcloud.com/lewruge"
