using System;

namespace Further.Strapi;

/// <summary>
/// Base interface for Strapi Collection Types
/// </summary>
public interface IStrapiCollectionType
{
    int? Id { get; set; }
    string? DocumentId { get; set; }
    DateTime? CreatedAt { get; set; }
    DateTime? UpdatedAt { get; set; }
    DateTime? PublishedAt { get; set; }
}