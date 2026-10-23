# The Living Structures - Windows x64 C11-to-WASM toolchain installation.
# Downloads pinned official archives, verifies SHA-256, tests Zig + Wasmtime,
# then moves the tested folder into the Unity project. No global PATH changes.
# Run manually, or via Tools > The Living Structures > Install or Verify C11 WASM Tools.
[CmdletBinding()]
param([string]$ProjectRoot)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

if ($PSVersionTable.PSEdition -eq 'Core' -and -not $IsWindows) {
    throw 'This installer supports Windows x64 only.'
}
if (-not [Environment]::Is64BitOperatingSystem) {
    throw 'This installer requires a 64-bit Windows installation.'
}

# If run directly from an extracted SETUP folder, find the containing Unity
# project. The Editor menu always supplies an explicit project root.
if ([string]::IsNullOrWhiteSpace($ProjectRoot)) {
    $oneUp = Split-Path -Parent $PSScriptRoot
    $twoUp = Split-Path -Parent $oneUp
    foreach ($candidate in @($oneUp, $twoUp)) {
        if (Test-Path -LiteralPath (Join-Path $candidate 'Assets\TheLivingStructures')) {
            $ProjectRoot = $candidate
            break
        }
    }
    if ([string]::IsNullOrWhiteSpace($ProjectRoot)) {
        throw 'Cannot locate the Unity project. Specify -ProjectRoot with the folder containing Assets and ProjectSettings.'
    }
}
$project = (Resolve-Path -LiteralPath $ProjectRoot).Path
if (-not (Test-Path -LiteralPath (Join-Path $project 'Assets\TheLivingStructures')) -or
    -not (Test-Path -LiteralPath (Join-Path $project 'Packages')) -or
    -not (Test-Path -LiteralPath (Join-Path $project 'ProjectSettings'))) {
    throw 'ProjectRoot must be the Unity project folder containing Assets\TheLivingStructures, Packages, and ProjectSettings.'
}
$dest = Join-Path $project 'TLS_Toolchain_Windows'
$tmp = Join-Path ([System.IO.Path]::GetTempPath()) ('tls-tools-' + [guid]::NewGuid().ToString('N'))
$stage = Join-Path $project ('TLS_Toolchain_Windows.setup-' + [guid]::NewGuid().ToString('N'))
$zigUrl = 'https://ziglang.org/download/0.17.0/zig-x86_64-windows-0.17.0.zip'
$zigDigest = 'b5663f69581dcf391293fbf16c06cb80d81d806545ce618b4d0bab7f0eb8c428'
# Pin the official immutable release asset and the SHA-256 shown on its GitHub release page.
# The ordinary release download does not use GitHub's REST API rate-limited metadata endpoint.
$wasmtimeUrl = 'https://github.com/bytecodealliance/wasmtime/releases/download/v50.0.0/wasmtime-v50.0.0-x86_64-windows.zip'
$wasmtimeDigest = 'cea9ccc700aaccb2b5507e96ac990ba60104bd7ab39284fb2e67cb64d58efcc4'

