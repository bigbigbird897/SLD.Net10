using SqlSugar;
using System.Collections.Generic;

namespace SLD.Net10.Repository
{
    /// <summary>
    /// 通用仓储接口
    /// </summary>
    /// <typeparam name="T">实体</typeparam>
    public interface IRepository<T> : ISimpleClient<T> where T : class, new()
    {
        /// <summary>
        /// SqlSugar数据库上下文
        /// </summary>
        ISqlSugarClient Context { get; }

        /// <summary>
        /// 自定义通用查询扩展
        /// </summary>
        List<T> CommQuery(string json);
    }
}