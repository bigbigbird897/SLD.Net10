using Newtonsoft.Json.Linq;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Text;

namespace SLD.Net10.Model.ControllerModuleEntity.Business.ModelOfExperimentDefinOpera.DbEntity
{
    /// <summary>
    /// 实验信息模型
    /// </summary>
    //[SugarTable("t_experiment")] //数据库表名
    public class ExperimentModel
    {
        /// <summary>
        /// 实验唯一编号
        /// </summary>
        [SugarColumn(IsPrimaryKey = true)]
        public string? ExperimentId { get; set; }

        /// <summary>
        /// 实验名称
        /// </summary>
        [SugarColumn(ColumnDataType = "varchar(200)", IsNullable = false)]
        public string ExperimentName { get; set; } = string.Empty;

        /// <summary>
        /// 创建时间
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 实验步骤集合
        /// </summary>
        /// <remarks>数组存储为json字符串，SqlSugar自动序列化/反序列化</remarks>
        [SugarColumn(ColumnDataType = "text", IsJson = true, IsNullable = true)]
        public string[] Steps { get; set; } = Array.Empty<string>();

        /// <summary>
        /// 硬件自定义参数（动态JSON对象）
        /// </summary>
        /// <remarks>JObject直接开启IsJson，底层存文本，读写自动转换JObject</remarks>
        [SugarColumn(ColumnDataType = "text", IsJson = true, IsNullable = true)]
        public JObject HardwareParams { get; set; } = new JObject();
    }
}