# Firebase Hosting deploy script for NawafAppointment
$ErrorActionPreference = "Stop"
$root = Split-Path $PSScriptRoot -Parent

Write-Host "1/3 Publishing Blazor WASM..."
dotnet publish "$root\NawafAppointment.csproj" -c Release -o "$root\publish-hosting" -p:BlazorEnableCompression=false

Write-Host "2/3 Preparing hosting folder..."
$src = Join-Path $root "publish-hosting\wwwroot"
$dst = Join-Path $root "publish"
if (Test-Path $dst) { Remove-Item $dst -Recurse -Force }
New-Item -ItemType Directory -Path $dst | Out-Null
Copy-Item "$src\*" -Destination $dst -Recurse -Force
& (Join-Path $PSScriptRoot "fix-static-aliases.ps1") -Folder $dst

$wasm = (Get-ChildItem "$dst\_framework\*.wasm").Count
Write-Host "   $wasm wasm files ready"
if ($wasm -lt 10) { throw "Missing wasm! Run: dotnet workload install wasm-tools" }

Write-Host "3/3 Deploying to Firebase Hosting..."
Push-Location $root
firebase.cmd deploy --only hosting
Pop-Location

Write-Host "Done!"
