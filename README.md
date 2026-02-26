# xqguo.github.io

This repository contains the source and generated files for the personal website hosted at xqguo.github.io. It uses F# scripts and Fornax to generate static content.

## Structure

- `posts/` – Markdown source for blog posts
- `generators/` – F# scripts to generate pages
- `assets/` – Static assets including images, CSS, and JavaScript
- `docs/` – Generated output (often used for hosting)

## Building

Run the following command in the root directory:

```powershell
fornax.exe build
```

This will generate the site under the `_public` folder, which needs to be renamed into `docs` and linked to Github pages site.

Enjoy!
