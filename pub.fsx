open System
open System.IO
open System.Text.RegularExpressions
open System.Diagnostics
let repoPath        = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "source/repo/xqguo.github.io")
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

printfn "🚀 Publishing"

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
    execute "git" ($"commit -m \"Publish\"") repoPath |> ignore
    //do a manual push for now, control the impact local in case of any issue.
    // execute "git" "push origin main" repoPath |> ignore
    printfn "✨ Done!"
