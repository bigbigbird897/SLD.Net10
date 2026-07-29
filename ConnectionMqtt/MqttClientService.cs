using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using System.Collections.Concurrent;


namespace ConnectionMqtt
{
    

    public class MqttClientService : IMqttClientService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<MqttClientService> _logger;
        // 缓存所有MQTT客户端：ClientId -> IMqttClient
        private readonly ConcurrentDictionary<string, IMqttClient> _clientDict = new();
        private readonly List<MqttClientConfig> _mqttConfigs = new();

        public MqttClientService(IConfiguration config, ILogger<MqttClientService> logger)
        {
            _config = config;
            _logger = logger;
            // 读取多MQTT配置
            _mqttConfigs = _config.GetSection("MqttConfigs").Get<List<MqttClientConfig>>() ?? new();
        }

        /// <summary>
        /// 程序启动时自动创建并连接所有MQTT客户端
        /// </summary>
        public async Task InitAllClientsAsync()
        {
            foreach (var cfg in _mqttConfigs)
            {
                await CreateAndConnectClientAsync(cfg);
            }
        }

        public IMqttClient GetClient(string clientId)
        {
            if (_clientDict.TryGetValue(clientId, out var client))
                return client;
            throw new KeyNotFoundException($"不存在ClientId:{clientId}的MQTT客户端");
        }

        public async Task PublishAsync(string clientId, string topic, string payload)
        {
            var client = GetClient(clientId);
            if (!client.IsConnected)
            {
                _logger.LogWarning("MQTT客户端{ClientId}未连接，尝试重连", clientId);
                await ReconnectClientAsync(clientId);
            }

            var msg = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                .Build();

            await client.PublishAsync(msg);
            _logger.LogDebug("MQTT[{ClientId}] 发布主题:{Topic},内容:{Payload}", clientId, topic, payload);
        }

        public async Task SubscribeAsync(string clientId, string topic, Func<MqttApplicationMessageReceivedEventArgs, Task> receiveHandler)
        {
            var client = GetClient(clientId);
            // 注册消息接收回调
            client.ApplicationMessageReceivedAsync += receiveHandler;
            // 订阅主题
            await client.SubscribeAsync(topic, MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce);
            _logger.LogInformation("MQTT[{ClientId}] 订阅主题:{Topic}", clientId, topic);
        }

        public async Task DisconnectAsync(string clientId)
        {
            if (_clientDict.TryGetValue(clientId, out var client) && client.IsConnected)
            {
                await client.DisconnectAsync(new MqttClientDisconnectOptionsBuilder().Build());
                _logger.LogInformation("MQTT[{ClientId}] 已断开连接", clientId);
            }
        }

        public async Task DisconnectAllAsync()
        {
            foreach (var kv in _clientDict)
            {
                await DisconnectAsync(kv.Key);
            }
            _clientDict.Clear();
        }

        #region 内部创建/重连逻辑
        private async Task CreateAndConnectClientAsync(MqttClientConfig cfg)
        {
            var factory = new MqttFactory();
            var mqttClient = factory.CreateMqttClient();

            // 配置连接参数
            var optionsBuilder = new MqttClientOptionsBuilder()
                .WithTcpServer(cfg.ServerIp, cfg.Port)
                .WithClientId(cfg.ClientId)
                .WithCleanSession(cfg.CleanSession)
                .WithKeepAlivePeriod(TimeSpan.FromSeconds(cfg.KeepAliveSecond));

            if (!string.IsNullOrEmpty(cfg.UserName))
            {
                optionsBuilder.WithCredentials(cfg.UserName, cfg.Password);
            }

            var options = optionsBuilder.Build();

            // 断线自动重连事件
            mqttClient.DisconnectedAsync += async e =>
            {
                _logger.LogError("MQTT[{ClientId}] 连接断开，原因:{Reason}", cfg.ClientId, e.Reason);
                await Task.Delay(3000);
                await ReconnectClientAsync(cfg.ClientId);
            };

            // 建立连接
            await mqttClient.ConnectAsync(options);
            _logger.LogInformation("MQTT[{ClientId}] 连接服务器成功 {Ip}:{Port}", cfg.ClientId, cfg.ServerIp, cfg.Port);

            // 存入缓存
            _clientDict.TryAdd(cfg.ClientId, mqttClient);
        }

        private async Task ReconnectClientAsync(string clientId)
        {
            var cfg = _mqttConfigs.FirstOrDefault(x => x.ClientId == clientId);
            if (cfg == null) return;

            try
            {
                await CreateAndConnectClientAsync(cfg);
                _logger.LogInformation("MQTT[{ClientId}] 重连成功", clientId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MQTT[{ClientId}] 重连失败", clientId);
            }
        }
        #endregion
    }

}
