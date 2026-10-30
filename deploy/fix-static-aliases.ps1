# Creates logical filenames (blazor.webassembly.js, dotnet.js, etc.)
# Required for static hosts (Firebase, Netlify) that don't map hashed names like the dev server.
param(
    [string]$Folder = (Join-Path $PSScriptRoot "..\publish")
)

$framework = Join-Path $Folder "_framework"
if (-not (Test-Path $framework)) {
    throw "Missing folder: $framework"
}

function Copy-Alias($pattern, $aliasName, [scriptblock]$Filter = $null) {
    $files = Get-ChildItem $framework -Filter $pattern -File
    if ($Filter) { $files = $files | Where-Object $Filter }
    $file = $files | Select-Object -First 1
    if (-not $file) {
        Write-Warning "Not found: $pattern"
        return
    }
    $dest = Join-Path $framework $aliasName
    Copy-Item $file.FullName $dest -Force
    Write-Host "  $aliasName <- $($file.Name)"
}

Write-Host "Fixing _framework aliases in $framework"
Copy-Alias "blazor.webassembly.*.js" "blazor.webassembly.js"
Copy-Alias "dotnet.*.js" "dotnet.js" { $_.Name -notmatch 'native|runtime' }
Copy-Alias "dotnet.native.*.js" "dotnet.native.js"
Copy-Alias "dotnet.runtime.*.js" "dotnet.runtime.js"
Write-Host "Done."
