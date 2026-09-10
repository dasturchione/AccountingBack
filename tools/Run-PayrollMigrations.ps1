[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$ConnectionString,
    [string]$FromScript = "16_pay/1633_create_pay_migration_history.sql",
    [string]$ToScript,
    [switch]$DryRun
)

$ErrorActionPreference = "Stop"
$psql = (Get-Command psql -ErrorAction Stop).Source
$scriptsRoot = Join-Path $PSScriptRoot "..\src\Infrastructure\Persistence\Scripts"
$orderFile = Join-Path $scriptsRoot "_run_order.txt"

# The application uses an ADO.NET connection string while psql expects
# connection options. Parse it without ever echoing the password.
$connectionParts = @{}
foreach ($part in ($ConnectionString -split ';')) {
    $separator = $part.IndexOf('=')
    if ($separator -gt 0) {
        $key = $part.Substring(0, $separator).Trim()
        $value = $part.Substring($separator + 1).Trim().Trim('"')
        $connectionParts[$key] = $value
    }
}

function Get-ConnectionValue {
    param([string[]]$Keys)

    foreach ($key in $Keys) {
        if ($connectionParts.ContainsKey($key)) {
            return [string]$connectionParts[$key]
        }
    }

    return $null
}

$dbHost = Get-ConnectionValue @('Host', 'Server', 'Data Source')
$dbPort = Get-ConnectionValue @('Port')
$dbName = Get-ConnectionValue @('Database', 'Initial Catalog')
$dbUser = Get-ConnectionValue @('Username', 'User ID', 'User')
$dbPassword = Get-ConnectionValue @('Password', 'Pwd')

$psqlConnectionArgs = @()
if ($dbHost) { $psqlConnectionArgs += "--host=$dbHost" }
if ($dbPort) { $psqlConnectionArgs += "--port=$dbPort" }
if ($dbName) { $psqlConnectionArgs += "--dbname=$dbName" }
if ($dbUser) { $psqlConnectionArgs += "--username=$dbUser" }
if ($dbPassword) { $env:PGPASSWORD = $dbPassword }

if (-not $dbName) {
    throw "Connection string must contain Database (or Initial Catalog)."
}

function Invoke-Psql {
    param([Parameter(Mandatory = $true)][string[]]$Arguments)

    $result = & $psql @psqlConnectionArgs @Arguments
    $exitCode = $LASTEXITCODE
    if ($exitCode -ne 0) {
        throw "psql failed with exit code $exitCode."
    }

    return $result
}

$bootstrap = @"
create table if not exists pay_migration_history
(
    id bigserial primary key,
    script_name varchar(300) not null,
    checksum_sha256 char(64) not null,
    applied_at_utc timestamp with time zone not null default now(),
    constraint ux_pay_migration_history_script unique (script_name)
);
"@
Invoke-Psql -Arguments @('--set', 'ON_ERROR_STOP=1', '--command', $bootstrap) | Out-Host

$entries = Get-Content $orderFile | Where-Object {
    $value = $_.Trim()
    $value -and -not $value.StartsWith("#")
}

if ($entries -notcontains $FromScript) {
    throw "Migration entry does not exist in _run_order.txt: $FromScript"
}
if ($ToScript -and $entries -notcontains $ToScript) {
    throw "Migration entry does not exist in _run_order.txt: $ToScript"
}

$started = [string]::IsNullOrWhiteSpace($FromScript)

foreach ($entry in $entries) {
    if (-not $started) {
        if ($entry -eq $FromScript) { $started = $true } else { continue }
    }
    $relativePath = $entry.Replace('/', '\')
    $file = Join-Path $scriptsRoot $relativePath
    if (-not (Test-Path -LiteralPath $file -PathType Leaf)) {
        throw "Migration file does not exist: $entry"
    }

    $checksum = (Get-FileHash -LiteralPath $file -Algorithm SHA256).Hash.ToLowerInvariant()
    $safeName = $entry.Replace("'", "''")
    $existingRows = @(Invoke-Psql -Arguments @('--tuples-only', '--no-align', '--set', 'ON_ERROR_STOP=1', '--command', "select checksum_sha256 from pay_migration_history where script_name = '$safeName';"))
    $existing = if ($existingRows.Count -eq 0) { '' } else { ($existingRows -join "`n").Trim() }
    if ($existing) {
        if ($existing -ne $checksum) {
            throw "Migration checksum mismatch for $entry. Existing=$existing Current=$checksum"
        }
        Write-Host "SKIP  $entry"
        if ($ToScript -and $entry -eq $ToScript) {
            break
        }
        continue
    }

    if ($DryRun) {
        Write-Host "APPLY $entry ($checksum)"
        continue
    }

    Write-Host "APPLY $entry"
    Invoke-Psql -Arguments @('--set', 'ON_ERROR_STOP=1', '--file', $file) | Out-Host
    Invoke-Psql -Arguments @('--set', 'ON_ERROR_STOP=1', '--command', "insert into pay_migration_history(script_name, checksum_sha256) values ('$safeName', '$checksum');") | Out-Host

    if ($ToScript -and $entry -eq $ToScript) {
        break
    }
}

Write-Host "Payroll migration manifest is consistent."
