using Newtonsoft.Json;
using SqlSugar;

namespace SLD.Net10.Repository
{
    public class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : class, new()
    {
        private readonly ISqlSugarClient _dbBase;
        public BaseRepository(ISqlSugarClient sqlSugarClient)
        {
            _dbBase = sqlSugarClient;
        }
        public async Task<List<TEntity>> Query()
        {
            //异步打印输出数据库上下文实例Db的哈希码，用于判断Db对象是不是同一个实例
            //await Console.Out.WriteLineAsync(Db.GetHashCode().ToString());
            return await _dbBase.Queryable<TEntity>().ToListAsync();
        }
    }
}
