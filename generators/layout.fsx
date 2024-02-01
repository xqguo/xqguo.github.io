#r "../_lib/Fornax.Core.dll"
#if !FORNAX
#load "../loaders/postloader.fsx"
#load "../loaders/pageloader.fsx"
#load "../loaders/globalloader.fsx"
#endif

open Html

let injectWebsocketCode (webpage:string) =
    let websocketScript =
        """
        <script type="text/javascript">
          var wsUri = "ws://localhost:8080/websocket";
      function init()
      {
        websocket = new WebSocket(wsUri);
        websocket.onclose = function(evt) { onClose(evt) };
      }
      function onClose(evt)
      {
        console.log('closing');
        websocket.close();
        document.location.reload();
      }
      window.addEventListener("load", init, false);
      </script>
        """
    let head = "<head>"
    let index = webpage.IndexOf head
    webpage.Insert ( (index + head.Length + 1),websocketScript)


let getFilenameForTag tag =
        sprintf "posts/tags/%s.html" tag

let layout (ctx : SiteContents) active bodyCnt =
    let pages = ctx.TryGetValues<Pageloader.Page> () |> Option.defaultValue Seq.empty
    let siteInfo = ctx.TryGetValue<Globalloader.SiteInfo> ()
    let ttl =
      siteInfo
      |> Option.map (fun si -> si.title)
      |> Option.defaultValue ""

    let menuEntries =
      pages
      |> Seq.map (fun p ->
        let cls = if p.title = active then "navbar-item is-active" else "navbar-item"
        a [Class cls; Href p.link] [!! p.title ])
      |> Seq.toList


    html [] [
        head [] [
            meta [CharSet "utf-8"]
            meta [Name "viewport"; Content "width=device-width, initial-scale=1"]
            title [] [!! ttl]
            link [Rel "icon"; Type "image/png"; Sizes "32x32"; Href "/images/favicon.png"]
            link [Rel "stylesheet"; Href "https://maxcdn.bootstrapcdn.com/font-awesome/4.7.0/css/font-awesome.min.css"]
            link [Rel "stylesheet"; Href "https://fonts.googleapis.com/css?family=Open+Sans"]
            link [Rel "stylesheet"; Href "https://unpkg.com/bulma@0.8.0/css/bulma.min.css"]
            link [Rel "stylesheet"; Type "text/css"; Href "/style/style.css"]

        ]
        body [] [
          nav [Class "navbar is-light"] [
            div [Class "container"] [
              div [Class "navbar-brand"] [
                a [Class "navbar-item"; Href "/"] [ !! "Home" ]
                // a [Class "navbar-item"; Href "/about.html"] [ !! "About" ]
                // a [Class "navbar-item"; Href "/contact.html"] [ !! "Contact" ]
                span [Class "navbar-burger"; HtmlProperties.Custom ("data-target", "navbarMenu")] [//burgerEntries
                // span [Class "navbar-burger";] [
                  span [] []
                  span [] []
                  span [] []
                ]
              ]
              div [Id "navbarMenu"; Class "navbar-menu"] menuEntries
              script [ Async true; Src "https://cse.google.com/cse.js?cx=c7bf194decda14529" ] [] 
              div [Class "gcse-search"] []
            ]
          ]
          script [ Src "/js/mobile.js" ] [] 
          yield! bodyCnt
        ]
    ]

let render (ctx : SiteContents) cnt =
  let disableLiveRefresh = ctx.TryGetValue<Postloader.PostConfig> () |> Option.map (fun n -> n.disableLiveRefresh) |> Option.defaultValue false
  cnt
  |> HtmlElement.ToString
  |> fun n -> if disableLiveRefresh then n else injectWebsocketCode n

let published (post: Postloader.Post) =
    post.published
    |> Option.defaultValue System.DateTime.Now
    |> fun n -> n.ToString("yyyy-MM-dd")

let postLayout (useSummary: bool) (post: Postloader.Post) =
    let hasSummary = post.summary.Length < post.content.Length
    let tags=
      post.tags
      |> List.map (fun tag->
        a [Href $"/posts/tags/{tag}.html" ] [!! tag ])
    div [Class "card article"] [
        div [Class "card-content"] [
            div [Class "media-content has-text-centered"] [
                p [Class "title article-title"; ] [ a [Href post.link] [!! post.title]]
                p [Class "subtitle is-6 article-subtitle"] [
                a [Href "#"] [!! (defaultArg post.author "")]
                !! (sprintf "on %s" (published post))
                ]
            ]
            div [Class "content article-body"] [
                !! (if useSummary then post.summary else post.content)
                (if useSummary && hasSummary then a [ Href post.link] [!! "More ... "] else !! "")

            ]
            div [] [
              !! "Tags: " 
              div [] tags
            ]
        ]
    ]
