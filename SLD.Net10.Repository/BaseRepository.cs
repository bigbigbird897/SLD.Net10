using SqlSugar;

namespace SLD.Net10.Repository
{
    public class BaseRepository<T> : IBaseRepository<T> where T : class, new()
    {

    }
}