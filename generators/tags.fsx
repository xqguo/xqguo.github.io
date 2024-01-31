#r "../_lib/Fornax.Core.dll"
#load "layout.fsx"

open Html

let generate' (ctx : SiteContents) (_: string) =
  let posts = 
    ctx.TryGetValues<Postloader.Post> () 
    |> Option.defaultValue Seq.empty 
    |> Seq.toList
    |> List.sortByDescending Layout.published

  let tags =
    posts
    |> Seq.toList
    |> List.map( fun p -> p.tags)
    |> List.collect id
    |> List.distinct

  let siteInfo = ctx.TryGetValue<Globalloader.SiteInfo> ()
  let desc =
    siteInfo
    |> Option.map (fun si -> si.description)
    |> Option.defaultValue ("")

  let psts =
    tags 
    |> List.map( fun tag -> tag, posts |> List.filter(fun x -> List.contains tag x.tags ))
    |> List.map( fun (t,p) -> t, p |> List.map (Layout.postLayout true))

  let layoutForPostSet  tag psts =

    Layout.layout ctx "Home" [
      section [Class "hero is-info is-medium is-bold"] [
        div [Class "hero-body"] [
          div [Class "container has-text-centered"] [
            h1 [Class "title"] [!!desc]
          ]
        ]
      ]
      div [Class "container"] [
        section [Class "articles"] [
          div [Class "column is-8 is-offset-2"] psts
        ]
      ]
      div [Class "container"] [
        div [Class "container has-text-centered"] [
        ]
      ]]

  psts
  |> List.map (fun (tag, psts) ->
        Layout.getFilenameForTag tag,
        layoutForPostSet  tag psts
        |> Layout.render ctx)

let generate (ctx : SiteContents) (projectRoot: string) (page: string) =
    generate' ctx page