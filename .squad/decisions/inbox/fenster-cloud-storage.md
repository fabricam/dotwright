# Add cloud storage provider detection for PlaywrightReportReader

Decision: Implement URI-based cloud storage detection and reading for Azure Blob Storage, AWS S3, and Google Cloud Storage in the PlaywrightReportReader.

Why:
- Users requested that the report reader automatically detect and read from cloud storage when given a cloud URI (e.g., Azure blob, S3, GCS paths).
- Centralizes cloud storage logic behind an abstraction (ICloudStorageProvider) for extensibility.
- FromUriAsync pattern enables readers to transparently handle cloud URIs without downstream code changes.

What changed:
- Added ICloudStorageProvider abstraction and three implementations: AzureBlobCloudStorageProvider, AwsS3CloudStorageProvider, GoogleCloudStorageProvider.
- Added CloudStorageProviderResolver to detect provider type from URI and instantiate the correct provider.
- Extended PlaywrightReportReader with FromUriAsync method for cloud URI support.
- Updated Dotwright.Playwright.csproj with necessary cloud SDK dependencies.
- Updated PACKAGING.md documentation.

Branch: squad/add-cloud-storage-reader

See branch/PR for implementation details and tests.
