using SLD.Net10.IService;
using SLD.Net10.Repository;

using AutoMapper;

namespace SLD.Net10.Service
{
    public class BaseServices<TEntity, TVo> : IBaseServices<TEntity, TVo> where TEntity : class, new()
    {
        private readonly IMapper _mapper;
        private readonly IBaseRepository<TEntity> _baseRepository;

        public BaseServices(IMapper mapper, IBaseRepository<TEntity> baseRepository)
        {
            _mapper = mapper;
            _baseRepository = baseRepository;
        }

        public async Task<List<TVo>> Query()
        {
            var entities = await _baseRepository.Query();
            // 打印仓储实例哈希码，用于调试判断是否为同一个对象实例
            //Console.WriteLine($"_baseRepository 实例HashCode ： {_baseRepository.GetHashCode()}");
            var llout = _mapper.Map<List<TVo>>(entities);
            return llout;
        }
    }
}
