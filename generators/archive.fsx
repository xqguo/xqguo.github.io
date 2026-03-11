#r "../_lib/Fornax.Core.dll"
#load "layout.fsx"

open Html
open System

let generate' (ctx : SiteContents) (_: string) =
  let posts =
    ctx.TryGetValues<Postloader.Post> ()
    |> Option.defaultValue Seq.empty
    |> Seq.toList
    |> List.sortByDescending Layout.published

  let siteInfo = ctx.TryGetValue<Globalloader.SiteInfo> ()
  let desc =
    siteInfo
    |> Option.map (fun si -> si.description)
    |> Option.defaultValue ""

  let groupPostsByMonth (posts: Postloader.Post list) =
    posts
    |> List.groupBy (fun p ->
        let date = p.published |> Option.defaultValue DateTime.Now
        date.Year, date.Month)
    |> List.sortByDescending fst

  let monthPosts = groupPostsByMonth posts

  let layoutForPostSet title psts =
    Layout.layout ctx title [
      section [Class "hero is-info is-medium is-bold"] [
        div [Class "hero-body"] [
          div [Class "container has-text-centered"] [
            h1 [Class "title"] [!!title]
          ]
        ]
      ]
      div [Class "container"] [
        section [Class "articles"] [
          div [Class "column is-8 is-offset-2"] psts
        ]
      ]
    ]

  let archiveIndex =
    Layout.layout ctx "Archives" [
      section [Class "hero is-info is-medium is-bold"] [
        div [Class "hero-body"] [
          div [Class "container has-text-centered"] [
            h1 [Class "title"] [!! "Archives"]
          ]
        ]
      ]
      div [Class "container"] [
        section [Class "section"] [
          div [Class "column is-8 is-offset-2"] [
            div [Class "content"] [
              ul [] [
                for (year, month), _ in monthPosts do
                  let monthName = DateTime(year, month, 1).ToString("MMMM yyyy")
                  li [] [
                    a [Href (Layout.getLinkForArchive (year, month))] [!! monthName]
                  ]
              ]
            ]
          ]
        ]
      ]
    ]

  let monthPages =
    monthPosts
    |> List.map (fun ((year, month), psts) ->
        let monthName = DateTime(year, month, 1).ToString("MMMM yyyy")
        let pstsLayout = psts |> List.map (Layout.postLayout true)
        Layout.getFilenameForArchive (year, month),
        layoutForPostSet monthName pstsLayout |> Layout.render ctx)

  ("posts/archive/index.html", archiveIndex |> Layout.render ctx) :: monthPages

let generate (ctx : SiteContents) (projectRoot: string) (page: string) =
    generate' ctx page
