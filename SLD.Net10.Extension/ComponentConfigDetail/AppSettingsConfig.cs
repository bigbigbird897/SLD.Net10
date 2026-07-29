using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

namespace SLD.Net10.Extension.ComponentConfigDetail
{
    /// <summary>
    /// appsettings.json 全局配置读取操作工具类
    /// 提供多种方式读取Json配置节点、数组配置
    /// </summary>
    public class AppSettingsConfig
    {
        /// <summary>
        /// 全局配置根对象，承载appsettings.json全部配置信息
        /// </summary>
        public static IConfiguration? Configuration { get; set; }

        /// <summary>
        /// 项目根目录路径
        /// </summary>
        private static string? contentPath { get; set; }

        /// <summary>
        /// 构造函数1：通过项目根路径手动加载appsettings.json配置文件
        /// </summary>
        /// <param name="contentPath">应用程序根目录物理路径</param>
        public AppSettingsConfig(string contentPath)
        {
            // 默认读取基础配置文件
            string Path = "appsettings.json";

            // 多环境配置启用示例（开发/测试/生产区分配置文件）
            //Path = $"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json";

            // 保存根路径
            AppSettingsConfig.contentPath = contentPath;

            // 构建配置读取器
            Configuration = new ConfigurationBuilder()
                // 设置配置文件查找根目录
                .SetBasePath(contentPath)
                // 自定义Json配置源
                .Add(new JsonConfigurationSource
                {
                    Path = Path,                  // 配置文件名
                    Optional = false,             // false：配置文件必须存在，不存在则启动报错
                    ReloadOnChange = true         // true：配置文件修改后自动重新加载配置
                })
                .Build();
        }

        /// <summary>
        /// 构造函数2：直接传入DI容器内置IConfiguration实例（推荐Web项目使用）
        /// </summary>
        /// <param name="configuration">框架内置配置实例</param>
        public AppSettingsConfig(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// 多节点拼接读取配置字符串
        /// 示例：AppSettings.app("ConnectionStrings", "Default")
        /// </summary>
        /// <param name="sections">配置层级节点数组，依次传入父节点、子节点</param>
        /// <returns>配置节点对应字符串值，读取失败返回空字符串</returns>
        public static string app(params string[] sections)
        {
            try
            {
                // 判断传入节点参数不为空
                if (sections.Any())
                {
                    // 拼接配置层级分隔符 :
                    string configKey = string.Join(":", sections);
                    return Configuration[configKey];
                }
            }
            catch (Exception)
            {
                // 读取配置异常时捕获，不抛出错误，返回空字符串
            }

            return "";
        }

        /// <summary>
        /// 读取配置中数组类型节点，并自动绑定为泛型集合List<T>
        /// 依赖NuGet包：Microsoft.Extensions.Configuration.Binder
        /// </summary>
        /// <typeparam name="T">数组元素实体类型</typeparam>
        /// <param name="sections">配置层级节点数组</param>
        /// <returns>绑定完成的泛型集合</returns>
        public static List<T> app<T>(params string[] sections)
        {
            List<T> list = new List<T>();
            // 拼接配置key并自动绑定数组数据到list集合
            Configuration.Bind(string.Join(":", sections), list);
            return list;
        }

        /// <summary>
        /// 直接传入完整层级字符串读取配置值
        /// 示例：AppSettings.GetValue("Logging:LogLevel:Default")
        /// </summary>
        /// <param name="sectionsPath">完整配置层级字符串，节点用:分隔</param>
        /// <returns>配置对应字符串，读取异常返回空</returns>
        public static string GetValue(string sectionsPath)
        {
            try
            {
                return Configuration[sectionsPath];
            }
            catch (Exception)
            {
                // 捕获读取异常，静默处理
            }

            return "";
        }
    }
}