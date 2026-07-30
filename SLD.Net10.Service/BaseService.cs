using AutoMapper;
using SLD.Net10.IService;
using SLD.Net10.Repository;

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
            var llout = _mapper.Map<List<TVo>>(entities);
            return llout;
        }
    }
}