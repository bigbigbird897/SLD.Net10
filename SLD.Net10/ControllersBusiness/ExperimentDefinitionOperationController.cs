using System.IO;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SLD.Net10.Common.WebApiUnifiedReturn;
using SLD.Net10.IService;

namespace SLD.Net10.ControllersBusiness
{
    /// <summary>
    /// 实验定义和操作
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    [ApiExplorerSettings(GroupName = "实验定义和操作")]
    public class ExperimentDefinitionOperationController : ControllerBase
    {
        private readonly ILogger<ExperimentDefinitionOperationController> _logger;

        public ExperimentDefinitionOperationController(ILogger<ExperimentDefinitionOperationController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 启动实验运行
        /// </summary>
        /// <param name="experimentId">实验ID</param>
        /// <returns></returns>
        [HttpPost("RunExperiment")]
        public async Task<ApiResult<object>> RunExperiment(long experimentId)
        {
            _logger.LogInformation("开始执行启动实验，实验ID：{ExperimentId}", experimentId);
            try
            {
                // 此处写业务逻辑：校验实验状态、初始化硬件、下发启动指令等

                return ResultHelper.Success($"实验 {experimentId} 已成功启动运行");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "启动实验失败，实验ID：{ExperimentId}", experimentId);
                return ResultHelper.ServerError($"启动实验异常：{ex.Message}");
            }
        }

        /// <summary>
        /// 暂停实验
        /// </summary>
        /// <param name="experimentId">实验ID</param>
        /// <returns></returns>
        [HttpPost("PauseExperiment")]
        public async Task<ApiResult<object>> PauseExperiment(long experimentId)
        {
            _logger.LogInformation("执行实验暂停操作，实验ID：{ExperimentId}", experimentId);
            try
            {
                // 业务逻辑：下发暂停指令、保存当前实验进度、更新状态

                return ResultHelper.Success($"实验 {experimentId} 已暂停");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "暂停实验失败，实验ID：{ExperimentId}", experimentId);
                return ResultHelper.ServerError($"暂停实验异常：{ex.Message}");
            }
        }

        /// <summary>
        /// 恢复已暂停实验
        /// </summary>
        /// <param name="experimentId">实验ID</param>
        /// <returns></returns>
        [HttpPost("ResumeExperiment")]
        public async Task<ApiResult<object>> ResumeExperiment(long experimentId)
        {
            _logger.LogInformation("执行恢复实验操作，实验ID：{ExperimentId}", experimentId);
            try
            {
                // 业务逻辑：校验当前为暂停状态、恢复硬件输出、继续实验流程

                return ResultHelper.Success($"实验 {experimentId} 已恢复运行");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "恢复实验失败，实验ID：{ExperimentId}", experimentId);
                return ResultHelper.ServerError($"恢复实验异常：{ex.Message}");
            }
        }

        /// <summary>
        /// 终止实验（强制结束，不可恢复）
        /// </summary>
        /// <param name="experimentId">实验ID</param>
        /// <returns></returns>
        [HttpPost("StopExperiment")]
        public async Task<ApiResult<object>> StopExperiment(long experimentId)
        {
            _logger.LogInformation("执行终止实验操作，实验ID：{ExperimentId}", experimentId);
            try
            {
                // 业务逻辑：切断设备输出、保存实验最终数据、标记实验为已结束

                return ResultHelper.Success($"实验 {experimentId} 已终止");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "终止实验失败，实验ID：{ExperimentId}", experimentId);
                return ResultHelper.ServerError($"终止实验异常：{ex.Message}");
            }
        }
    }
}
