using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace SLD.Net10.Model.Hitbot
{
    /// <summary>
    /// Hitbot 统一交互报文模型（非泛型，兼容旧JObject场景）
    /// </summary>
    public class HitbotMessModel
    {
        /// <summary>
        /// 无参构造：给所有字段赋默认空/实例，防止空引用
        /// </summary>
        public HitbotMessModel()
        {
            message_type = string.Empty;
            message_id = string.Empty;
            instruct = string.Empty;
            set = new JObject();
            status = new StatusModel();
        }

        /// <summary>
        /// 原有构造：传入JObject参数
        /// </summary>
        /// <param name="instructName">指令名称</param>
        /// <param name="setPara">动态参数JObject</param>
        public HitbotMessModel(string instructName, JObject setPara)
        {
            message_type = "request";
            message_id = Guid.NewGuid().ToString("N");
            instruct = instructName;
            set = setPara ?? new JObject();
            status = new StatusModel();
        }

        /// <summary>
        /// 消息类型：request / response
        /// </summary>
        public string message_type { get; set; }

        /// <summary>
        /// 唯一消息ID，GUID无横杠字符串
        /// </summary>
        public string message_id { get; set; }

        /// <summary>
        /// 指令名称
        /// </summary>
        public string instruct { get; set; }

        /// <summary>
        /// 业务动态参数集合
        /// </summary>
        public JObject set { get; set; }

        /// <summary>
        /// 设备运行状态
        /// </summary>
        public StatusModel status { get; set; }

        /// <summary>
        /// 泛型解析set为实体
        /// </summary>
        public T GetSet<T>()
        {
            return set.ToObject<T>();
        }

        /// <summary>
        /// 将当前报文序列化为JSON字符串
        /// </summary>
        /// <param name="indented">是否格式化缩进，默认false紧凑输出</param>
        /// <returns>完整JSON字符串</returns>
        public string ToJsonString(bool indented = false)
        {
            Formatting fmt = indented ? Formatting.Indented : Formatting.None;
            return JsonConvert.SerializeObject(this, fmt);
        }
    }

    /// <summary>
    /// Hitbot 泛型报文模型，强类型参数专用
    /// 使用方式：new HitbotMessModel<ParaMqttAxis>("离心区z轴", new ParaMqttAxis { Position = 3.1415926 })
    /// </summary>
    /// <typeparam name="T">set对应的参数实体</typeparam>
    public class HitbotMessModel<T> : HitbotMessModel
    {
        /// <summary>
        /// 泛型构造，直接传入强类型参数实体
        /// </summary>
        /// <param name="instructName">指令名称</param>
        /// <param name="paramEntity">强类型参数对象</param>
        public HitbotMessModel(string instructName, T paramEntity)
        {
            message_type = "request";
            message_id = Guid.NewGuid().ToString("N");
            instruct = instructName;
            // 实体自动转JObject存入set
            set = paramEntity == null ? new JObject() : JObject.FromObject(paramEntity);
            status = new StatusModel();
        }

        /// <summary>
        /// 快速获取泛型参数实体
        /// </summary>
        public T GetParam()
        {
            return GetSet<T>();
        }
    }

    /// <summary>
    /// 状态子实体
    /// </summary>
    public class StatusModel
    {
        /// <summary>
        /// 工作状态 1运行 0停止
        /// </summary>
        public int work { get; set; } = 1;

        /// <summary>
        /// 报警信息，无报警为空字符串
        /// </summary>
        public string alarm_mes { get; set; } = string.Empty;
    }
}
