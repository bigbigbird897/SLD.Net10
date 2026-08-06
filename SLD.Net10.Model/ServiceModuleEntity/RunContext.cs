using System;
using System.Collections.Generic;
using System.Text;

namespace SLD.Net10.IService.Entity
{
    /// <summary>
    /// 运行上下文
    /// </summary>
    public class RunContext
    {
        public RunStatus Status { get; set; } = RunStatus.Idle;

        /// <summary>当前执行命令索引</summary>
        public int CurrentCommandIndex { get; set; } = 0;

        /// <summary>【关键】全局变量池：上一步输出结果保存在这里，后续步骤读取</summary>
        public Dictionary<string, object> ContextVariables { get; set; } = new();

        public readonly ManualResetEventSlim PauseEvent = new ManualResetEventSlim(true);

        public CancellationTokenSource? StopTokenSource { get; set; }
    }
}
