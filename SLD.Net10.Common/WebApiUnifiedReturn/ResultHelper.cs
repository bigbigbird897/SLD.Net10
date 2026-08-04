using SLD.Net10.Common.WebApiUnifiedReturn;

namespace SLD.Net10.Common.WebApiUnifiedReturn;

/// <summary>
/// 参考文档：
/// 1 ASP.NET Core 工业自动化项目统一JSON规范方案
/// </summary>
public static class ResultHelper
{
    // 成功无数据
    public static ApiResult<object> Success(string msg = "操作成功")
        => new ApiResult<object> { Code = 200, Msg = msg, Data = null };

    // 成功带数据
    public static ApiResult<T> Success<T>(T data, string msg = "操作成功")
        => new ApiResult<T> { Code = 200, Msg = msg, Data = data };

    // 参数错误
    public static ApiResult<object> ParamError(string msg, object ext = null)
        => new ApiResult<object> { Code = 400, Msg = msg, Ext = ext };

    // PLC/Modbus设备异常
    public static ApiResult<object> DeviceError(int code, string msg, object ext = null)
        => new ApiResult<object> { Code = code, Msg = msg, Ext = ext };

    // 系统内部错误
    public static ApiResult<object> ServerError(string msg = "服务器异常")
        => new ApiResult<object> { Code = 500, Msg = msg };
    // 成功带数据
    public static ApiResult<T> ServerError<T>(T data, string msg = "服务器异常")
        => new ApiResult<T> { Code = 500, Msg = msg, Data = data };
}