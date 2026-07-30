using SLD.Net10.IService.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace SLD.Net10.IService
{
    /// <summary>
    /// 命令执行器接口，每个实验命令实现此接口
    /// </summary>
    public interface ICommandCustomizeExecutor
    {
        /// <summary>匹配命令名称</summary>
        string MatchCommand { get; }

        /// <summary>执行命令，传入解析完成的强类型参数，返回输出结果</summary>
        Task<StepExecuteResult> ExecuteAsync(Dictionary<string, object> resolvedParams, CancellationToken token);
    }
}
