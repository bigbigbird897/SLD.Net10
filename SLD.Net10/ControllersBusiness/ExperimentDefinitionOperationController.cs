using Autofac;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SLD.Net10.Common.WebApiUnifiedReturn;
using SLD.Net10.IService;
using SLD.Net10.IService.Entity;
using SLD.Net10.Model.Business.ModelOfExperimentDefinOpera;
using SLD.Net10.Service;
using SLD.Net10.Service.CommandCustomizeExecutor;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Forms.Design;

[ApiController]
[Route("[controller]/[action]")]
[ApiExplorerSettings(GroupName = "实验定义和操作")]
public class ExperimentDefinitionOperationController : ControllerBase
{
    private readonly ILogger<ExperimentDefinitionOperationController> _logger;
    private readonly IExperimentRuntimePool _runtimePool;
    private IExperimentManager _experimentManager;
    private readonly ILifetimeScope _scope;

    public ExperimentDefinitionOperationController(
        ILogger<ExperimentDefinitionOperationController> logger,
        IExperimentRuntimePool runtimePool,
        IExperimentManager experimentManager,
        ILifetimeScope scope
    )
    {
        _logger = logger;
        _runtimePool = runtimePool;
        _experimentManager = experimentManager;
        _scope=scope;
    }

    #region 导出实验
    [HttpPost]
    public async Task<IActionResult> ExportExperiment(long experimentId)
    {
        _logger.LogInformation("开始导出实验文件，实验ID：{ExperimentId}", experimentId);
        try
        {
            var experimentModel = new ExperimentModel
            {
                ExperimentId = experimentId,
                ExperimentName = $"实验_{experimentId}",
                CreateTime = DateTime.Now,
                Steps = new[] { "升温", "保温", "降温" },
                HardwareParams = new JObject(
                    new JProperty("Temp", 25),
                    new JProperty("Speed", 500)
                ),
            };

            string json = JsonConvert.SerializeObject(experimentModel, Formatting.Indented);
            byte[] fileBytes = System.Text.Encoding.UTF8.GetBytes(json);
            string fileName = $"实验_{experimentId}_{DateTime.Now:yyyyMMddHHmmss}.exp";

            _logger.LogInformation(
                "实验{ExperimentId}导出文件成功，文件名：{FileName}",
                experimentId,
                fileName
            );
            return File(fileBytes, "application/octet-stream", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "导出实验文件失败，实验ID：{ExperimentId}", experimentId);
            return BadRequest(ResultHelper.ServerError($"导出失败：{ex.Message}"));
        }
    }

