using System.Threading.Tasks;

namespace Further.Strapi;

/// <summary>
/// Strapi Single Type Provider 接口
/// 用於處理 Strapi 的 Single Type 內容類型（如全域設定、首頁資訊等唯一性內容）
/// </summary>
/// <typeparam name="T">Single Type 的資料模型</typeparam>
public interface ISingleTypeProvider<T> where T : class
{
    /// <summary>
    /// 取得 Single Type 資料
    /// </summary>
    /// <returns>Single Type 資料</returns>
    Task<T> GetAsync();

    /// <summary>
    /// 更新 Single Type 資料
    /// </summary>
    /// <typeparam name="TUpdateValue">更新資料的型別</typeparam>
    /// <param name="updateValue">要更新的資料</param>
    /// <returns>更新後的 Document ID</returns>
    Task<string> UpdateAsync<TUpdateValue>(TUpdateValue updateValue);
}