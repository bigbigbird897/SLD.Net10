using Microsoft.Extensions.Logging;
using SLD.Net10.IService;
using SLD.Net10.IService.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace SLD.Net10.Service.CommandCustomizeExecutor
{
    /// <summary>
    /// StepC：轴移动
    /// 输入参数 MovePosition，可以填常量，也可以填 ${BarCodePos} 引用上一步输出
    /// 输出：AxisFinalPos
    /// </summary>
    public class StepCAxisMoveExecutor : ICommandCustomizeExecutor
    {
        private readonly ILogger<StepCAxisMoveExecutor> _logger;
        public string MatchCommand => "StepC_AxisMove";

        public async Task<StepExecuteResult> ExecuteAsync(Dictionary<string, object> resolvedParams, CancellationToken token)
        {
            var result = new StepExecuteResult { Success = true };
            double pos = Convert.ToDouble(resolvedParams["MovePosition"]);

            _logger.LogInformation($"[StepC] 轴移动到位置 {pos}");
            await Task.Delay(2000, token);

            result.OutputVars["AxisFinalPos"] = pos;
            result.Message = "轴运动完成";
            _logger.LogInformation(result.Message);
            return result;
        }
    }
}
