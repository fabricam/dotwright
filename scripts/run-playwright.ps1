# Run Playwright tests locally and ingest results into SQLite
Set-Location -Path (Join-Path $PSScriptRoot "..\tests\playwright")
npm install
npx playwright install --with-deps
# run tests and output JSON to repo root
npx playwright test --reporter=json > "..\..\playwright-test-results.json"
Set-Location -Path (Join-Path $PSScriptRoot "..\..")
python tools\ingest_sample\ingest.py --input playwright-test-results.json --db ingestion.db
Write-Output "Ingestion DB: $(Resolve-Path ingestion.db)"
