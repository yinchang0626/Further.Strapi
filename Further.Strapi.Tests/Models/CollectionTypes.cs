using Further.Strapi.Serialization;
using System;
using System.Collections.Generic;

namespace Further.Strapi.Tests.Models;

/// <summary>
/// Article Collection Type - 對應 api::article.article
/// Create your blog content
/// </summary>
[StrapiCollectionName("articles")]
public class Article
{
    public int? Id { get; set; }
    public string? DocumentId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }

    /// <summary>
    /// Article title
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Article description (max 80 characters)
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// URL slug generated from title
    /// </summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>
    /// Cover image/video/file
    /// </summary>
    public StrapiMediaField? Cover { get; set; }

    /// <summary>
    /// Dynamic zone with multiple component types
    /// </summary>
    public List<IStrapiComponent>? Blocks { get; set; }

    /// <summary>
    /// Related author (Many-to-One relationship)
    /// </summary>
    public Author? Author { get; set; }

    /// <summary>
    /// Related category (Many-to-One relationship)
    /// </summary>
    public Category? Category { get; set; }
}

/// <summary>
/// Author Collection Type - 對應 api::author.author
/// Create authors for your content
/// </summary>
[StrapiCollectionName("authors")]
public class Author
{
    public int? Id { get; set; }
    public string? DocumentId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }

    /// <summary>
    /// Author name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Author email
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Author avatar image
    /// </summary>
    public StrapiMediaField? Avatar { get; set; }

    /// <summary>
    /// Articles written by this author (One-to-Many relationship)
    /// </summary>
    public List<Article>? Articles { get; set; }
}

/// <summary>
/// Category Collection Type - 對應 api::category.category
/// Organize your content into categories
/// </summary>
[StrapiCollectionName("categories")]
public class Category
{
    public int? Id { get; set; }
    public string? DocumentId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }

    /// <summary>
    /// Category name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Category description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// URL slug
    /// </summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>
    /// Articles in this category (One-to-Many relationship)
    /// </summary>
    public List<Article>? Articles { get; set; }
}