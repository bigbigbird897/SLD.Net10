using System.IO;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SLD.Net10.Common.WebApiUnifiedReturn;
using SLD.Net10.IService;

namespace SLD.Net10.ControllersBusiness
{
    /// <summary>
    /// 实验定义和操作
    /// </summary>
    [ApiController]
    [Route("[controller]/[action]")]
    [ApiExplorerSettings(GroupName = "实验定义和操作")]
    public class ExperimentDefinitionOperationController : ControllerBase{}
}
