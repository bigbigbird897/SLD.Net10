using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic.ApplicationServices;
using SLD.Net10.Common.WebApiUnifiedReturn;
using SLD.Net10.Extension.FrameHelper;
using SLD.Net10.Model.ControllerModuleEntity.AppSetting;
using SLD.Net10.Model.ControllerModuleEntity.Background.ModelOfManageUser.DbEntity;
using SLD.Net10.Repository;
using System.Security.Permissions;

namespace SLD.Net10.ControllersBackground
{
    [ApiController]
    [Route("[controller]/[action]")]
    [ApiExplorerSettings(GroupName = "其它后台功能")]
    public class AuthController : ControllerBase
    {
        private readonly JwtHelper _jwtHelper;
        private readonly IRepository<Model.ControllerModuleEntity.Background.ModelOfManageUser.DbEntity.User> _repository;
        private readonly IConfiguration _configuration;
        public AuthController(JwtHelper jwtHelper, IRepository<Model.ControllerModuleEntity.Background.ModelOfManageUser.DbEntity.User> repository, IConfiguration configuration)
        {
            _jwtHelper = jwtHelper;
            _repository = repository;
            _configuration = configuration;
        }

        [HttpPost]
        public async Task<ApiResult<string>> Login([FromBody] UserVoForLogin dto)
        {
            var userSecurityOption = _configuration.GetSection("UserSecurityOption").Get<UserSecurityOption>() ?? new();
            // 1. 校验账号密码（模拟数据库验证）
            var result = await _repository.Context.Queryable<Model.ControllerModuleEntity.Background.ModelOfManageUser.DbEntity.User>().AnyAsync(it => it.Username == dto.Username && it.Password == dto.Password);
            if (result)
            {
                
                var userInfo = await _repository.Context.Queryable<Model.ControllerModuleEntity.Background.ModelOfManageUser.DbEntity.User>().Where(it => it.Username == dto.Username).FirstAsync();
                
                #region 检查账户输入密码错误是否已经超过5次
                if (userInfo.PasswordErrorCount > userSecurityOption.MaxPasswordErrorCount|| (!userSecurityOption.EnableLoginFailLock))
                {
                    return ResultHelper.ServerError<string>("账户已锁定，请稍后重试或联系管理员");
                }
                #endregion

                #region 密码使用超过90天检查
                DateTime expireTime = userInfo.LastPasswordChangeTime.Value.AddDays(userSecurityOption.PasswordExpireDay);
                if (DateTime.Now > expireTime||(!userSecurityOption.EnablePasswordExpire))
                {
                    //密码已过期，拒绝登录，强制跳转修改密码
                    return ResultHelper.ServerError<string>("密码已过期，拒绝登录，修改密码");
                }
                #endregion

                

                // 生成token，携带用户ID、名称、角色
                var token = _jwtHelper.GenerateToken(userInfo.Id, userInfo.Username, userInfo.Roles.ToList());
                return ResultHelper.Success<string>(token);
            }
            else
            {
                //如果数据库查询没有找到，但是不确定是用户名错误，还是密码错误
                var resultUserName = await _repository.Context.Queryable<Model.ControllerModuleEntity.Background.ModelOfManageUser.DbEntity.User>().AnyAsync(it => it.Username == dto.Username);
                //如果找到这个用户名，那就是密码错误
                if (resultUserName)
                {
                    var userInfo = await _repository.Context.Queryable<Model.ControllerModuleEntity.Background.ModelOfManageUser.DbEntity.User>().Where(it=>it.Username==dto.Username).FirstAsync();
                    if (userInfo.PasswordErrorCount> userSecurityOption.MaxPasswordErrorCount)
                    {
                        return ResultHelper.ServerError<string>("账户已锁定，请稍后重试或联系管理员");
                    }
                    userInfo.PasswordErrorCount++;
                    if(userInfo.PasswordErrorCount> userSecurityOption.MaxPasswordErrorCount)
                    {
                        userInfo.IsLocked = true;
                    }
                    _repository.Context.Updateable<Model.ControllerModuleEntity.Background.ModelOfManageUser.DbEntity.User>(userInfo).ExecuteCommand();
                }
            }
            return ResultHelper.ServerError<string>("账号密码错误");
        }

        /// <summary>
        /// 用户退出登录
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ApiResult<bool> Logout()
        {
            // 简易逻辑：前端清除本地存储Token即可，后端直接返回成功
            // 如果需要强制Token失效，这里实现Redis黑名单逻辑
            return ResultHelper.Success(true, "退出登录成功");
        }
    }
}