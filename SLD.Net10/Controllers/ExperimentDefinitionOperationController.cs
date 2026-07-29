using Microsoft.AspNetCore.Mvc;
using Serilog;
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
        private readonly ILogger<ExperimentDefinitionOperationController>? _logger;
        public ExperimentDefinitionOperationController(ILogger<ExperimentDefinitionOperationController> logger)
        {
            _logger = logger;
        }
        [HttpGet("ReadRegister")]
        public async Task<ApiResult<object>> ReadRegister(string deviceId, int addr)
        {
            Log.Error("这是一个错误");
            _logger.LogInformation("测试一下");
            // 3. 正常返回点位数据
            return ResultHelper.Success("点位读取完成");
        }
    }
}
