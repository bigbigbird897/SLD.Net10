using ConnectionModbusRtuWithTcp;
using Microsoft.AspNetCore.Mvc;
using SLD.Net10.Common.UnifiedReturn;

namespace SLD.Net10.BackgroundControllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    [ApiExplorerSettings(GroupName = "其它后台功能")]
    public class ModbusOperationController : ControllerBase
    {
        private readonly IModbusRtuWithTcpClient _modbusClient;

        public ModbusOperationController(IModbusRtuWithTcpClient modbusClient)
        {
            _modbusClient = modbusClient;
        }

        /// <summary>
        /// 读取保持寄存器
        /// </summary>
        [HttpPost]
        public async Task<ApiResult<ushort[]>> ReadRegister(string deviceCode, ushort startAddr, ushort count)
        {
            var data = await _modbusClient.ReadHoldRegistersAsync(deviceCode, startAddr, count);
            return ResultHelper.Success(data);
        }

        /// <summary>
        /// 写入单个寄存器
        /// </summary>
        [HttpPost]
        public async Task<ApiResult<object>> WriteSingleRegister(string deviceCode, ushort addr, ushort value)
        {
            await _modbusClient.WriteSingleRegisterAsync(deviceCode, addr, value);
            return ResultHelper.Success("写入完成");
        }
    }
}
