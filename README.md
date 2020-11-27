# core

## Deployment Concerns

### Prerequisites

A NuGet API key must have been added to Stackage / Project settings / Service connections in order for DevOps to be able to push NuGet packages

### Releasing

Tag the commit in the `master` branch that you wish to release with format `v*.*.*`. DevOps will build this version and push the package to NuGet. Use format `v*.*.*-preview***` to build a pre-release NuGet package.
