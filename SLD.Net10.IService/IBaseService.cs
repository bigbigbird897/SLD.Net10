namespace SLD.Net10.IService
{
    public interface IBaseServices<TEntity, TVo> where TEntity : class
    {
        Task<List<TVo>> Query();
    }
}
