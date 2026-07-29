using Microsoft.AspNetCore.Mvc;
using SLD.Net10.Common.UnifiedReturn;

namespace SLD.Net10.Controllers
{
    /// <summary>
    /// 实验定义和操作
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    [ApiExplorerSettings(GroupName = "实验定义和操作")]
    public class ExperimentDefinitionOperationController:ControllerBase
    {
        [HttpGet("ReadRegister")]
        public async Task<ApiResult<object>> ReadRegister(string deviceId, int addr)
        {
            // 3. 正常返回点位数据
            return ResultHelper.Success("点位读取完成");
        }
    }
}
