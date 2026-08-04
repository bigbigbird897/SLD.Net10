using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SLD.Net10.Common.WebApiUnifiedReturn;
using SqlSugar;
using SLD.Net10.Model.Background.ModelOfManageUser;
using SLD.Net10.IService;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SLD.Net10.ControllersBackground
{
    [ApiController]
    [Route("[controller]/[action]")]
    [ApiExplorerSettings(GroupName = "其它后台功能")]
    [Authorize] // 需要JWT登录访问
    public class ManageUserController : ControllerBase
    {
        private readonly IBaseServices<User,UserVo> _baseService;

        public ManageUserController(IBaseServices<User, UserVo> baseService)
        {
            _baseService = baseService;
        }

        [HttpGet]
        public async Task<ApiResult<object>> TestJwtFunc()
        {
            return ResultHelper.Success("测试用户认证功能Jwt");
        }

        /// <summary>
        /// 用户分页列表
        /// </summary>
        //[HttpGet]
        //public async Task<ApiResult<PageResult<User>>> GetUserPage([FromQuery] UserVo query)
        //{
        //    var q = _baseService.Queryable<User>();
        //    if (!string.IsNullOrEmpty(query.Username))
        //    {
        //        q = q.Where(it => it.Username.Contains(query.Username));
        //    }

        //    var page = await q.ToPageAsync(query.PageIndex, query.PageSize);
        //    return ResultHelper.Success(page);
        //}

        /// <summary>
        /// 根据ID查询单条用户
        /// </summary>
        [HttpGet]
        public async Task<ApiResult<User?>> GetUserById(long id)
        {
            var model = await _baseService.Queryable<User>().FirstAsync(it => it.Id == id);
            return ResultHelper.Success(model);
        }

        /// <summary>
        /// 新增用户
        /// </summary>
        [HttpPost]
        public async Task<ApiResult<bool>> AddUser([FromBody] UserEditDto dto)
        {
            // 校验用户名是否重复
            var exist = await _baseService.Queryable<User>().AnyAsync(it => it.Username == dto.Username);
            if (exist)
            {
                return ResultHelper.ServerError<bool>("用户名已存在");
            }

            if (string.IsNullOrWhiteSpace(dto.Password))
            {
                return ResultHelper.ServerError<bool>("新增用户密码不能为空");
            }

            var user = new User
            {
                Username = dto.Username,
                // 建议加密：Password = MD5Helper.Encrypt(dto.Password)
                Password = dto.Password
            };
            await _baseService.InsertAsync(user);
            return ResultHelper.Success(true, "新增成功");
        }

        /// <summary>
        /// 编辑用户（不修改密码时Password不传）
        /// </summary>
        [HttpPost]
        public async Task<ApiResult<bool>> EditUser([FromBody] UserEditDto dto)
        {
            if (!dto.Id.HasValue || dto.Id <= 0)
            {
                return ResultHelper.ServerError<bool>("用户Id不能为空");
            }

            // 排除自身，校验重名
            var exist = await _baseService.Queryable<User>()
                .AnyAsync(it => it.Username == dto.Username && it.Id != dto.Id.Value);
            if (exist)
            {
                return ResultHelper.ServerError<bool>("用户名已存在");
            }

            var user = await _baseService.Queryable<User>().FirstAsync(it => it.Id == dto.Id.Value);
            if (user == null)
            {
                return ResultHelper.ServerError<bool>("用户不存在");
            }

            user.Username = dto.Username;
            // 传入密码才更新，不传保留原有密码
            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                user.Password = dto.Password;
                // 加密版本：user.Password = MD5Helper.Encrypt(dto.Password);
            }

            await _baseService.UpdateAsync(user);
            return ResultHelper.Success(true, "编辑成功");
        }

        /// <summary>
        /// 删除用户
        /// </summary>
        [HttpPost]
        public async Task<ApiResult<bool>> DeleteUser(long id)
        {
            if (id <= 0)
                return ResultHelper.ServerError<bool>("id无效");

            // 可增加逻辑：禁止删除超级管理员admin
            var user = await _baseService.Queryable<User>().FirstAsync(it => it.Id == id);
            if (user == null)
                return ResultHelper.ServerError<bool>("用户不存在");

            await _baseService.DeleteAsync<User>(id);
            return ResultHelper.Success(true, "删除成功");
        }

        /// <summary>
        /// 用户修改密码
        /// </summary>
        [HttpPost]
        public async Task<ApiResult<bool>> ChangePassword(long userId, string oldPwd, string newPwd)
        {
            var user = await _baseService.Queryable<User>().FirstAsync(it => it.Id == userId);
            if (user == null)
                return ResultHelper.ServerError<bool>("用户不存在");

            if (user.Password != oldPwd)
            {
                return ResultHelper.ServerError<bool>("原密码不正确");
            }

            user.Password = newPwd;
            // user.Password = MD5Helper.Encrypt(newPwd);
            await _baseService.UpdateAsync(user);
            return ResultHelper.Success(true, "密码修改成功");
        }
    }
}