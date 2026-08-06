using Microsoft.Extensions.Logging;
using Serilog;
using SLD.Net10.IService;
using SLD.Net10.IService.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace SLD.Net10.Service.CommandCustomizeExecutor
{
    /// <summary>
    /// StepA：离心控温模拟
    /// 输入参数：TargetTemp
    /// 输出变量：CentActualTemp
    /// </summary>
    public class StepACentrifugeTempExecutor : ICommandCustomizeExecutor
    {
        public string MatchCommand => "StepA_CentrifugeTemp";

        public StepACentrifugeTempExecutor()
        {
        }

        public async Task<StepExecuteResult> ExecuteAsync(Dictionary<string, object> resolvedParams, CancellationToken token)
        {
            var result = new StepExecuteResult { Success = true };
            //int targetTemp = Convert.ToInt32(resolvedParams["TargetTemp"]);
            int targetTemp = 5;

            Log.Logger.Information($"[StepA] 设置离心目标温度：{targetTemp} ℃");
            await Task.Delay(1000, token);

            // 模拟硬件返回实际温度，作为输出变量给后面步骤使用
            int actualTemp = targetTemp - 2;
            result.OutputVars["CentActualTemp"] = actualTemp;
            result.Message = $"离心完成，实际温度 {actualTemp}℃";
            Log.Logger.Information(result.Message);
            return result;
        }
    }
}
