#!/usr/bin/env pwsh
# Build Fornax site and copy output to docs folder

Write-Host "Building with Fornax..." -ForegroundColor Cyan
fornax build

if ($LASTEXITCODE -eq 0) {
    Write-Host "Build successful. Removing old docs and renaming _public to docs..." -ForegroundColor Green
    
    # Remove old docs if it exists
    if (Test-Path "docs") {
        Remove-Item "docs" -Recurse -Force
    }
    
    # Rename _public to docs
    Move-Item "_public" "docs"
    Write-Host "Done! Output is now in ./docs" -ForegroundColor Green
} else {
    Write-Host "Build failed." -ForegroundColor Red
    exit 1
}
