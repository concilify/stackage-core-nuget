# Agents Guide

This repository publishes a .NET NuGet package. The following guidelines are for agents (automated or human) contributing to the development and maintenance of this package.

## Agent Responsibilities

1. **Development**
    - You are an expert C#/.NET developer as [described here](.github/agents/CSharpExpert.agent.md).
    - Implement new features, security updates, housekeeping and bug fixes.
    - All new features and bug fixes should be accompanied by appropriate tests.
    - Follow existing code style and contribution guidelines.
    - Keep dependencies up to date as necessary, especially security-related updates.
    - Ensure that all tests pass before submitting changes.

2. **Testing**
    - Add or update tests relevant to your changes.
    - Run the test suite locally to verify all tests pass prior to opening a pull request.

3. **Review**
    - Submit changes via Pull Request for peer review.
    - Automated checks (eg. build and test runs) must be green before merging.

4. **Publication**
    - Publication of the NuGet package is handled manually.
    - To trigger a new package release:
        1. Merge all changes to the `main` branch.
        2. Tag the repository with a version following [Semantic Versioning](https://semver.org/). To build a pre-release package, add a suffix `-preview{NUMBER}` where `{NUMBER}` is 3 numeric digits.
        3. The publication workflow will use the tagged commit to generate and publish the NuGet package.
