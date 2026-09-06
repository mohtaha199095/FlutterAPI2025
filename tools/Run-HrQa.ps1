# Runs HR QA against a running WebApplication2 API instance.
# Usage:
#   .\tools\Run-HrQa.ps1
#   .\tools\Run-HrQa.ps1 -BaseUrl "http://localhost:5000" -CompanyId 1

param(
    [string]$BaseUrl = "http://localhost:5000",
    [int]$CompanyId = 1,
    [switch]$JournalOnly,
    [switch]$GoldenOnly,
    [switch]$NoDatabaseScan
)

function Invoke-QaEndpoint {
    param([string]$Path)
    try {
        return Invoke-RestMethod -Uri "$BaseUrl$Path" -Method Get -TimeoutSec 120
    } catch {
        Write-Host "FAILED: $Path" -ForegroundColor Red
        Write-Host $_.Exception.Message
        return $null
    }
}

function Show-Report {
    param($Report, [string]$Title)
    if ($null -eq $Report) { return }
    Write-Host ""
    Write-Host "=== $Title ===" -ForegroundColor Cyan
    $allPassed = $Report.allPassed
    if ($allPassed) {
        Write-Host "ALL PASSED ($($Report.passedChecks)/$($Report.totalChecks))" -ForegroundColor Green
    } else {
        Write-Host "FAILURES ($($Report.failedChecks)/$($Report.totalChecks) failed)" -ForegroundColor Red
    }
    foreach ($r in $Report.results) {
        $color = if ($r.passed) { "Green" } else { "Red" }
        $cat = if ($r.category) { "[$($r.category)] " } else { "" }
        Write-Host ("  {0}{1}: {2}" -f $cat, $r.name, $(if ($r.passed) { "PASS" } else { "FAIL" })) -ForegroundColor $color
        if (-not $r.passed -and $r.detail) {
            Write-Host "    $($r.detail)" -ForegroundColor DarkYellow
        }
    }
}

Write-Host "HR QA runner -> $BaseUrl (CompanyId=$CompanyId)" -ForegroundColor Cyan

if ($GoldenOnly) {
    $golden = Invoke-QaEndpoint "/api/ctlPayrollPreview/RunGoldenFixtures"
    Show-Report $golden "Payroll Golden Fixtures"
    exit $(if ($golden -and $golden.allPassed) { 0 } else { 1 })
}

if ($JournalOnly) {
    $journal = Invoke-QaEndpoint "/api/ctlHrReports/RunJournalQa?CompanyID=$CompanyId"
    Show-Report $journal "Journal QA"
    exit $(if ($journal -and $journal.allPassed) { 0 } else { 1 })
}

$scan = if ($NoDatabaseScan) { "false" } else { "true" }
$full = Invoke-QaEndpoint "/api/ctlHrReports/RunHrQa?CompanyID=$CompanyId&ScanDatabase=$scan"
Show-Report $full "Full HR QA"

exit $(if ($full -and $full.allPassed) { 0 } else { 1 })
