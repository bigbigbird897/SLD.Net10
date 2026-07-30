using System;
using System.Collections.Generic;
using System.Text;

namespace SLD.Net10.IService.Entity
{
    /// <summary>
    /// 实验命令定义：一条可执行步骤，携带输入参数字典
    /// 输入参数支持两种：固定值，或者 ${VarName} 引用上一步输出变量
    /// </summary>
    public class ExperimentCommand
    {
        /// <summary>命令名称 StepA / StepB / StepC</summary>
        public string CommandName { get; set; }

        /// <summary>输入参数字典：key=参数名，value可以是常量或者 ${变量名}</summary>
        public Dictionary<string, string> InputParams { get; set; } = new();

        /// <summary>模拟该命令执行耗时</summary>
        public int SimulateDelayMs { get; set; }
    }
}