New-Item -ItemType Directory -Path $tmp | Out-Null
try {
    if (Test-Path -LiteralPath $dest) {
        $toolDir = $dest
        Write-Host 'Found existing project-local tools. Rechecking their C-to-WASM smoke test; no download or replacement.' -ForegroundColor Cyan
    }
    else {
        $headers = @{ 'User-Agent' = 'TLS-Unity-Educational-Toolchain' }
        Write-Host 'Using the pinned official Wasmtime v50.0.0 Windows x64 release archive (no GitHub API needed).'
        $zigZip = Join-Path $tmp 'zig.zip'
        $wasmtimeZip = Join-Path $tmp 'wasmtime.zip'
        Write-Host 'Downloading Zig 0.17.0 (~100 MB)...'
        Invoke-WebRequest -Uri $zigUrl -OutFile $zigZip
        Write-Host 'Downloading Wasmtime v50.0.0...'
        Invoke-WebRequest -Uri $wasmtimeUrl -Headers $headers -OutFile $wasmtimeZip
        foreach ($pair in @(@($zigZip, $zigDigest), @($wasmtimeZip, $wasmtimeDigest))) {
            $actual = (Get-FileHash -Algorithm SHA256 -LiteralPath $pair[0]).Hash.ToLowerInvariant()
            if ($actual -ne $pair[1]) { throw "SHA-256 mismatch for $($pair[0]). Refusing unverified software." }
        }
        Write-Host 'Both official archives passed SHA-256 verification.' -ForegroundColor Green
        $zigExtract = Join-Path $tmp 'zig-extract'
        $wasmtimeExtract = Join-Path $tmp 'wasmtime-extract'
        Expand-Archive -LiteralPath $zigZip -DestinationPath $zigExtract
        Expand-Archive -LiteralPath $wasmtimeZip -DestinationPath $wasmtimeExtract
        $zigBin = Get-ChildItem -LiteralPath $zigExtract -Recurse -File -Filter zig.exe | Select-Object -First 1
        $runBin = Get-ChildItem -LiteralPath $wasmtimeExtract -Recurse -File -Filter wasmtime.exe | Select-Object -First 1
        if ($null -eq $zigBin -or -not (Test-Path -LiteralPath (Join-Path $zigBin.Directory.FullName 'lib'))) {
            throw 'The official Zig ZIP lacks zig.exe beside its lib directory.'
        }
        if ($null -eq $runBin) { throw 'The official Wasmtime ZIP lacks wasmtime.exe.' }
        New-Item -ItemType Directory -Path $stage | Out-Null
        Copy-Item -LiteralPath $zigBin.Directory.FullName -Destination (Join-Path $stage 'zig') -Recurse
        Copy-Item -LiteralPath $runBin.Directory.FullName -Destination (Join-Path $stage 'wasmtime') -Recurse
        $toolDir = $stage
    }

    $zigExe = Join-Path $toolDir 'zig\zig.exe'
    $runExe = Join-Path $toolDir 'wasmtime\wasmtime.exe'
    if (-not (Test-Path -LiteralPath $zigExe) -or -not (Test-Path -LiteralPath $runExe)) {
        throw 'Compiler/runtime files missing. No incomplete toolchain will be accepted.'
    }
    Write-Host 'Zig version:'
    & $zigExe version
    if ($LASTEXITCODE -ne 0) { throw 'Zig could not launch.' }
    Write-Host 'Wasmtime version:'
    & $runExe --version
    if ($LASTEXITCODE -ne 0) { throw 'Wasmtime could not launch.' }

    $smokeC = Join-Path $tmp 'tls-wasm-smoke.c'
    $smokeWasm = Join-Path $tmp 'tls-wasm-smoke.wasm'
    [System.IO.File]::WriteAllText($smokeC, 'int main(void) { return 0; }')
    # PowerShell interprets commas in unquoted native arguments as parameter
    # separators. Send each argument as one literal string using an array.
    $compileArgs = @(
        'cc', '-target', 'wasm32-wasi', '-std=c11', '-O0',
        '-Wall', '-Wextra', '-Werror', '-pedantic',
        '-Wl,-z,stack-size=262144', '-Wl,--max-memory=16777216',
        $smokeC, '-o', $smokeWasm
    )
    Write-Host 'Checking real C11-to-WASI compilation...'
    & $zigExe @compileArgs
    if ($LASTEXITCODE -ne 0 -or -not (Test-Path -LiteralPath $smokeWasm)) {
        throw 'Zig C11-to-WebAssembly smoke compilation failed; read the output above.'
    }
    $runArgs = @(
        'run', '-W', 'fuel=20000000,max-memory-size=16777216,max-wasm-stack=262144,timeout=2s',
        $smokeWasm
    )
    Write-Host 'Checking resource-limited Wasmtime execution...'
    & $runExe @runArgs
    if ($LASTEXITCODE -ne 0) { throw 'The restricted Wasmtime smoke execution failed; read the output above.' }

    if ($toolDir -eq $stage) {
        Move-Item -LiteralPath $stage -Destination $dest
    }
    Write-Host ''
    Write-Host 'SUCCESS: Zig C11 -> WASM and Wasmtime restricted run both passed.' -ForegroundColor Green
    Write-Host "Toolchain ready at: $dest" -ForegroundColor Green
    Write-Host 'Nothing was added to the system PATH. You may return to Unity and run the WASM C11 tests.'
}
finally {
    if (Test-Path -LiteralPath $tmp) { Remove-Item -LiteralPath $tmp -Recurse -Force }
    if (Test-Path -LiteralPath $stage) { Remove-Item -LiteralPath $stage -Recurse -Force }
}
