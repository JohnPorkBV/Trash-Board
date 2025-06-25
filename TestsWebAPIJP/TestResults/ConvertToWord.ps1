param (
    [string]$inputPath = ".\TestResults\TestReport.txt",
    [string]$outputPath = ".\TestResults\TestReport.docx"
)

# Load the lines from the test report
$lines = Get-Content $inputPath

# Start Word
$word = New-Object -ComObject Word.Application
$word.Visible = $false
$doc = $word.Documents.Add()

# Add heading
$doc.Content.Text = "Automated Test Report`r`n"

# Insert summary section
$summaryLines = $lines | Select-String "^(Total tests|Passed|Failed|Skipped):" | ForEach-Object { $_.Line }
$summaryText = ($summaryLines -join "`r`n") + "`r`n`r`n"
$range = $doc.Range()
$range.InsertAfter($summaryText)

# Identify start of detailed results
$detailedStart = ($lines | Select-String "^Detailed Results:$").LineNumber
if (-not $detailedStart) {
    Write-Host "No detailed test section found."
    exit
}

$testLines = $lines[($detailedStart)..($lines.Count - 1)] | Where-Object { $_ -match "^(ok|fail|notexecuted|unknown) " }

# Create a table
$table = $doc.Tables.Add($doc.Paragraphs.Last.Range, $testLines.Count + 1, 4)
$table.Range.ParagraphFormat.SpaceAfter = 6
$table.Borders.Enable = $true

# Set header row
$table.Cell(1, 1).Range.Text = "Status"
$table.Cell(1, 2).Range.Text = "Service/Class"
$table.Cell(1, 3).Range.Text = "Test Name"
$table.Cell(1, 4).Range.Text = "Result"
$table.Rows.Item(1).Range.Bold = $true

# Fill the table
for ($i = 0; $i -lt $testLines.Count; $i++) {
    if ($testLines[$i] -match "^(?<status>\w+)\s+(?<class>[^>]+)\s*>\s*(?<name>[^:]+):\s*(?<outcome>.+)$") {
        $row = $i + 2
        $table.Cell($row, 1).Range.Text = $matches.status
        $table.Cell($row, 2).Range.Text = $matches.class
        $table.Cell($row, 3).Range.Text = $matches.name
        $table.Cell($row, 4).Range.Text = $matches.outcome
    }
}

# Save and close
$doc.SaveAs([ref]$outputPath)
$doc.Close()
$word.Quit()

Write-Host "Word test report saved to: $outputPath"
