//using Serilog;
//using System.Windows;

//namespace SLD.Net10;

//public partial class App : System.Windows.Application
//{
//    private IHost? _webHost;
//    private CancellationTokenSource _cts = new();

//    /// <summary>
//    /// Web服务启动等待超时时间(毫秒)
//    /// </summary>
//    private const int ServiceStartTimeoutMs = 3000;

//    protected override void OnStartup(StartupEventArgs e)
//    {
//        base.OnStartup(e);

//        try
//        {
//            var args = e.Args;
//            Log.Information("开始初始化Web后端服务");

//            // 构建完整Web宿主（复用Program所有DI、数据库、Swagger配置）
//            _webHost = (IHost?)Program.BuildWebHostAsync(args);

//            // 异步后台启动Web服务，支持取消
//            Task.Run(async () =>
//            {
//                try
//                {
//                    await _webHost.RunAsync(_cts.Token);
//                }
//                catch (TaskCanceledException)
//                {
//                    Log.Information("Web服务已被手动停止");
//                }
//                catch (Exception ex)
//                {
//                    Log.Fatal(ex, "Web服务运行时发生致命异常");
//                }
//            }, _cts.Token);

//            // 替换硬编码Sleep：循环检测服务是否监听端口，超时兜底
//            WaitWebServiceReady(_cts.Token);

//            Log.Information("Web后端服务启动完成，打开Swagger窗口");

//            // 创建主窗口并绑定关闭事件
//            var mainWin = new MainWindow();
//            mainWin.Closed += MainWindow_Closed;
//            mainWin.Show();
//        }
//        catch (Exception ex)
//        {
//            Log.Fatal(ex, "程序启动失败，即将退出");
//            System.Windows.MessageBox.Show($"程序启动异常：{ex.Message}", "启动失败", MessageBoxButton.OK, MessageBoxImage.Error);
//            Shutdown();
//        }
//    }

//    /// <summary>
//    /// 循环等待Web服务就绪，替代固定延时
//    /// </summary>
//    private void WaitWebServiceReady(CancellationToken token)
//    {
//        var startTime = DateTimeOffset.UtcNow;
//        while ((DateTimeOffset.UtcNow - startTime).TotalMilliseconds < ServiceStartTimeoutMs)
//        {
//            if (token.IsCancellationRequested) break;

//            // Kestrel启动后IServer会被注册进DI，存在即代表服务就绪
//            var server = _webHost?.Services.GetService<Microsoft.AspNetCore.Hosting.Server.IServer>();
//            if (server != null)
//                return;

//            Thread.Sleep(100);
//        }
//        Log.Warning($"Web服务启动超时({ServiceStartTimeoutMs}ms)，窗口可能无法加载Swagger");
//    }

//    /// <summary>
//    /// 主窗口关闭事件：直接关闭整个程序
//    /// </summary>
//    private void MainWindow_Closed(object sender, EventArgs e)
//    {
//        Shutdown();
//    }

//    /// <summary>
//    /// 程序整体退出，优雅停止Web服务、释放资源
//    /// </summary>
//    protected override async void OnExit(ExitEventArgs e)
//    {
//        Log.Information("程序正在退出，停止Web服务");
//        _cts.Cancel();

//        if (_webHost != null)
//        {
//            // 使用异步等待，不阻塞UI线程
//            await _webHost.StopAsync(TimeSpan.FromSeconds(3));
//            _webHost.Dispose();
//        }

//        _cts.Dispose();
//        Log.CloseAndFlush();

//        base.OnExit(e);
//    }
//}

using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace SLD.Net10;

public partial class App : System.Windows.Application
{
    private IHost? _webHost;
    private readonly CancellationTokenSource _cts = new();

    /// <summary>
    /// Web服务启动等待超时时间(毫秒)
    /// </summary>
    private const int ServiceStartTimeoutMs = 3000;
    /// <summary>
    /// Kestrel默认监听端口，用于检测服务是否真正监听成功
    /// 若你的项目改了端口同步修改此处
    /// </summary>
    private const int WebListenPort = 5000;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            string[] args = e.Args;
            Log.Information("开始初始化Web后端服务");

            // 1. 异步构建Web宿主（修复原代码直接强转Task<IHost>的致命错误）
            _webHost = await Program.BuildWebHostAsync(args);

            // 2. 后台线程运行Web服务，不阻塞WPF主线程
            _ = Task.Run(async () =>
            {
                try
                {
                    await _webHost.RunAsync(_cts.Token);
                }
                catch (TaskCanceledException)
                {
                    Log.Information("Web服务已收到停止信号，正常退出");
                }
                catch (Exception ex)
                {
                    Log.Fatal(ex, "Web服务运行过程中发生致命异常");
                }
            }, _cts.Token);

            // 3. 循环等待Web端口监听就绪，替代固定Sleep
            bool serviceReady = WaitWebServiceReady(_cts.Token);
            if (serviceReady)
                Log.Information("Web后端服务启动完成，可正常访问Swagger");
            else
                Log.Warning($"Web服务启动超时({ServiceStartTimeoutMs}ms)，接口/Swagger可能无法访问");

            // 4. 打开主窗口
            var mainWin = new MainWindow();
            mainWin.Closed += MainWindow_Closed;
            mainWin.Show();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "WPF程序启动初始化失败");
            System.Windows.MessageBox.Show($"程序启动异常：{ex.Message}", "启动失败", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown();
        }
    }

    /// <summary>
    /// 循环检测端口是否监听，判断Web服务真正就绪
    /// </summary>
    private bool WaitWebServiceReady(CancellationToken token)
    {
        DateTime startTime = DateTime.UtcNow;
        TimeSpan timeout = TimeSpan.FromMilliseconds(ServiceStartTimeoutMs);

        while (DateTime.UtcNow - startTime < timeout)
        {
            if (token.IsCancellationRequested)
                break;

            // 检测本机端口是否被监听
            if (IsPortListening(WebListenPort))
                return true;

            Thread.Sleep(100);
        }
        return false;
    }

    /// <summary>
    /// 检测本机指定端口是否处于监听状态
    /// </summary>
    private bool IsPortListening(int port)
    {
        try
        {
            using TcpClient client = new();
            client.Client.Blocking = true;
            var connectTask = client.ConnectAsync(IPAddress.Loopback, port);
            return connectTask.Wait(100) && client.Connected;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 主窗口关闭事件：触发程序整体退出
    /// </summary>
    private void MainWindow_Closed(object sender, EventArgs e)
    {
        Shutdown();
    }

    /// <summary>
    /// 程序整体退出，优雅停止Web服务、释放资源
    /// </summary>
    protected override async void OnExit(ExitEventArgs e)
    {
        Log.Information("程序正在退出，准备停止Web后端服务");
        _cts.Cancel();

        if (_webHost != null)
        {
            try
            {
                // 优雅停止，最多等待3秒
                await _webHost.StopAsync(TimeSpan.FromSeconds(3));
            }
            catch (Exception ex)
            {
                Log.Error(ex, "停止Web服务时发生异常");
            }
            finally
            {
                _webHost.Dispose();
                _webHost = null;
            }
        }

        _cts.Dispose();
        // 强制刷新所有日志缓冲区，防止丢失退出日志
        Log.CloseAndFlush();
        Log.Information("程序资源释放完成，正常退出");

        base.OnExit(e);
    }
}
