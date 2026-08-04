using Microsoft.AspNetCore.Mvc;
using SLD.Net10.Common.WebApiUnifiedReturn;
using SLD.Net10.Extension.ComponentConfig;
using SLD.Net10.Model.Background.ModelOfManageUser;

namespace SLD.Net10.ControllersBackground
{
    [ApiController]
    [Route("[controller]/[action]")]
    [ApiExplorerSettings(GroupName = "其它后台功能")]
    public class AuthController : ControllerBase
    {
        private readonly JwtHelper _jwtHelper;
        public AuthController(JwtHelper jwtHelper)
        {
            _jwtHelper = jwtHelper;
        }

        [HttpPost]
        public ApiResult<string> Login([FromBody] UserVo dto)
        {
            // 1. 校验账号密码（模拟数据库验证）
            if (dto.Username == "admin" && dto.Password == "admin")
            {
                // 生成token，携带用户ID、名称、角色
                var token = _jwtHelper.GenerateToken(1, "admin", new List<string> { "Admin" });
                return ResultHelper.Success<string>(token);
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