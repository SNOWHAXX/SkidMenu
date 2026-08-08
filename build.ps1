$ErrorActionPreference = 'Stop'
$src = Join-Path $PSScriptRoot 'src'
$built = Join-Path $src 'bin\Release\net6.0\SkidMenu.dll'
$target = 'C:\Program Files (x86)\Steam\steamapps\common\Among Us\BepInEx\plugins\SkidMenu.dll'

function Fail([string]$msg) { Write-Host "[FAIL] $msg" -ForegroundColor Red; exit 1 }
function Step([string]$msg) { Write-Host $msg -ForegroundColor Cyan }

Step '[1/3] nuking bin/obj'
Remove-Item (Join-Path $src 'bin') -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item (Join-Path $src 'obj') -Recurse -Force -ErrorAction SilentlyContinue

Step '[2/3] building Release'
dotnet build (Join-Path $src 'SkidMenu.csproj') -c Release --nologo -v q
if ($LASTEXITCODE -ne 0) { Fail 'build failed' }
if (-not (Test-Path -LiteralPath $built)) { Fail "built DLL not found: $built" }

Step '[3/3] deploying to plugins'
try { Copy-Item -LiteralPath $built -Destination $target -Force }
catch { Fail 'copy failed - is Among Us still running?' }
if (-not (Test-Path -LiteralPath $target)) { Fail 'deployed DLL missing' }

Write-Host 'BUILD OK' -ForegroundColor Green
