using Castle.DynamicProxy;
using Newtonsoft.Json;
using SLD.Net10.Extension.Model;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SLD.Net10.Extension.ComponentConfigDetail
{
    /// <summary>
    /// 业务服务层全局AOP拦截器
    /// 实现Castle动态代理IInterceptor接口，统一拦截Service接口方法
    /// 功能：记录入参、出参、执行耗时、捕获全局异常日志，同步支持同步/异步Task方法
    /// </summary>
    public class ServiceAOPConfig : IInterceptor
    {
        /// <summary>
        /// 拦截器核心入口方法，所有被代理的Service方法都会进入该方法
        /// </summary>
        /// <param name="invocation">被拦截方法的上下文对象，包含方法名、入参、返回值等全部信息</param>
        public void Intercept(IInvocation invocation)
        {
            // 序列化方法入参JSON字符串
            string requestParamJson;
            try
            {
                // 将方法所有入参数组序列化为JSON
                requestParamJson = JsonConvert.SerializeObject(invocation.Arguments);
            }
            catch (Exception ex)
            {
                // 序列化失败兜底文本（常见于参数包含lambda表达式、循环引用实体）
                requestParamJson = $"参数序列化失败，存在无法序列化对象：{ex}";
            }

            // 记录方法执行开始时间
            DateTime executeStartTime = DateTime.Now;

            // 初始化AOP日志实体，填充请求基础信息
            ServiceAOPLogInfo aopLog = new ServiceAOPLogInfo
            {
                RequestTime = executeStartTime.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                OpUserName = "", // 预留操作人字段，可从HttpContext/登录上下文赋值
                RequestMethodName = invocation.Method.Name,
                // 拼接所有入参的字符串描述
                RequestParamsName = string.Join(", ", invocation.Arguments.Select(arg => (arg ?? "").ToString())),
                ResponseJsonData = requestParamJson
            };

            try
            {
                // 执行被拦截的原始业务方法
                invocation.Proceed();

                // 判断当前拦截的方法是否为异步Task方法
                if (IsAsyncMethod(invocation.Method))
                {
                    #region 异步方法处理逻辑（Task / Task<T>）
                    // 无返回值异步方法 Task
                    if (invocation.Method.ReturnType == typeof(Task))
                    {
                        invocation.ReturnValue = InternalAsyncHelper.AwaitTaskWithPostActionAndFinally(
                            (Task)invocation.ReturnValue,
                            // 方法执行成功后回调
                            async () => await SuccessAction(invocation, aopLog, executeStartTime),
                            // 方法抛出异常时回调
                            ex => LogEx(ex, aopLog));
                    }
                    // 有返回值异步方法 Task<TResult>
                    else
                    {
                        invocation.ReturnValue = InternalAsyncHelper.CallAwaitTaskWithPostActionAndFinallyAndGetResult(
                            invocation.Method.ReturnType.GenericTypeArguments[0],
                            invocation.ReturnValue,
                            // 成功回调，传入异步方法返回结果
                            async (resultObj) => await SuccessAction(invocation, aopLog, executeStartTime, resultObj),
                            // 异常回调
                            ex => LogEx(ex, aopLog));
                    }
                    #endregion
                }
                else
                {
                    #region 同步方法处理逻辑
                    string responseResultJson;
                    try
                    {
                        // 序列化同步方法返回值
                        responseResultJson = JsonConvert.SerializeObject(invocation.ReturnValue);
                    }
                    catch (Exception ex)
                    {
                        responseResultJson = $"返回值序列化失败，存在无法序列化对象：{ex}";
                    }

                    // 计算执行结束时间与耗时
                    DateTime executeEndTime = DateTime.Now;
                    long executeIntervalMs = (executeEndTime - executeStartTime).Milliseconds;

                    // 填充返回信息至日志实体
                    aopLog.ResponseTime = executeEndTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    aopLog.ResponseIntervalTime = $"{executeIntervalMs}ms";
                    aopLog.ResponseJsonData = responseResultJson;

                    // 控制台打印完整AOP日志（生产环境可替换为Serilog写入文件/数据库）
                    //Console.WriteLine(JsonConvert.SerializeObject(aopLog));
                    #endregion
                }
            }
            catch (Exception ex)
            {
                // 捕获业务方法同步异常，记录日志后重新抛出，不吞异常
                LogEx(ex, aopLog);
                throw;
            }
        }

        /// <summary>
        /// 异步方法执行成功后的回调，统一记录异步返回结果、耗时并打印日志
        /// </summary>
        /// <param name="invocation">方法上下文</param>
        /// <param name="aopLog">日志实体</param>
        /// <param name="startTime">方法开始时间</param>
        /// <param name="resultObj">异步Task返回值，无返回值异步方法传null</param>
        /// <returns></returns>
        private async Task SuccessAction(IInvocation invocation, ServiceAOPLogInfo aopLog, DateTime startTime, object resultObj = null)
        {
            DateTime executeEndTime = DateTime.Now;
            long executeIntervalMs = (executeEndTime - startTime).Milliseconds;

            // 填充异步返回数据、结束时间、耗时
            aopLog.ResponseTime = executeEndTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
            aopLog.ResponseIntervalTime = $"{executeIntervalMs}ms";
            aopLog.ResponseJsonData = JsonConvert.SerializeObject(resultObj);

            // 异步打印日志，不阻塞主线程
            await Task.Run(() =>
            {
                //Console.WriteLine($"执行成功-->{JsonConvert.SerializeObject(aopLog)}");
            });
        }

        /// <summary>
        /// 全局异常日志打印方法，业务方法抛出异常时执行
        /// </summary>
        /// <param name="ex">捕获的异常对象</param>
        /// <param name="aopLog">当前方法的日志载体</param>
        private void LogEx(Exception ex, ServiceAOPLogInfo aopLog)
        {
            if (ex == null) return;
            // 控制台输出异常日志（生产替换为日志框架）
            //Console.WriteLine($"error!!!:{ex.Message} 完整日志信息：{JsonConvert.SerializeObject(aopLog)}");
        }

        /// <summary>
        /// 判断当前方法是否为异步Task方法
        /// 两种异步：无返回Task、带返回值Task<>
        /// </summary>
        /// <param name="method">待判断的方法元数据</param>
        /// <returns>true=异步方法，false=同步方法</returns>
        public static bool IsAsyncMethod(MethodInfo method)
        {
            return method.ReturnType == typeof(Task)
                   || (method.ReturnType.IsGenericType
                       && method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>));
        }
    }

    /// <summary>
    /// 异步Task方法辅助工具静态类
    /// 封装Task/Task<T>等待、后置回调、异常捕获通用逻辑
    /// 采用反射泛型调用，统一兼容有无返回值的异步方法
    /// </summary>
    internal static class InternalAsyncHelper
    {
        /// <summary>
        /// 处理无返回值异步Task方法
        /// </summary>
        /// <param name="actualReturnValue">原始业务Task对象</param>
        /// <param name="postAction">执行成功后的后置回调</param>
        /// <param name="finalAction">无论成功失败都会执行的异常回调</param>
        /// <returns></returns>
        public static async Task AwaitTaskWithPostActionAndFinally(Task actualReturnValue, Func<Task> postAction, Action<Exception> finalAction)
        {
            Exception exception = null;
            try
            {
                // 等待原始业务异步方法执行完成
                await actualReturnValue;
                // 执行成功后置日志记录逻辑
                await postAction();
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            finally
            {
                // 异常回调统一记录错误日志
                finalAction(exception);
            }
        }

        /// <summary>
        /// 处理带返回值异步Task<T>方法，返回原始业务返回值
        /// </summary>
        /// <typeparam name="T">异步返回值类型</typeparam>
        /// <param name="actualReturnValue">原始业务Task<T>对象</param>
        /// <param name="postAction">执行成功后置回调，传入返回值</param>
        /// <param name="finalAction">异常捕获回调</param>
        /// <returns>业务方法原始返回值</returns>
        public static async Task<T> AwaitTaskWithPostActionAndFinallyAndGetResult<T>(Task<T> actualReturnValue, Func<object, Task> postAction, Action<Exception> finalAction)
        {
            Exception exception = null;
            try
            {
                // 等待异步方法并获取返回值
                var result = await actualReturnValue;
                // 执行成功日志回调
                await postAction(result);
                return result;
            }
            catch (Exception ex)
            {
                exception = ex;
                throw;
            }
            finally
            {
                finalAction(exception);
            }
        }

        /// <summary>
        /// 反射入口：动态调用泛型异步处理方法，兼容任意Task<T>返回类型
        /// </summary>
        /// <param name="taskReturnType">异步返回值泛型类型</param>
        /// <param name="actualReturnValue">原始代理返回的Task对象</param>
        /// <param name="action">成功后置回调</param>
        /// <param name="finalAction">异常回调</param>
        /// <returns>处理完成后的Task对象，赋值给invocation.ReturnValue</returns>
        public static object CallAwaitTaskWithPostActionAndFinallyAndGetResult(Type taskReturnType, object actualReturnValue, Func<object, Task> action, Action<Exception> finalAction)
        {
            // 反射获取泛型方法，传入具体返回值类型并执行调用
            return typeof(InternalAsyncHelper)
                .GetMethod("AwaitTaskWithPostActionAndFinallyAndGetResult", BindingFlags.Public | BindingFlags.Static)
                .MakeGenericMethod(taskReturnType)
                .Invoke(null, new object[] { actualReturnValue, action, finalAction });
        }
    }
}
