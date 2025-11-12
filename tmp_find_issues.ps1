$json = Get-Content -Raw -Path '.\tmp_orders.json' -ErrorAction Stop
$orders = ConvertFrom-Json $json
$problems = @()
foreach ($o in $orders) {
    $issues = @()
    if ($null -eq $o.FormaDePago) { $issues += 'FormaDePago=null' }
    if ($null -eq $o.Tercero) { $issues += 'Tercero=null' }
    # try parse Fecha
    $parsed = $false
    try { [datetime]::Parse($o.Fecha) | Out-Null; $parsed = $true } catch { $parsed = $false }
    if (-not $parsed) { $issues += 'Fecha invalid' }
    # check Guid
    if ($o.Guid -eq '00000000-0000-0000-0000-000000000000') { $issues += 'Guid empty' }
    if ($issues.Count -gt 0) {
        $problems += [PSCustomObject]@{ IdVentaLocal = $o.IdVentaLocal; Guid = $o.Guid; Issues = ($issues -join ', ') }
    }
}
$problems | Format-Table -AutoSize
$problems | ConvertTo-Json -Depth 5 | Out-File -FilePath '.\tmp_problems.json' -Encoding utf8
Write-Output "Done. Problems saved to tmp_problems.json"