    /// <summary>
    /// 上传.exp文件加载实验，创建ExperimentManager存入运行池
    /// </summary>
    [HttpPost]
    public async Task<ApiResult<object>> LoadExperimentByFile(IFormFile file)
    {
        _logger.LogInformation("开始上传并解析实验文件，文件名：{FileName}", file.FileName);
        try
        {
            string ext = Path.GetExtension(file.FileName).ToLower();
            if (ext != ".exp")
            {
                return ResultHelper.ParamError("仅支持后缀为 .exp 的实验文件");
            }

            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            ms.Position = 0;
            string fileJson = System.Text.Encoding.UTF8.GetString(ms.ToArray());
            var experimentModel = JsonConvert.DeserializeObject<ExperimentModel>(fileJson);
            /*
             * TODO:
             * 1 ExperimentConfigAnalysis(ExperimentModel experimentModel)
             */
            // ============ 建议：通过DI获取所有执行器，不要手动new ============
            var executors = new List<ICommandCustomizeExecutor>()
            {
                new StepACentrifugeTempExecutor(),
                new StepBFridgeQueryExecutor(),
                new StepCAxisMoveExecutor(),
            };
            //var manager = new ExperimentManager(executors);
            await _experimentManager.DismantleExecutors(executors);

            // 构造方案（后续改为从experimentModel解析生成List<ExperimentCommand>）
            var scheme = new List<ExperimentCommand>
            {
                new ExperimentCommand
                {
                    CommandName = "StepA_CentrifugeTemp",
                    InputParams = new Dictionary<string, string> { { "TargetTemp", "45" } },
                },
                new ExperimentCommand
                {
                    CommandName = "StepB_FridgeQuery",
                    InputParams = new Dictionary<string, string>
                    {
                        { "Col", "2" },
                        { "Layer", "3" },
                    },
                },
                new ExperimentCommand
                {
                    CommandName = "StepC_AxisMove",
                    InputParams = new Dictionary<string, string>
                    {
                        { "MovePosition", "${BarCodePos}" },
                    },
                },
                new ExperimentCommand
                {
                    CommandName = "StepC_AxisMove",
                    InputParams = new Dictionary<string, string>
                    {
                        { "MovePosition", "${BarCodePos}" },
                    },
                },
                new ExperimentCommand
                {
                    CommandName = "StepC_AxisMove",
                    InputParams = new Dictionary<string, string>
                    {
                        { "MovePosition", "${BarCodePos}" },
                    },
                },
                new ExperimentCommand
                {
                    CommandName = "StepC_AxisMove",
                    InputParams = new Dictionary<string, string>
                    {
                        { "MovePosition", "${BarCodePos}" },
                    },
                },
            };
            _experimentManager.LoadScheme(scheme);

            // 存入全局运行池，使用实验Id关联
            long expId = experimentModel.ExperimentId;
            _runtimePool.SetManager(expId, _experimentManager);
            _logger.LogInformation(
                "加载实验成功，_runtimePool实例Hash=0x{Hash:X8}",
                _runtimePool.GetHashCode().ToString()
            );
            // 打印实例Hash（安全，manager不为null）
            _logger.LogInformation(
                "加载实验成功，ExperimentManager实例Hash=0x{Hash:X8}",
                _experimentManager.GetHashCode().ToString()
            );

            return ResultHelper.Success<object>(
                new
                {
                    FileName = file.FileName,
                    ExperimentId = expId,
                    Message = "实验文件加载成功，可执行启动/暂停操作",
                }
            );
        }
        catch (JsonException jsonEx)
        {
            _logger.LogError(jsonEx, "实验文件{FileName}格式解析失败", file.FileName);
            return ResultHelper.ParamError("文件格式损坏，非标准实验.exp文件");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "加载实验文件{FileName}发生异常", file.FileName);
            return ResultHelper.ServerError($"文件加载异常：{ex.Message}");
        }
    }
    #endregion

    #region 实验运行控制
    [HttpPost("RunExperiment")]
    public async Task<ApiResult<object>> RunExperiment(long experimentId)
    {
        _logger.LogInformation("开始执行启动实验，实验ID：{ExperimentId}", experimentId);
        try
        {
            // 从池中查找实例
            var manager = _runtimePool.GetManager(experimentId);
            _logger.LogInformation(
                "加载实验成功，_runtimePool实例Hash=0x{Hash:X8}",
                _runtimePool.GetHashCode().ToString()
            );
            if (manager == null)
            {
                return ResultHelper.ParamError(
                    $"实验{experimentId}尚未加载，请先上传.exp文件加载实验方案"
                );
            }

            _logger.LogInformation(
                "找到实验管理器实例 Hash=0x{Hash:X8}",
                manager.GetHashCode().ToString()
            );

            // 后台运行
            _ = Task.Run(async () =>
            {
                try
                {
                    await manager.RunAsync();
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("实验[{ExperimentId}]被取消终止", experimentId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "实验[{ExperimentId}]运行异常", experimentId);
                }
            });

            return ResultHelper.Success($"实验 {experimentId} 已成功启动运行");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "启动实验失败，实验ID：{ExperimentId}", experimentId);
            return ResultHelper.ServerError($"启动实验异常：{ex.Message}");
        }
    }

    [HttpPost("PauseExperiment")]
    public async Task<ApiResult<object>> PauseExperiment(long experimentId)
    {
        _logger.LogInformation("执行实验暂停操作，实验ID：{ExperimentId}", experimentId);
        try
        {
            var manager = _runtimePool.GetManager(experimentId);
            if (manager == null)
            {
                return ResultHelper.ParamError($"实验{experimentId}尚未加载");
            }
            manager.Pause();
            return ResultHelper.Success($"实验 {experimentId} 已暂停");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "暂停实验失败，实验ID：{ExperimentId}", experimentId);
            return ResultHelper.ServerError($"暂停实验异常：{ex.Message}");
        }
    }

    [HttpPost("ResumeExperiment")]
    public async Task<ApiResult<object>> ResumeExperiment(long experimentId)
    {
        _logger.LogInformation("执行恢复实验操作，实验ID：{ExperimentId}", experimentId);
        try
        {
            var manager = _runtimePool.GetManager(experimentId);
            if (manager == null)
            {
                return ResultHelper.ParamError($"实验{experimentId}尚未加载");
            }
            manager.Resume();
            return ResultHelper.Success($"实验 {experimentId} 已恢复运行");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "恢复实验失败，实验ID：{ExperimentId}", experimentId);
            return ResultHelper.ServerError($"恢复实验异常：{ex.Message}");
        }
    }

    [HttpPost("StopExperiment")]
    public async Task<ApiResult<object>> StopExperiment(long experimentId)
    {
        _logger.LogInformation("执行终止实验操作，实验ID：{ExperimentId}", experimentId);
        try
        {
            var manager = _runtimePool.GetManager(experimentId);
            if (manager == null)
            {
                return ResultHelper.ParamError($"实验{experimentId}尚未加载");
            }
            manager.Stop();
            // 可选：停止后移除实例
            // _runtimePool.RemoveManager(experimentId);
            return ResultHelper.Success($"实验 {experimentId} 已终止");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "终止实验失败，实验ID：{ExperimentId}", experimentId);
            return ResultHelper.ServerError($"终止实验异常：{ex.Message}");
        }
    }
    #endregion
}
