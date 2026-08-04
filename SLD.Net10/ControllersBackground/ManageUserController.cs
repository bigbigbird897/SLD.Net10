using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SLD.Net10.Common;
using SLD.Net10.Common.WebApiUnifiedReturn;
using SLD.Net10.Model.Background.ModelOfManageUser;
using SLD.Net10.Repository;
using SqlSugar;

namespace SLD.Net10.ControllersBackground
{
    [ApiController]
    [Route("[controller]/[action]")]
    [ApiExplorerSettings(GroupName = "其它后台功能")]
    //[Authorize] // 需要JWT登录访问时打开
    public class ManageUserController : ControllerBase
    {
        private readonly IRepository<User> _repository;
        private readonly IMapper _mapper;

        public ManageUserController(IRepository<User> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        /// <summary>
        /// 用户分页列表
        /// </summary>
        //[HttpGet]
        //public async Task<ApiResult<PageResult<UserVo>>> GetUserPage([FromQuery] UserQueryDto query)
        //{
        //    var q = _repository.Context.Queryable<User>();
        //    if (!string.IsNullOrEmpty(query.Username))
        //    {
        //        q = q.Where(it => it.Username.Contains(query.Username));
        //    }

        //    var page = await q.ToPageAsync(query.PageIndex, query.PageSize);
        //    var pageVo = _mapper.Map<PageResult<UserVo>>(page);
        //    return ResultHelper.Success(pageVo);
        //}

        /// <summary>
        /// 根据ID查询单条用户
        /// </summary>
        [HttpGet]
        public async Task<ApiResult<UserVo?>> GetUserById(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return ResultHelper.ServerError<UserVo?>(null,"Id不能为空");
            }
            var model = await _repository.Context.Queryable<User>()
                .Where(it => it.Id == id)
                .FirstAsync();
            var vo = model == null ? null : _mapper.Map<UserVo>(model);
            return ResultHelper.Success(vo);
        }

        /// <summary>
        /// 新增用户
        /// </summary>
        [HttpPost]
        public async Task<ApiResult<bool>> AddUser(string username,string password)
        {
            // 校验用户名是否重复
            var exist = await _repository.Context.Queryable<User>()
                .AnyAsync(it => it.Username == username);
            if (exist)
            {
                return ResultHelper.ServerError<bool>(false,"用户名已存在");
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                return ResultHelper.ServerError<bool>(false,"新增用户密码不能为空");
            }

            var user = new User
            {
                Id = StringHelper.GetDateTimeWithGuid(),
                Username = username,
                //正式环境建议密码加密
                Password = password
            };
            await _repository.InsertAsync(user);
            return ResultHelper.Success(true, "新增成功");
        }

        /// <summary>
        /// 编辑用户（不修改密码时Password不传）
        /// </summary>
        [HttpPost]
        public async Task<ApiResult<bool>> EditUser([FromBody] UserVo dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Id))
            {
                return ResultHelper.ServerError<bool>(false,"用户Id不能为空");
            }

            // 排除自身，校验重名
            var exist = await _repository.Context.Queryable<User>()
                .AnyAsync(it => it.Username == dto.Username && it.Id != dto.Id);
            if (exist)
            {
                return ResultHelper.ServerError<bool>(false, "用户名已存在");
            }

            var user = await _repository.Context.Queryable<User>()
                .FirstAsync(it => it.Id == dto.Id);
            if (user == null)
            {
                return ResultHelper.ServerError<bool>(false, "用户不存在");
            }

            user.Username = dto.Username;
            // 传入密码才更新，不传保留原有密码
            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                user.Password = dto.Password;
                //user.Password = MD5Helper.Encrypt(dto.Password);
            }

            await _repository.UpdateAsync(user);
            return ResultHelper.Success(true, "编辑成功");
        }

        /// <summary>
        /// 删除用户
        /// </summary>
        [HttpPost]
        public async Task<ApiResult<bool>> DeleteUser(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return ResultHelper.ServerError<bool>(false, "id不能为空");

            var user = await _repository.Context.Queryable<User>().FirstAsync(it => it.Id == id);
            if (user == null)
                return ResultHelper.ServerError<bool>(false, "用户不存在");

            //可选限制：禁止删除admin账号
            //if(user.Username == "admin")
            //    return ResultHelper.ServerError<bool>("超级管理员禁止删除");

            var result=_repository.Context.Deleteable<User>(new User { Id=id}).ExecuteCommand();
            return ResultHelper.Success(true, "删除成功");
        }

        /// <summary>
        /// 用户修改密码
        /// </summary>
        [HttpPost]
        public async Task<ApiResult<bool>> ChangePassword(string userId, string oldPwd, string newPwd)
        {
            var user = await _repository.Context.Queryable<User>().FirstAsync(it => it.Id == userId);
            if (user == null)
                return ResultHelper.ServerError<bool>(false,"用户不存在");

            if (user.Password != oldPwd)
            {
                return ResultHelper.ServerError<bool>(false, "原密码不正确");
            }

            user.Password = newPwd;
            //user.Password = MD5Helper.Encrypt(newPwd);
            await _repository.UpdateAsync(user);
            return ResultHelper.Success(true, "密码修改成功");
        }
    }
}