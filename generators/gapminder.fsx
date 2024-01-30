#r "nuget: FSharp.Data"
#r "nuget: Deedle"
#r "nuget: Plotly.NET"
open FSharp.Data
open Deedle
open Plotly.NET
open Plotly.NET.LayoutObjects
//enable paraellel and cache
type WorldBank = WorldBankDataProvider<"World Development Indicators", Asynchronous=true>
let wb = WorldBank.GetDataContext()
let yrs = [|1950 .. 2021|]
let getdf refresh =    
    if refresh then
        let df =   
            // [for c in countries ->
            [for c in wb.Countries ->
                // let c = wb.Countries.Singapore
                async{
                    let! s = c.Indicators.``Life expectancy at birth, total (years)``
                    let s' = 
                        s
                        |> Seq.filter( fun (y,_) -> Array.contains y yrs )
                        |> Series.ofObservations
                    let! gdp = c.Indicators.``GDP per capita (constant 2015 US$)``
                    let gdp' = 
                        gdp
                        |> Seq.filter( fun (y,_) -> Array.contains y yrs )
                        |> Series.ofObservations
                    let! pop = c.Indicators.``Population, total``
                    let pop' = 
                        pop
                        |> Seq.filter( fun (y,_) -> Array.contains y yrs )
                        |> Series.ofObservations
                    let ct = Series.mapValues (fun _ -> c.Name) pop' 
                    let reg = Series.mapValues (fun _ -> c.Region) pop' 
                    let df = Frame.ofColumns [
                                "lifeExp" => s'; 
                                "gdpPercap" => gdp';
                                "pop" => pop']
                    df.AddColumn( "country", ct)
                    df.AddColumn( "continent", reg)
                    let year = gdp' |> Series.map( fun k v -> k ) 
                    df.AddColumn( "year", year)
                    return df.Rows.Values}]
            |> Async.Parallel
            |> Async.RunSynchronously
            |> Seq.collect id
            |> Frame.ofRowsOrdinal
            |> Frame.dropSparseRows
            |> Frame.mapRowKeys int
        df.SaveCsv("gapminder.csv", includeRowKeys = false)
        df
    else
        Frame.ReadCsv("assets/gapminder.csv") 
let df = getdf false
let continents =  (df.GetColumn<string> "continent").Values |> Seq.distinct |> Seq.sort |> Seq.toArray
let steps = (df.GetColumn<int> "year").Values |> Seq.distinct |> Seq.sort |> Seq.toArray
let sliderSteps steps  continents =
    let nc = continents |> Array.length
    let ns = steps |> Array.length
    steps
    |> Seq.indexed
    |> Seq.map (fun (i, step) ->
        // Create a visibility and a title parameter
        // The visibility parameter includes an array where every parameter
        // is mapped onto the trace visibility
        let visible =
            // Set true only for the current step
            // (fun index -> index = i) |> Array.init (steps.Length ) |> box
            (fun index -> index >= i * nc && index < ( i + 1 )* nc ) 
            |> Array.init (ns * nc) 
            |> box
        let title = sprintf "Year: %i" step |> box
        SliderStep.init (
            Args = [ "visible", visible; "title", title ],
            Method = StyleParam.Method.Update,
            Label = string (step)
        ))
let slider =
    Slider.init (
        CurrentValue = SliderCurrentValue.init (Prefix = "Year: "),
        Padding = Padding.init (T = 50),
        Steps = sliderSteps steps continents
    )
let scattersChart df steps continents=
    let hoverTemplate = "<i>GDP Percap</i>: %{x:.2f}"+
                        "<br><i>Life Exp</i>: %{y:.2f}"+
                        "<br><i>Country</i>: %{text}" 
    let xaxis = LinearAxis.init(AxisType=StyleParam.AxisType.Log, Range = StyleParam.Range.ofMinMax((2, 5)))
    let yaxis = LinearAxis.init(AxisType=StyleParam.AxisType.Linear, Range=StyleParam.Range.ofMinMax( 20, 90))
    steps
    |> Seq.map (fun year ->
        // Create a scatter plot for every step
        // Some plot must be visible here or the chart is empty at the beginning
        let chartVisibility =
            if year = (steps |> Seq.min) then
                StyleParam.Visible.True
            else
                StyleParam.Visible.False
        let go = 
            continents
            |> Seq.map( fun k -> 
                let data= 
                    df 
                    |> Frame.filterRowValues( fun o -> 
                        (o.GetAs<int> "year" = year) && 
                        ((o.GetAs<string> "continent") = k)) 
                let x = (data?gdpPercap).Values
                let y = data?lifeExp.Values
                let s = data?pop.Values
                let n = (data.GetColumn<string> "country").Values
                let s' = s |> Seq.map( fun x -> max 8 (x**0.3 /10. |> int) )
                Chart.Bubble( x, y, s', Name = k) 
                |> Chart.withXAxis xaxis
                |> Chart.withYAxis yaxis
                |> GenericChart.mapTrace  (Trace2DStyle.Scatter(HoverTemplate=hoverTemplate,MultiText=n))
                )
            |> Chart.combine
            |> Chart.withTraceInfo (Visible = chartVisibility)
        go)
    |> Chart.combine
    |> Chart.withSize(550,500)
let c = scattersChart df steps continents |> Chart.withSlider slider
let chart = c |> GenericChart.toChartHTML
// let chart = "test"
// let about = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Morbi nisi diam, vehicula quis blandit id, suscipit sed libero. Proin at diam dolor. In hac habitasse platea dictumst. Donec quis dui vitae quam eleifend dignissim non sed libero. In hac habitasse platea dictumst. In ullamcorper mollis risus, a vulputate quam accumsan at. Donec sed felis sodales, blandit orci id, vulputate orci."

#r "../_lib/Markdig.dll"
open Markdig
let markdownPipeline =
    MarkdownPipelineBuilder()
        .UsePipeTables()
        .UseGridTables()
        .Build()

let post = """### Gapminder

[Gapminder](https://www.gapminder.org/) is a good topic covering world development, data analytics with good insights.

I spend a Saturday afternoon to recreate a similar interactive chart with data from world bank [](https://data.worldbank.org/), using fsharp and [Plotly.Net](https://plotly.net/) gdp per capita vs life expectancy over years, sized in populatio and colored by region. 

I struggled a bit on the animation part, so just left the result as a manual slider. It was a fun exercise for me to figure out how slider works by setting the visibility mask, and formating different aspects of the chart.

The source code is here [source](https://github.com/xqguo/Gapminder)

"""

let posthtml = Markdown.ToHtml( post, markdownPipeline)
#r "../_lib/Fornax.Core.dll"
#load "layout.fsx"
open Html
let generate' (ctx : SiteContents) (_: string) =
  let siteInfo = ctx.TryGetValue<Globalloader.SiteInfo> ()
  let desc =
    siteInfo
    |> Option.map (fun si -> si.description)
    |> Option.defaultValue ""

  Layout.layout ctx "Gapminder" [
    section [Class "hero is-info is-medium is-bold"] [
      div [Class "hero-body"] [
        div [Class "container has-text-centered"] [
          h1 [Class "title"] [!!desc]
        ]
      ]
    ]
    div [Class "container"] [
      section [Class "articles"] [
        div [Class "column is-8 is-offset-2"] [
            div [Class "card article"] [
                div [Class "card-content"] [
                    div [] [ 
                      !! """<script src="https://cdn.plot.ly/plotly-latest.min.js"></script>""" ]
                    div [Class "content article-body"] [!! posthtml]                    
                    div [Class "media-content"] [ !! chart ]
                ]
            ]
        ]
      ]
    ]]
let generate (ctx : SiteContents) (projectRoot: string) (page: string) =
  generate' ctx page
  |> Layout.render ctx
