using System.Threading.Tasks;

namespace Further.Strapi;

public interface ICollectionTypeProvider<T>
    where T : class
{
    Task<T> GetAsync(string documentId);

    Task<string> CreateAsync(T collectionType);

    Task<string> UpdateAsync(string documentId, T collectionType);

    Task DeleteAsync(string documentId);
}
