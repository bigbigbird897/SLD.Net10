using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace SLD.Net10.Model.Business.ModelOfExperimentDefinOpera
{
    /// <summary>
    /// 实验信息模型
    /// </summary>
    public class ExperimentModel
    {
        /// <summary>
        /// 实验唯一编号
        /// </summary>
        public string ExperimentId { get; set; }

        /// <summary>
        /// 实验名称
        /// </summary>
        public string ExperimentName { get; set; } = string.Empty;

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 实验步骤集合
        /// </summary>
        public string[] Steps { get; set; } = Array.Empty<string>();

        /// <summary>
        /// 硬件自定义参数（动态JSON对象）
        /// </summary>
        public JObject HardwareParams { get; set; } = new JObject();
    }
}
