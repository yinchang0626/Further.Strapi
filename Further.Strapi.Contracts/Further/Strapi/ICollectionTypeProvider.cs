using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Further.Strapi;

public interface ICollectionTypeProvider<T>
    where T : class
{
    Task<T> GetAsync(string documentId);

    Task<string> CreateAsync(T collectionType);

    Task<string> UpdateAsync(string documentId, T collectionType);

    Task DeleteAsync(string documentId);

    /// <summary>
    /// 取得集合類型列表，支援篩選、排序和分頁
    /// </summary>
    Task<List<T>> GetListAsync(
        Action<FilterBuilder>? filter = null,
        Action<PopulateBuilder>? populate = null,
        Action<SortBuilder>? sort = null,
        Action<PaginationBuilder>? pagination = null);
}
