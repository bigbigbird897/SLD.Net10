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
            return await _dbBase.Queryable<TEntity>().ToListAsync();
        }
    }
}