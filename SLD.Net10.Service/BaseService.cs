using AutoMapper;
using SLD.Net10.IService;
using SLD.Net10.Model.Background.ModelOfManageUser;
using SLD.Net10.Repository;

namespace SLD.Net10.Service
{
    public class BaseServices<TEntity, TVo> : IBaseServices<TEntity, TVo> where TEntity : class, new()
    {
        private readonly IMapper _mapper;
        private readonly IRepository<User> _repository;

        public BaseServices(IMapper mapper, IRepository<User> repository)
        {
            _mapper = mapper;
            _repository = repository;
        }

        public Task<int> DeleteAsync(string name)
        {
            throw new NotImplementedException();
        }

        public Task<int> InsertAsync(string name)
        {
            throw new NotImplementedException();
        }

        public Task<int> UpdateAsync(string name)
        {
            throw new NotImplementedException();
        }

        public Task<TVo> QueryableByEntityAsync(string name)
        {
            throw new NotImplementedException();
        }

        public Task<TVo> QueryByNameAsync(string name)
        {
            throw new NotImplementedException();
        }

        Task<List<TVo>> IBaseServices<TEntity, TVo>.QueryAll()
        {
            throw new NotImplementedException();
        }

        Task<TVo> IBaseServices<TEntity, TVo>.QueryByIdAsync(string id)
        {
            throw new NotImplementedException(); ;
        }
    }
}