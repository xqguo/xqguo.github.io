#r "../_lib/Fornax.Core.dll"

type SiteInfo = {
    title: string
    description: string
    postPageSize: int
}

let loader (projectRoot: string) (siteContent: SiteContents) =
    let siteInfo =
        { title = "Xiaoqiang's blog";
          description = "Xiaoqiang's Space"
          postPageSize = 5 }
    siteContent.Add(siteInfo)

    siteContent
