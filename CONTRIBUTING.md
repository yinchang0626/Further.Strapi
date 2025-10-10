# Contributing to Further.Strapi

We love your input! We want to make contributing to Further.Strapi as easy and transparent as possible, whether it's:

- Reporting a bug
- Discussing the current state of the code
- Submitting a fix
- Proposing new features
- Becoming a maintainer

## Development Process

We use GitHub to host code, to track issues and feature requests, as well as accept pull requests.

## Pull Requests

1. Fork the repo and create your branch from `main`.
2. If you've added code that should be tested, add tests.
3. If you've changed APIs, update the documentation.
4. Ensure the test suite passes.
5. Make sure your code lints.
6. Issue that pull request!

## Development Setup

### Prerequisites

- .NET 9.0 SDK
- Visual Studio 2022 or VS Code
- Node.js (for test Strapi instance)

### Quick Setup

For a quick start, see the [Development Setup](README.md#quick-development-setup) section in the main README.

### Detailed Setup Steps

1. **Clone and build:**
   ```bash
   git clone https://github.com/your-org/Further.Strapi.git
   cd Further.Strapi
   dotnet restore && dotnet build
   ```

2. **Start test Strapi instance:**
   ```bash
   cd etc/eco-trace-strapi
   npm install
   npm run develop
   ```

3. **Configure test environment:**
   
   Create `appsettings.test.json` in `Further.Strapi.Tests/`:
   ```json
   {
     "StrapiOptions": {
       "StrapiUrl": "http://localhost:1337",
       "StrapiToken": "your-test-api-token"
     }
   }
   ```

4. **Verify setup:**
   ```bash
   # Run unit tests (fast)
   dotnet test --filter "Category!=StrapiRealIntegration"
   
   # Run integration tests (requires Strapi running)
   dotnet test --filter "Category=StrapiRealIntegration"
   ```

## Coding Standards

### Code Style

- Follow C# coding conventions
- Use meaningful variable and method names
- Add XML documentation for public APIs
- Keep methods focused and small

### Example:

```csharp
/// <summary>
/// Retrieves a collection of items with optional filtering and pagination.
/// </summary>
/// <typeparam name="T">The type of items to retrieve.</typeparam>
/// <param name="filter">Optional filter criteria.</param>
/// <param name="pagination">Optional pagination settings.</param>
/// <returns>A paged result containing the requested items.</returns>
public async Task<PagedResult<T>> GetListAsync<T>(
    FilterBuilder? filter = null,
    PaginationInput? pagination = null) where T : class
{
    // Implementation here
}
```

### Testing

- Write unit tests for all new functionality
- Include integration tests for API interactions
- Aim for high test coverage
- Use descriptive test names

```csharp
[Fact]
public async Task GetListAsync_WithValidFilter_ShouldReturnFilteredResults()
{
    // Arrange
    var filter = new FilterBuilder()
        .Where("title", FilterOperator.Contains, "test");
    
    // Act
    var result = await _provider.GetListAsync(filter: filter);
    
    // Assert
    Assert.NotNull(result);
    Assert.All(result.Data, item => 
        Assert.Contains("test", item.Title, StringComparison.OrdinalIgnoreCase));
}
```

## Issue Reporting

We use GitHub issues to track public bugs. Report a bug by [opening a new issue](https://github.com/your-org/Further.Strapi/issues/new); it's that easy!

### Bug Reports

**Great Bug Reports** tend to have:

- A quick summary and/or background
- Steps to reproduce
  - Be specific!
  - Give sample code if you can
- What you expected would happen
- What actually happens
- Notes (possibly including why you think this might be happening, or stuff you tried that didn't work)

### Feature Requests

We welcome feature requests! Please provide:

- Clear description of the feature
- Use case scenarios
- Expected behavior
- Any relevant examples or mockups

## License

By contributing, you agree that your contributions will be licensed under the MIT License.

## References

This document was adapted from the open-source contribution guidelines for [Facebook's Draft.js](https://github.com/facebook/draft-js/blob/master/CONTRIBUTING.md)