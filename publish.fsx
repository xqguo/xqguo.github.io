open System
open System.IO
open System.Text.RegularExpressions
open System.Diagnostics

// --- Configuration ---
let vaultPath       = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "source/repo/note")
let attachmentPath  = Path.Combine(vaultPath, "attachments") // Adjust if your folder is named differently
let repoPath        = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "source/repo/xqguo.github.io")
let postsDir        = "posts"
let assetsDir       = "assets" // Fornax public assets folder

// --- Helper: Run Shell Commands ---
let execute command (args: string) workingDir =
    let startInfo: ProcessStartInfo = ProcessStartInfo(command, args)
    startInfo.WorkingDirectory <- workingDir
    startInfo.RedirectStandardOutput <- true
    startInfo.UseShellExecute <- false
    startInfo.CreateNoWindow <- true

    match Process.Start(startInfo) with
    | null -> -1
    | proc ->
        use p = proc
        let out = p.StandardOutput.ReadToEnd()
        p.WaitForExit()
        if not (String.IsNullOrWhiteSpace out) then printfn "%s" out
        p.ExitCode

// --- Logic to Process Images ---
let processImages (content: string) =
    // Matches both ![[image.png]] and ![alt](image.png)
    let pattern = @"!\[\[(.*?)\]\]|!\[.*?\]\((.*?)\)"
    let mutable updatedContent = content

    Regex.Matches(content, pattern)
    |> Seq.cast<Match>
    |> Seq.iter (fun m ->
        // Extract filename from whichever group matched
        let rawPath = if m.Groups.[1].Success then m.Groups.[1].Value else m.Groups.[2].Value
        let fileName = Path.GetFileName(rawPath)
        
        let sourceImg = Path.Combine(attachmentPath, fileName)
        let destImg   = Path.Combine(repoPath, assetsDir, fileName)

        if File.Exists(sourceImg) then
            if not (Directory.Exists(Path.GetDirectoryName(destImg))) then 
                Directory.CreateDirectory(Path.GetDirectoryName(destImg)) |> ignore
            
            let finalDestImg = 
                if File.Exists(destImg) then
                    let guid = Guid.NewGuid().ToString().Substring(0, 8)
                    let name = Path.GetFileNameWithoutExtension(fileName)
                    let ext = Path.GetExtension(fileName)
                    Path.Combine(repoPath, assetsDir, $"{name}-{guid}{ext}")
                else
                    destImg
            
            File.Copy(sourceImg, finalDestImg, true)
            printfn "📸 Copied Image: %s" (Path.GetFileName(finalDestImg))
            
            // Replace the link in the text to point to /assets/filename
            let newLink = $"![{Path.GetFileName(finalDestImg)}](/{assetsDir}/{Path.GetFileName(finalDestImg)})"
            updatedContent <- updatedContent.Replace(m.Value, newLink)
            
            // Replace the link in the text to point to /assets/filename
            let newLink = $"![{fileName}](/{assetsDir}/{fileName})"
            updatedContent <- updatedContent.Replace(m.Value, newLink)
        else
            printfn "⚠️ Warning: Image not found at %s" sourceImg
    )
    updatedContent


//need a function to add pre matters
let addheaders title content =
    let date = DateTime.Now.ToString("yyyy-MM-dd")
    let headers = $"""---
layout: post
title: {title}
author: xq
published: {date}
---

"""
    headers + content


// --- Main Execution ---
let args = fsi.CommandLineArgs |> Array.skip 1
if args.Length = 0 then
    printfn "Usage: dotnet fsi publish.fsx \"Note Name\""
else
    let noteName = args.[0]
    let sourceFile = Path.Combine(vaultPath, $"{noteName}.md")
    let fn = noteName.Split(Path.DirectorySeparatorChar) |> Array.last
    let webFileName = DateTime.Now.ToString("yyyy-MM-dd") + "-" + fn.Replace(" ", "-").ToLower() + ".md"
    let destFile = Path.Combine(repoPath, postsDir, webFileName)

    if not (File.Exists(sourceFile)) then
        printfn "❌ Note not found: %s" sourceFile
    else
        printfn "🚀 Publishing: %s" noteName

        // Read, Process Images, and Write
        let originalContent = File.ReadAllText(sourceFile)
        let paddedContent = originalContent |> addheaders noteName
        let finalContent = processImages paddedContent
        File.WriteAllText(destFile, finalContent)

        //clear the old dir docs and _publish
        let docsDir = Path.Combine(repoPath, "docs")
        let publishDir = Path.Combine(repoPath, "_public")

        // Clear old directories
        if Directory.Exists(docsDir) then Directory.Delete(docsDir, true)
        if Directory.Exists(publishDir) then Directory.Delete(publishDir, true)

        // Build with Fornax
        if execute "fornax" "build" repoPath = 0 then
            // Move _publish to docs
            if Directory.Exists(publishDir) then
                Directory.Move(publishDir, docsDir)
                printfn "✨ Build complete and published to docs/"
            else
                printfn "⚠️ Warning: _publish directory not found after build"
        else
            printfn "❌ Fornax build failed"


        // // Build and Deploy
        if execute "fornax" "build" repoPath = 0 then
            execute "git" "add ." repoPath |> ignore
            execute "git" ($"commit -m \"Publish: {noteName}\"") repoPath |> ignore
            //do a manual push for now, control the impact local in case of any issue.
            // execute "git" "push origin main" repoPath |> ignore
            printfn "✨ Done!"