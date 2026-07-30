using Microsoft.AspNetCore.Mvc;
using SLD.Net10.Common.WebApiUnifiedReturn;
using SLD.Net10.Common.WebApiUnifiedReturn;
using SLD.Net10.Model.Hitbot;

namespace SLD.Net10.BackgroundControllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    [ApiExplorerSettings(GroupName = "其它后台功能")]
    public class MqttOperationController : ControllerBase
    {
        private readonly IMqttClientService _mqttService;

        private readonly ILogger? _logger;

        // 构造函数DI自动注入
        public MqttOperationController(IMqttClientService mqttService, ILogger? logger)
        {
            _logger = logger;
            _mqttService = mqttService;
        }

        [HttpPost]
        public async Task<ApiResult<object>> PublishMsg(string clientId, string topic, string msg)
        {
            var message = new HitbotMessModel<ParaMqttAxis>(
                "离心区z轴",
                new ParaMqttAxis { Position = 3.1415926 }
            );
            // 紧凑JSON（MQTT发送推荐）
            string jsonCompact = message.ToJsonString(true);
            await _mqttService.PublishAsync(clientId, topic, jsonCompact);
            return ResultHelper.Success("发布成功");
        }

        [HttpPost]
        public async Task<ApiResult<object>> SubTopic(string clientId, string topic)
        {
            await _mqttService.SubscribeAsync(
                clientId,
                topic,
                async args =>
                {
                    // 收到消息回调
                    var payload = args.ApplicationMessage.PayloadSegment.ToString();
                    var t = args.ApplicationMessage.Topic;
                    _logger.LogInformation($"收到消息 主题:{t} 内容:{payload}");
                    await Task.CompletedTask;
                }
            );
            return ResultHelper.Success("订阅成功");
        }
    }
}
