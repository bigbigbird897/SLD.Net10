using System;
using System.Collections.Generic;
using System.Text;

namespace SLD.Net10.IService.Entity
{
    /// <summary>
    /// 单条实验命令执行结果
    /// </summary>
    public class StepExecuteResult
    {
        /// <summary>是否执行成功</summary>
        public bool Success { get; set; }
        /// <summary>输出键值对，会存入全局上下文，供后续步骤引用</summary>
        public Dictionary<string, object> OutputVars { get; set; } = new();
        /// <summary>日志信息</summary>
        public string Message { get; set; } = "";
    }
}
