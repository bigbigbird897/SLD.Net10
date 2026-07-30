using MQTTnet;
using MQTTnet.Client;
using MQTTnet.LowLevelClient;

/// <summary>
/// 参考文档：
/// 1 多MQTT客户端封装+配置读取+DI注入完整实现（MQTTnet + ASP.NET Core）
/// </summary>
public interface IMqttClientService
{
    /// <summary>
    /// 根据ClientId获取对应MQTT客户端
    /// </summary>
    IMqttClient GetClient(string clientId);

    /// <summary>
    /// 发布消息
    /// </summary>
    Task PublishAsync(string clientId, string topic, string payload);

    /// <summary>
    /// 订阅主题
    /// </summary>
    Task SubscribeAsync(string clientId, string topic, Func<MqttApplicationMessageReceivedEventArgs, Task> receiveHandler);

    /// <summary>
    /// 断开指定客户端
    /// </summary>
    Task DisconnectAsync(string clientId);

    /// <summary>
    /// 全部断开
    /// </summary>
    Task DisconnectAllAsync();

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    Task InitAllClientsAsync();
}
