using Microsoft.AspNetCore.Mvc;

namespace SLD.Net10.ControllersBusiness
{
    /// <summary>
    /// 手动控制
    /// </summary>
    [ApiController]
    [Route("[controller]/[action]")]
    [ApiExplorerSettings(GroupName = "手动控制")]
    public class ManualControlController : ControllerBase { }
}
