$url = 'http://localhost:5000/ingest'
$payload = Get-Content -Raw -Path './sample_payload.json'
Invoke-RestMethod -Uri $url -Method Post -ContentType 'application/json' -Body $payload
Write-Output 'Posted sample payload to ' + $url
