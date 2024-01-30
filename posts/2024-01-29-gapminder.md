---
layout: post
title: Gapminder
published: 2024-01-29
---

[Gapminder](https://www.gapminder.org/) is a good topic covering world development, data analytics with good insights.

I spent a Saturday afternoon to recreate a similar interactive chart with data from world bank [](https://data.worldbank.org/), using fsharp and [Plotly.Net](https://plotly.net/) gdp per capita vs. life expectancy over years, sized in populatio and colored by region. 

I struggled a bit on the animation part, so just left the result as a manual slider. It was a fun exercise for me to figure out how slider works by setting the visibility mask, and formating different aspects of the chart.

The source code is here [source](https://github.com/xqguo/Gapminder)

The chart can be viewed [here](/gapminder.html)
