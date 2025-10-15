# Further.Strapi

[![Integration Tests](https://github.com/yinchang0626/Further.Strapi/actions/workflows/integration-tests.yml/badge.svg)](https://github.com/yinchang0626/Further.Strapi/actions/workflows/integration-tests.yml)
[![codecov](https://codecov.io/gh/yinchang0626/Further.Strapi/branch/main/graph/badge.svg)](https://codecov.io/gh/yinchang0626/Further.Strapi)
[![.NET](https://img.shields.io/badge/.NET-8.0%20%7C%209.0-blue.svg?logo=dotnet)](https://dotnet.microsoft.com/)
[![ABP Framework](https://img.shields.io/badge/ABP%20Framework-8.3.0%20%7C%209.3.5-green.svg)](https://abp.io/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg?logo=opensource)](LICENSE)

[![Coverage Tree](https://codecov.io/gh/yinchang0626/Further.Strapi/branch/main/graphs/tree.svg)](https://codecov.io/gh/yinchang0626/Further.Strapi)

A comprehensive .NET integration library for [Strapi v5](https://strapi.io/) headless CMS, built on top of the [ABP Framework](https://abp.io/).

## 🚀 Features

- **🔗 Complete Strapi v5 API Integration** - Collection Types, Single Types, and Components
- **📁 Media Library Support** - Upload, manage, and organize media files
- **🧩 Polymorphic Components** - Handle complex content structures with dynamic zones
- **🔍 Advanced Querying** - Filtering, sorting, and population with fluent Action-based API
- **🛡️ Type-Safe Operations** - Strongly-typed C# interfaces for all Strapi operations
- **✨ Unified API Design** - Consistent Action-based configuration across all query builders

## 🚀 Quick Start

### 1. Install the Package

```bash
dotnet add package Further.Strapi
```

### 2. Configure Services

#### Basic Configuration

```csharp
[DependsOn(typeof(StrapiModule))]
public class YourAppModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<StrapiOptions>(options =>
        {
            options.StrapiUrl = "http://localhost:1337";
            options.StrapiToken = "your-api-token";
        });
    }
}
```

#### Advanced Configuration

```csharp
[DependsOn(typeof(StrapiModule))]
public class YourAppModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // Method 1: Use appsettings.json
        // No additional configuration needed - reads from "Strapi" section

        // Method 2: Override with code
        context.Services.AddStrapi(builder =>
        {
            builder.ConfigureOptions(options =>
            {
                options.StrapiUrl = "http://production-strapi.example.com";
                options.StrapiToken = "production-token";
            });

            // Customize HttpClient behavior
            builder.ConfigureHttpClient((serviceProvider, client) =>
            {
                client.Timeout = TimeSpan.FromSeconds(60);
                client.DefaultRequestHeaders.Add("X-Custom-Header", "MyApp");
            });
        });
    }
}
```

#### Configuration with appsettings.json

```json
{
  "Strapi": {
    "StrapiUrl": "http://localhost:1337",
    "StrapiToken": "your-api-token"
  }
}
```

### 3. Work with Collection Types

Collection Types handle multiple content items of the same type (Articles, Products, Users).

```csharp
public class ArticleService : ApplicationService
{
    private readonly ICollectionTypeProvider<Article> _articleProvider;

    public ArticleService(ICollectionTypeProvider<Article> articleProvider)
    {
        _articleProvider = articleProvider;
    }

    public async Task<List<Article>> GetPublishedArticlesAsync()
    {
        return await _articleProvider.GetListAsync(
            filter: f => f
                .Where("title", FilterOperator.ContainsInsensitive, "技術")
                .And("publishedAt", FilterOperator.NotNull),
            populate: p => p
                .Include("author")
                .Include("blocks"),
            sort: s => s.Descending("publishedAt"),
            pagination: p => p.Page(1, 10)
        );
    }

    public async Task<string> CreateArticleAsync(Article article)
    {
        return await _articleProvider.CreateAsync(article);
    }

    public async Task<Article> GetArticleAsync(string documentId)
    {
        return await _articleProvider.GetAsync(documentId);
    }
}
```

### 4. Work with Single Types

Single Types handle global or unique content (Global Settings, Homepage, About Us).

```csharp
public class GlobalSettingsService : ApplicationService
{
    private readonly ISingleTypeProvider<GlobalSettings> _globalProvider;

    public async Task<GlobalSettings> GetGlobalSettingsAsync()
    {
        return await _globalProvider.GetAsync();
    }

    public async Task UpdateGlobalSettingsAsync(GlobalSettings settings)
    {
        await _globalProvider.UpdateAsync(settings);
    }
}
```

### 5. Work with Media Library

```csharp
public class MediaService : ApplicationService
{
    private readonly IMediaLibraryProvider _mediaProvider;

    public async Task<StrapiMediaField> UploadFileAsync(IFormFile file)
    {
        var fileUpload = new FileUploadRequest
        {
            Files = new[] { file },
            FileInfo = new[]
            {
                new FileInfoRequest
                {
                    AlternativeText = "Uploaded image",
                    Caption = "Image caption"
                }
            }
        };

        var results = await _mediaProvider.UploadAsync(fileUpload);
        return results.FirstOrDefault();
    }

    public async Task<List<StrapiMediaField>> GetMediaLibraryAsync()
    {
        return await _mediaProvider.GetLibraryAsync();
    }

    public async Task<StrapiMediaField> UpdateFileInfoAsync(int fileId, FileInfoRequest fileInfo)
    {
        return await _mediaProvider.UpdateFileInfoAsync(fileId, fileInfo);
    }
}
```

## 🧪 Development & Testing

### For Local Testing

1. **Start test Strapi:**
   ```bash
   cd etc/strapi-integration-test
   npm install
   npm run develop
   ```

2. **Run tests:**
   ```bash
   # Fast tests only
   dotnet test --filter "Category!=StrapiRealIntegration"
   
   # All tests (requires Strapi running)
   dotnet test
   ```

### GitHub Actions Integration Tests

- **Manual trigger**: Use GitHub Actions UI and enable "Run integration tests"
- **Commit trigger**: Include `[integration]` in commit message
- **Automatic**: Integration tests run automatically in CI when triggered

## 🔍 Advanced Usage

### Complex Filtering

```csharp
var articles = await _articleProvider.GetListAsync(
    filter: f => f
        .Where("title", FilterOperator.ContainsInsensitive, "technology")
        .And(nested => nested
            .Where("author.name", FilterOperator.Equals, "John Doe")
            .Or("author.email", FilterOperator.Contains, "@company.com")
        )
        .And("publishedAt", FilterOperator.Between, startDate, endDate),
    sort: s => s
        .Descending("publishedAt")
        .Ascending("title"),
    populate: p => p
        .Include("author")
        .Include("categories")
        .Include("dynamicZone"),
    pagination: p => p
        .Page(1, 20)
        .WithCount(true)
);
```

### Polymorphic Component Handling

```csharp
[StrapiComponentName("shared.rich-text")]
public class RichTextComponent : IStrapiComponent
{
    public string Body { get; set; } = string.Empty;
    // __component is automatically handled by the polymorphic system
}

[StrapiComponentName("shared.media")]
public class MediaComponent : IStrapiComponent
{
    public StrapiMediaField? File { get; set; }
    // __component is automatically handled by the polymorphic system
}

// Components are automatically handled based on polymorphic serialization
var article = await _articleProvider.GetAsync(documentId, 
    populate: p => p.Include("dynamicZone"));

foreach (var component in article.DynamicZone)
{
    // Polymorphic deserialization is handled automatically
    // Type checking can be done using pattern matching
    switch (component)
    {
        case RichTextComponent richText:
            Console.WriteLine($"Rich text: {richText.Body}");
            break;
        case MediaComponent media:
            Console.WriteLine($"Media file: {media.File.Url}");
            break;
    }
        case nameof(RichTextComponent):
            var richText = component as RichTextComponent;
            Console.WriteLine($"Rich Text: {richText?.Body}");
            break;
        case nameof(MediaComponent):
            var media = component as MediaComponent;
            Console.WriteLine($"Media: {media?.File?.Name}");
            break;
    }
}
```

### Multiple Pagination Modes

```csharp
// Traditional page-based pagination
var pagedArticles = await _articleProvider.GetListAsync(
    pagination: p => p.Page(1, 10)
);

// Offset-based pagination with performance optimization
var offsetArticles = await _articleProvider.GetListAsync(
    pagination: p => p
        .StartLimit(0, 20)
        .WithCount(false) // Skip total count for better performance
);
```

## 🏗️ Architecture

### Project Structure

```
Further.Strapi/
├── Further.Strapi/                 # Core implementation
│   ├── CollectionTypeProvider.cs   # Collection type operations
│   ├── SingleTypeProvider.cs       # Single type operations  
│   ├── MediaLibraryProvider.cs     # Media library management
│   ├── StrapiProtocol.cs          # HTTP protocol handling
│   └── StrapiWriteSerializer.cs    # Smart serialization
├── Further.Strapi.Shared/          # Shared utilities and interfaces
│   ├── IStrapiComponent.cs        # Component interface
│   ├── StrapiMediaField.cs        # Media field types
│   ├── StrapiAttributes.cs        # Component name attributes
│   └── Components/                # Shared component definitions
└── Further.Strapi.Tests/           # Comprehensive test suite
    ├── Integration/                # Integration tests
    ├── Unit/                      # Unit tests
    └── Components/                # Component-specific tests
```

### Key Components

- **Providers**: High-level service abstractions for different Strapi content types
- **Protocol Layer**: Low-level HTTP communication with Strapi API
- **Serialization**: Intelligent handling of Strapi-specific data structures
- **Query Builders**: Fluent API for building complex queries

## 🤝 Contributing

We welcome contributions! Please see our [Contributing Guide](CONTRIBUTING.md) for detailed information.

### Quick Development Setup

1. **Clone and build:**
   ```bash
   git clone https://github.com/yinchang0626/Further.Strapi.git
   cd Further.Strapi
   dotnet restore && dotnet build
   ```

2. **Start test Strapi:**
   ```bash
   cd etc/strapi-integration-test
   npm install && npm run develop
   ```

3. **Configure test settings:**
   Create `appsettings.test.json` in the test project:
   ```json
   {
     "StrapiOptions": {
       "StrapiUrl": "http://localhost:1337",
       "StrapiToken": "your-test-api-token"
     }
   }
   ```

4. **Run tests:**
   ```bash
   dotnet test
   ```

### Contribution Guidelines

- 🐛 **Bug Reports**: [Open an issue](https://github.com/your-org/Further.Strapi/issues/new) with reproduction steps
- ✨ **Feature Requests**: Describe use cases and expected behavior
- 🔧 **Pull Requests**: Fork → Code → Test → Document → Submit
- 📝 **Code Style**: Follow C# conventions, add XML docs for public APIs
- 🧪 **Testing**: Write unit tests for new functionality, include integration tests for API interactions

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 📞 Support

- 📖 [Documentation](https://github.com/yinchang0626/Further.Strapi/wiki)
- 🐛 [Issue Tracker](https://github.com/yinchang0626/Further.Strapi/issues)
- 💬 [Discussions](https://github.com/yinchang0626/Further.Strapi/discussions)