param (
    [string]$trxPath = ".\TestResults\test_results.trx",
    [string]$outputPath = ".\TestResults\TestReport.txt"
)


[xml]$trx = Get-Content -Raw $trxPath
$results = $trx.TestRun.Results.UnitTestResult
$definitions = @{}
$trx.TestRun.TestDefinitions.UnitTest | ForEach-Object {
    $definitions[$_.id] = @{
        Name = $_.TestMethod.name
        ClassName = $_.TestMethod.className
    }
}

$total = $results.Count
$passed = ($results | Where-Object outcome -eq "Passed").Count
$failed = ($results | Where-Object outcome -eq "Failed").Count
$skipped = ($results | Where-Object outcome -eq "NotExecuted").Count

$output = @()
$output += "Test Report Summary"
$output += "==================="
$output += "Total tests: $total"
$output += "Passed:      $passed"
$output += "Failed:      $failed"
$output += "Skipped:     $skipped"
$output += ""

$output += "Detailed Results:"
$output += "-----------------"

foreach ($result in $results) {
    $definition = $definitions[$result.testId]
    $statusIcon = switch ($result.outcome) {
        "Passed"      { "ok" }
        "Failed"      { "fail" }
        "NotExecuted" { "notexecuted" }
        default       { "unkown" }
    }

    $output += "$statusIcon $($definition.ClassName) > $($definition.Name): $($result.outcome)"
    
    if ($result.outcome -eq "Failed" -and $result.Output.ErrorInfo.Message) {
        $output += "    Reason: $($result.Output.ErrorInfo.Message.Trim())"
    }
}

$output | Out-File -Encoding UTF8 -FilePath $outputPath
Write-Host "Report written to: $outputPath"