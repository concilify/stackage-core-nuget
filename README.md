# stackage-core-nuget

## Deployment Concerns

### Prerequisites

A secret named `NUGET_PUSH_TOKEN` containing a NuGet API key must have been added in order for GitHub Actions to be able to push NuGet packages

### Releasing

Tag the commit in the `main` branch that you wish to release with format `v*.*.*`. GitHub Actions will build this version and push the package to NuGet. Use format `v*.*.*-preview***` to build a pre-release NuGet package.
