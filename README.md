# Further.Strapi

[![.NET](https://img.shields.io/badge/.NET-9.0-blue.svg)](https://dotnet.microsoft.com/)
[![ABP Framework](https://img.shields.io/badge/ABP%20Framework-9.3.5-green.svg)](https://abp.io/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

A comprehensive .NET integration library for [Strapi v5](https://strapi.io/) headless CMS, built on top of the [ABP Framework](https://abp.io/). This library provides a complete set of tools for seamless integration with Strapi, including advanced features like polymorphic components, dynamic zones, and media library management.

## 🚀 Features

### Core Functionality
- **🔗 Complete Strapi v5 API Integration** - Full support for Collection Types, Single Types, and Components
- **📁 Media Library Support** - Upload, manage, and organize media files with metadata
- **🧩 Polymorphic Components** - Handle complex content structures with dynamic zones
- **🔍 Advanced Querying** - Sophisticated filtering, sorting, and population capabilities with fluent Action-based API
- **🛡️ Type-Safe Operations** - Strongly-typed C# interfaces for all Strapi operations
- **✨ Unified API Design** - Consistent Action-based configuration across all query builders

### Advanced Features
- **🔄 Smart Serialization** - Intelligent handling of system fields and component structures
- **⚡ Performance Optimized** - Efficient HTTP client management and caching strategies
- **🧪 Comprehensive Testing** - Extensive test suite with integration and unit tests
- **📖 Rich Documentation** - Detailed examples and API documentation

## 📦 Installation

Install the package via NuGet Package Manager:

```bash
dotnet add package Further.Strapi
```

Or via Package Manager Console:

```powershell
Install-Package Further.Strapi
```

## 🔧 Quick Start

### 1. Configuration

Configure Strapi integration in your `Program.cs` or `Startup.cs`:

```csharp
public override void ConfigureServices(ServiceConfigurationContext context)
{
    var configuration = context.Services.GetConfiguration();
    
    Configure<StrapiOptions>(options =>
    {
        options.StrapiUrl = "https://your-strapi-instance.com";
        options.StrapiToken = "your-api-token";
    });
    
    // Add Further.Strapi module
    context.Services.AddAbpDbContext<YourDbContext>(options =>
    {
        options.AddDefaultRepositories(includeAllEntities: true);
    });
}
```

### 2. Define Your Content Models

```csharp
public class Article
{
    public int Id { get; set; }
    public string DocumentId { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public Author Author { get; set; }
    public List<Component> DynamicZone { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class Author
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}
```

### 3. Use Collection Type Provider

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

    public async Task<List<Article>> GetArticlesWithPaginationAsync(int page = 1, int pageSize = 10)
    {
        return await _articleProvider.GetListAsync(
            filter: f => f.Where("publishedAt", FilterOperator.NotNull),
            populate: p => p
                .Include("author")
                .Include("category"),
            sort: s => s.Descending("createdAt"),
            pagination: p => p.Page(page, pageSize)
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

```csharp
public class GlobalSettingsService : ApplicationService
{
    private readonly ISingleTypeProvider<GlobalSettings> _globalProvider;

    public GlobalSettingsService(ISingleTypeProvider<GlobalSettings> globalProvider)
    {
        _globalProvider = globalProvider;
    }

    public async Task<GlobalSettings> GetGlobalSettingsAsync()
    {
        return await _globalProvider.GetAsync();
    }

    public async Task<string> UpdateGlobalSettingsAsync(GlobalSettings settings)
    {
        return await _globalProvider.UpdateAsync(settings);
    }
}
```

### 5. Multiple Pagination Modes

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

// Backward compatibility with PaginationInput
var paginationInput = new PaginationInput { Page = 1, PageSize = 10 };
var compatArticles = await _articleProvider.GetListAsync(
    pagination: p => p.Page(paginationInput.Page, paginationInput.PageSize)
);
```

### 6. Media Library Operations

```csharp
public class MediaService : ApplicationService
{
    private readonly IMediaLibraryProvider _mediaProvider;

    public MediaService(IMediaLibraryProvider mediaProvider)
    {
        _mediaProvider = mediaProvider;
    }

    public async Task<StrapiFile> UploadFileAsync(IFormFile file)
    {
        using var stream = file.OpenReadStream();
        
        return await _mediaProvider.UploadAsync(
            stream: stream,
            fileName: file.FileName,
            alternativeText: "Uploaded image",
            caption: "Image caption"
        );
    }

    public async Task<StrapiFile> UpdateFileInfoAsync(int fileId, UpdateFileInfoInput input)
    {
        return await _mediaProvider.UpdateFileInfoAsync(fileId, new FileInfoUpdate
        {
            AlternativeText = input.AltText,
            Caption = input.Caption,
            Name = input.Name
        });
    }
}
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
├── Further.Strapi.Contracts/       # Public interfaces and DTOs
│   ├── ICollectionTypeProvider.cs  # Collection type interface
│   ├── ISingleTypeProvider.cs      # Single type interface
│   ├── IMediaLibraryProvider.cs    # Media library interface
│   └── StrapiOptions.cs           # Configuration options
├── Further.Strapi.Shared/          # Shared utilities
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

### Dynamic Components Handling

```csharp
public class RichTextComponent
{
    public string __component => "shared.rich-text";
    public string Body { get; set; }
}

public class MediaComponent  
{
    public string __component => "shared.media";
    public StrapiFile File { get; set; }
}

// Components are automatically serialized/deserialized
// based on the __component field
```

### Polymorphic Component Processing

The library automatically handles polymorphic components in dynamic zones:

```csharp
// Components are automatically serialized/deserialized
// based on the __component field
var article = await _articleProvider.GetAsync(documentId);

foreach (var component in article.DynamicZone)
{
    switch (component)
    {
        case RichTextComponent richText:
            // Handle rich text content
            Console.WriteLine($"Rich Text: {richText.Body}");
            break;
        case MediaComponent media:
            // Handle media content  
            Console.WriteLine($"Media: {media.File.Name}");
            break;
        case QuoteComponent quote:
            // Handle quote content
            Console.WriteLine($"Quote: {quote.Title} - {quote.Body}");
            break;
    }
}
```

## 🧪 Testing

The library includes comprehensive tests covering:

- **Integration Tests**: Full end-to-end scenarios with real Strapi instances
- **Unit Tests**: Individual component testing with mocked dependencies
- **Component Tests**: Polymorphic component handling verification

Run tests with:

```bash
dotnet test
```

For integration tests, configure a test Strapi instance:

```json
{
  "StrapiOptions": {
    "StrapiUrl": "http://localhost:1337",
    "StrapiToken": "your-test-token"
  }
}
```

## 📚 Documentation

### API Reference

- [Collection Type Provider](docs/collection-type-provider.md)
- [Single Type Provider](docs/single-type-provider.md)  
- [Media Library Provider](docs/media-library-provider.md)
- [Query Builders](docs/query-builders.md)
- [Configuration Options](docs/configuration.md)

### Examples

- [Basic CRUD Operations](examples/basic-crud.md)
- [Advanced Querying](examples/advanced-querying.md)
- [Media Management](examples/media-management.md)
- [Component Handling](examples/component-handling.md)

## 🤝 Contributing

We welcome contributions! Please see our [Contributing Guide](CONTRIBUTING.md) for details.

### Development Setup

1. Clone the repository
2. Install .NET 9.0 SDK
3. Set up a local Strapi v5 instance for testing
4. Run `dotnet restore`
5. Run `dotnet build`
6. Run tests with `dotnet test`

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- Built on top of the excellent [ABP Framework](https://abp.io/)
- Designed for seamless integration with [Strapi v5](https://strapi.io/)
- Inspired by the needs of modern headless CMS applications

## 📞 Support

- 📖 [Documentation](https://github.com/yinchang0626/Further.Strapi/wiki)
- 🐛 [Issue Tracker](https://github.com/yinchang0626/Further.Strapi/issues)
- 💬 [Discussions](https://github.com/yinchang0626/Further.Strapi/discussions)

---

**Made with ❤️ for the .NET and Strapi communities**
