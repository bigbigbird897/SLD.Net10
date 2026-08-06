namespace SLD.Net10.IService
{
    /// <summary>
    /// 基础服务接口，定义实体的通用 CRUD 异步操作
    /// </summary>
    /// <typeparam name="TEntity">实体类型，必须为引用类型</typeparam>
    /// <typeparam name="TVo">视图对象类型，用于返回数据传输</typeparam>
    /// <remarks>
    /// 所有业务服务接口建议继承此接口，以统一基础数据操作规范
    /// </remarks>
    public interface IBaseServices<TEntity, TVo> where TEntity : class
    {
        /// <summary>
        /// 根据实体名称条件查询单个数据
        /// </summary>
        /// <param name="name">查询条件名称</param>
        /// <returns>匹配的视图对象，未找到返回 null</returns>
        Task<TVo> QueryableByEntityAsync(string name);

        /// <summary>
        /// 根据 ID 查询单条数据
        /// </summary>
        /// <param name="id">数据唯一标识</param>
        /// <returns>匹配的视图对象，未找到返回 null</returns>
        Task<TVo> QueryByIdAsync(string id);

        /// <summary>
        /// 根据名称查询单条数据
        /// </summary>
        /// <param name="name">数据名称</param>
        /// <returns>匹配的视图对象，未找到返回 null</returns>
        Task<TVo> QueryByNameAsync(string name);

        /// <summary>
        /// 查询所有数据
        /// </summary>
        /// <returns>所有数据的视图对象集合</returns>
        Task<List<TVo>> QueryAll();

        /// <summary>
        /// 新增一条数据
        /// </summary>
        /// <param name="name">新增数据的名称</param>
        /// <returns>受影响的行数，成功返回 1，失败返回 0</returns>
        Task<int> InsertAsync(string name);

        /// <summary>
        /// 更新一条数据
        /// </summary>
        /// <param name="name">更新数据的名称</param>
        /// <returns>受影响的行数，成功返回 1，失败返回 0</returns>
        Task<int> UpdateAsync(string name);

        /// <summary>
        /// 删除一条数据
        /// </summary>
        /// <param name="name">待删除数据的名称</param>
        /// <returns>受影响的行数，成功返回 1，失败返回 0</returns>
        Task<int> DeleteAsync(string name);
    }
}