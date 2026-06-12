Quickstart: Dotwright Ingest backend

cd src\Dotwright.Ingest
dotnet restore
dotnet run --urls "http://localhost:5000"

To ingest sample data (in a separate shell):
PowerShell: .\sample\ingest-sample.ps1

Schema located at src\Dotwright.Ingest\data\schema.sql
