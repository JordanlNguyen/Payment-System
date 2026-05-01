$ErrorActionPreference = 'Stop'

$scriptDirectory = Split-Path -Parent $MyInvocation.MyCommand.Path
$sqlFile = Join-Path $scriptDirectory 'setup-databases.sql'

$dbHost = if ($env:DB_HOST) { $env:DB_HOST } else { 'localhost' }
$dbPort = if ($env:DB_PORT) { $env:DB_PORT } else { '5432' }
$dbUsername = if ($env:DB_USERNAME) { $env:DB_USERNAME } else { 'postgres' }
$dbPassword = $env:DB_PASSWORD
$dbAdminDatabase = if ($env:DB_ADMIN_DATABASE) { $env:DB_ADMIN_DATABASE } else { 'postgres' }

if (-not (Get-Command psql -ErrorAction SilentlyContinue)) {
    throw 'psql was not found. Install PostgreSQL and ensure psql is on PATH before running this script.'
}

if ([string]::IsNullOrWhiteSpace($dbPassword)) {
    $securePassword = Read-Host "PostgreSQL password for $dbUsername" -AsSecureString
    $marshal = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($securePassword)
    try {
        $dbPassword = [Runtime.InteropServices.Marshal]::PtrToStringBSTR($marshal)
    }
    finally {
        [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($marshal)
    }
}

$env:PGPASSWORD = $dbPassword
try {
    & psql --host $dbHost --port $dbPort --username $dbUsername --dbname $dbAdminDatabase --file $sqlFile
}
finally {
    Remove-Item Env:PGPASSWORD -ErrorAction SilentlyContinue
}

Write-Host 'Databases are ready.'