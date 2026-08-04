using SqlSugar;
using System.Collections.Generic;

namespace SLD.Net10.Repository
{
    /// <summary>
    /// 通用仓储实现
    /// </summary>
    /// <typeparam name="T">实体</typeparam>
    public class Repository<T> : SimpleClient<T>, IRepository<T> where T : class, new()
    {
        public new ISqlSugarClient Context
        {
            get => base.Context;
            set => base.Context = value;
        }

        public Repository(ISqlSugarClient db)
        {
            base.Context = db;
        }

        /// <summary>
        /// 扩展方法，自带方法不能满足的时候可以添加新方法
        /// </summary>
        /// <returns></returns>
        public List<T> CommQuery(string json)
        {
            //base.Context.Queryable<T>().ToList();可以拿到SqlSugarClient 做复杂操作
            return null;
        }
    }
}