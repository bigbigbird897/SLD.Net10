using Microsoft.AspNetCore.Mvc;
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

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDto dto)
        {
            // 1. 校验账号密码（模拟数据库验证）
            if (dto.Username == "admin" && dto.Password == "admin")
            {
                // 生成token，携带用户ID、名称、角色
                var token = _jwtHelper.GenerateToken(1, "admin", new List<string> { "Admin" });
                return Ok(new { token });
            }
            return Unauthorized("账号密码错误");
        }
    }

    
}
