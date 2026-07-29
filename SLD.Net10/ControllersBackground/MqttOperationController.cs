using Microsoft.AspNetCore.Mvc;
using SLD.Net10.Common.UnifiedReturn;

namespace SLD.Net10.BackgroundControllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    [ApiExplorerSettings(GroupName = "其它后台功能")]
    public class MqttOperationController : ControllerBase
    {
        private readonly IMqttClientService _mqttService;

        // 构造函数DI自动注入
        public MqttOperationController(IMqttClientService mqttService)
        {
            _mqttService = mqttService;
        }

        [HttpPost]
        public async Task<ApiResult<object>> PublishMsg(string clientId, string topic, string msg)
        {
            await _mqttService.PublishAsync(clientId, topic, msg);
            return ResultHelper.Success("发布成功");
        }

        [HttpPost]
        public async Task<ApiResult<object>> SubTopic(string clientId, string topic)
        {
            await _mqttService.SubscribeAsync(clientId, topic, async args =>
            {
                // 收到消息回调
                var payload = args.ApplicationMessage.PayloadSegment.ToString();
                var t = args.ApplicationMessage.Topic;
                Console.WriteLine($"收到消息 主题:{t} 内容:{payload}");
                await Task.CompletedTask;
            });
            return ResultHelper.Success("订阅成功");
        }
    }
}
