namespace SLD.Net10.IService
{
    public interface IBaseServices<TEntity, TVo> where TEntity : class
    {
        Task<TVo> QueryableByEntityAsync(string name);
        Task<TVo> QueryByIdAsync(string id);
        Task<TVo> QueryByNameAsync(string name);
        Task<List<TVo>> QueryAll();
        Task<int> InsertAsync(string name);
        Task<int> UpdateAsync(string name);
        Task<int> DeleteAsync(string name);
    }
}