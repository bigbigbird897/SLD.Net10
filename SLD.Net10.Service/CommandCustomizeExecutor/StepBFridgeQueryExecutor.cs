using Microsoft.Extensions.Logging;
using SLD.Net10.IService;
using SLD.Net10.IService.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace SLD.Net10.Service.CommandCustomizeExecutor
{
    /// <summary>
    /// StepB：冰箱物料查询模拟
    /// 输入参数：Col, Layer
    /// 输出变量：BarCode、BarCodePos
    /// </summary>
    public class StepBFridgeQueryExecutor : ICommandCustomizeExecutor
    {
        private readonly ILogger<StepBFridgeQueryExecutor> _logger;
        public string MatchCommand => "StepB_FridgeQuery";

        public async Task<StepExecuteResult> ExecuteAsync(Dictionary<string, object> resolvedParams, CancellationToken token)
        {
            var result = new StepExecuteResult { Success = true };
            string col = resolvedParams["Col"].ToString();
            string layer = resolvedParams["Layer"].ToString();

            _logger.LogInformation($"[StepB] 查询冰箱：列{col} 层{layer}");
            await Task.Delay(1200, token);

            // 输出，供StepC使用
            result.OutputVars["BarCode"] = $"BAR_{col}_{layer}_001";
            result.OutputVars["BarCodePos"] = 120.55;
            result.Message = "物料查询完成";
            _logger.LogInformation($"[StepB] 读到条码：{result.OutputVars["BarCode"]}，位置={result.OutputVars["BarCodePos"]}");
            return result;
        }
    }
}
