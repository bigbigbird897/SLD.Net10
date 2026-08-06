using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SLD.Net10.Common.FunctionHelper;
using SLD.Net10.Common.WebApiUnifiedReturn;
using SLD.Net10.Model.ControllerModuleEntity.AppSetting;
using SLD.Net10.Model.ControllerModuleEntity.Background.ModelOfManageUser.DbEntity;
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
        private readonly IConfiguration _configuration;

        public ManageUserController(IRepository<User> repository, IMapper mapper, IConfiguration configuration)
        {
            _repository = repository;
            _mapper = mapper;
            _configuration = configuration;
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
        public async Task<ApiResult<User?>> GetUserById(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return ResultHelper.ServerError<User?>(null,"Id不能为空");
            }
            var model = await _repository.Context.Queryable<User>()
                .Where(it => it.Id == id)
                .FirstAsync();
            var vo = model == null ? null : _mapper.Map<User>(model);
            return ResultHelper.Success(vo);
        }

        /// <summary>
        /// 根据用户名称查询单条用户
        /// </summary>
        [HttpGet]
        public async Task<ApiResult<User?>> GetUserByUserName(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                return ResultHelper.ServerError<User?>(null, "userName不能为空");
            }
            var model = await _repository.Context.Queryable<User>()
                .Where(it => it.Username == userName)
                .FirstAsync();
            var vo = model == null ? null : _mapper.Map<User>(model);
            return ResultHelper.Success(vo);
        }

        /// <summary>
        /// 新增用户
        /// </summary>
        [HttpPost]
        //[Authorize(Roles = "Admin")]
        public async Task<ApiResult<bool>> AddUser([FromBody] UserVoForAdd userVo)
        {
            // 校验用户名是否重复
            var exist = await _repository.Context.Queryable<User>()
                .AnyAsync(it => it.Username == userVo.Username);
            if (exist)
            {
                return ResultHelper.ServerError<bool>(false,"用户名已存在");
            }

            if (string.IsNullOrWhiteSpace(userVo.Password))
            {
                return ResultHelper.ServerError<bool>(false,"新增用户密码不能为空");
            }

            var userSecurityOption = _configuration.GetSection("UserSecurityOption").Get<UserSecurityOption>() ?? new();
            if (userVo.Password.Trim().Length < 8|| (!userSecurityOption.EnablePasswordHave8Bit))
            {
                return ResultHelper.ServerError<bool>(false, "密码长度不能少于8位");
            }

            var user = new User
            {
                Id = GenerateGuidStringHelper.GetDateTimeWithGuid(),
                Username = userVo.Username,
                //正式环境建议密码加密
                Password = userVo.Password,
                Roles = userVo.Roles,
                LastPasswordChangeTime=DateTime.Now
            };
            await _repository.InsertAsync(user);
            return ResultHelper.Success(true, "新增成功");
        }

        /// <summary>
        /// 编辑用户（不修改密码时Password不传）
        /// </summary>
        [HttpPost]
        public async Task<ApiResult<bool>> EditUser([FromBody] User newEntity)
        {
            if (string.IsNullOrWhiteSpace(newEntity.Id))
            {
                return ResultHelper.ServerError<bool>(false, "用户Id不能为空");
            }

            // 排除自身，校验重名
            // 可能存在同名的使用者
            //var exist = await _repository.Context.Queryable<User>()
            //    .AnyAsync(it => it.Username == entity.Username);
            //if (exist)
            //{
            //    return ResultHelper.ServerError<bool>(false, "用户名已存在");
            //}

            var user = await _repository.Context.Queryable<User>()
                .FirstAsync(it => it.Id == newEntity.Id);
            if (user == null)
            {
                return ResultHelper.ServerError<bool>(false, "用户不存在");
            }

            user.Username = newEntity.Username;
            // 传入密码才更新，不传保留原有密码
            if (!string.IsNullOrWhiteSpace(newEntity.Password))
            {
                user.Password = newEntity.Password;
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

        /// <summary>
        /// 根据用户Id移除用户被锁定的状态
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult<bool>> RemoveUserLockedStatusById(string id)
        {
            var userInfo = await _repository.Context.Queryable<User>().Where(it => it.Id == id).FirstAsync();
            userInfo.IsLocked = false;
            userInfo.PasswordErrorCount = 0;
            var returnCode=_repository.Context.Updateable<User>(userInfo).ExecuteCommand();
            if (returnCode == 0)
            {
                return ResultHelper.Success<bool>(false);
            }
            return ResultHelper.Success<bool>(true);
        }
    }
}