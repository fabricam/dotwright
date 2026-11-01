Orchestration: Add cloud storage provider detection and reading

Branch: squad/add-cloud-storage-reader
Implementation: fenster

Files created:
- src/Dotwright.Playwright/Services/CloudProviders/ICloudStorageProvider.cs
- src/Dotwright.Playwright/Services/CloudProviders/AzureBlobCloudStorageProvider.cs
- src/Dotwright.Playwright/Services/CloudProviders/AwsS3CloudStorageProvider.cs
- src/Dotwright.Playwright/Services/CloudProviders/GoogleCloudStorageProvider.cs
- src/Dotwright.Playwright/Services/CloudStorageProviderResolver.cs

Files edited:
- src/Dotwright.Playwright/PlaywrightReportingServiceCollectionExtensions.cs (added FromUriAsync registration)
- src/Dotwright.Playwright/Services/PlaywrightReportReader.cs (added FromUriAsync method)
- src/Dotwright.Playwright/Dotwright.Playwright.csproj (added Azure.Storage.Blobs, Google.Cloud.Storage.V1, AWSSDK.S3 dependencies)
- PACKAGING.md (updated documentation with cloud storage support)

Tests & build:
- Cloud storage providers: abstraction handles Azure, AWS, GCS detection and reading
- PlaywrightReportReader.FromUriAsync: URI parsing and appropriate provider selection
- Integration tested with existing test suite

Commit and push:
- Commit message: "Add cloud storage providers and URI-based reader (S3, Azure Blob, GCS); FromUriAsync; update CSProj"
- Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>
- Branch pushed: squad/add-cloud-storage-reader

Notes:
- Extensible architecture allows future cloud providers to be added by implementing ICloudStorageProvider.
- Ready for PR creation when reviewers are available.
