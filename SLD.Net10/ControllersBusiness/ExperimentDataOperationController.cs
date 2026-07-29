using Microsoft.AspNetCore.Mvc;

/// <summary>
/// 实验数据操作
/// </summary>
[ApiController]
[Route("[controller]/[action]")]
[ApiExplorerSettings(GroupName = "实验数据操作")]
public class ExperimentDataOperationController : ControllerBase
{
    #region 1. 查询相关接口
    /// <summary>
    /// 根据实验Id查询实验明细数据
    /// </summary>
    /// <param name="experimentId">实验主键ID</param>
    /// <returns>实验数据明细集合</returns>
    [HttpGet("GetListByExperimentId")]
    public IActionResult GetListByExperimentId(long experimentId)
    {
        return Ok(new ResultModel()
        {
            Success = true,
            Msg = "查询成功",
            Data = new List<object>()
        });
    }

    /// <summary>
    /// 分页查询全部实验数据
    /// </summary>
    /// <param name="pageIndex">页码</param>
    /// <param name="pageSize">每页条数</param>
    /// <returns>分页实验数据</returns>
    [HttpGet("GetPageList")]
    public IActionResult GetPageList(int pageIndex = 1, int pageSize = 20)
    {
        return Ok(new ResultModel()
        {
            Success = true,
            Msg = "分页查询成功",
            Data = new { Total = 100, Rows = new List<object>() }
        });
    }
    #endregion

    #region 2. 新增、修改接口
    /// <summary>
    /// 新增一条实验采集数据
    /// </summary>
    /// <param name="model">实验数据实体</param>
    /// <returns>新增数据主键Id</returns>
    [HttpPost("AddData")]
    public IActionResult AddData([FromBody] ExperimentDataModel model)
    {
        long newId = 10001;
        return Ok(new ResultModel()
        {
            Success = true,
            Msg = "新增实验数据成功",
            Data = newId
        });
    }

    /// <summary>
    /// 更新实验数据记录
    /// </summary>
    /// <param name="model">待更新数据实体</param>
    /// <returns>更新结果</returns>
    [HttpPut("UpdateData")]
    public IActionResult UpdateData([FromBody] ExperimentDataModel model)
    {
        return Ok(new ResultModel()
        {
            Success = true,
            Msg = "数据更新成功"
        });
    }
    #endregion

    #region 3. 删除、导出接口
    /// <summary>
    /// 根据Id删除单条实验数据
    /// </summary>
    /// <param name="id">数据主键ID</param>
    /// <returns>删除结果</returns>
    [HttpDelete("DeleteById/{id}")]
    public IActionResult DeleteById(long id)
    {
        return Ok(new ResultModel()
        {
            Success = true,
            Msg = $"Id={id} 的实验数据删除完成"
        });
    }

    /// <summary>
    /// 导出实验数据Excel文件
    /// </summary>
    /// <param name="experimentId">筛选实验ID</param>
    /// <returns>文件流</returns>
    [HttpGet("ExportExcel")]
    public IActionResult ExportExcel(long experimentId)
    {
        byte[] emptyFile = Array.Empty<byte>();
        return File(emptyFile, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "实验数据.xlsx");
    }
    #endregion
}

#region 配套简易实体、统一返回模型
/// <summary>
/// 实验数据实体
/// </summary>
public class ExperimentDataModel
{
    /// <summary>
    /// 主键
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 所属实验ID
    /// </summary>
    public long ExperimentId { get; set; }

    /// <summary>
    /// 点位名称
    /// </summary>
    public string PositionName { get; set; }

    /// <summary>
    /// 采集数值
    /// </summary>
    public decimal CollectValue { get; set; }

    /// <summary>
    /// 采集时间
    /// </summary>
    public DateTime CollectTime { get; set; }
}

/// <summary>
/// 接口统一返回格式
/// </summary>
public class ResultModel
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 提示信息
    /// </summary>
    public string Msg { get; set; }

    /// <summary>
    /// 返回业务数据
    /// </summary>
    public object Data { get; set; }
}
#endregion
