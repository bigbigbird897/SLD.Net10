using Autofac;
using Autofac.Extensions.DependencyInjection;
using AutoMapper;
using ConnectionMqtt;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.OpenApi;
using Serilog;
using Serilog.Events;
using Serilog.Filters;
using SLD.Net10.ComponentConfigDetail;
using SLD.Net10.Extension.ComponentConfigDetail;
using SqlSugar;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SLD.Net10
{
    /// <summary>
    /// 程序入口类，提取BuildWebHost供WPF调用
    /// </summary>
    public class Program
    {
        // 提取公共静态方法：构建并返回IHost，给WPF调用
        public static async Task<IHost> BuildWebHostAsync(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            #region 一、Autofac依赖注入容器配置

            builder.Host
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureContainer<ContainerBuilder>(containerBuilder =>
                {
                    containerBuilder.RegisterModule<AutofacModuleConfig>();
                    containerBuilder.RegisterModule<AutofacPropertityModuleReg>();
                });

            builder.Services.Replace(ServiceDescriptor.Transient<IControllerActivator, ServiceBasedControllerActivator>());

            #endregion 一、Autofac依赖注入容器配置

            #region 二、基础框架服务注册

            builder.Services.AddControllers();

            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("实验数据操作", new OpenApiInfo { Title = "实验数据操作接口", Version = "v1" });
                options.SwaggerDoc("实验定义和操作", new OpenApiInfo { Title = "实验定义和操作接口", Version = "v1" });
                options.SwaggerDoc("手动控制", new OpenApiInfo { Title = "手动控制接口", Version = "v1" });
                options.SwaggerDoc("物料定义和操作", new OpenApiInfo { Title = "物料定义和操作接口", Version = "v1" });
                options.SwaggerDoc("点位标定", new OpenApiInfo { Title = "点位标定接口", Version = "v1" });
                options.SwaggerDoc("其它后台功能", new OpenApiInfo { Title = "其它后台功能接口", Version = "v1" });

                options.DocInclusionPredicate((docName, apiDesc) =>
                                {
                                    if (!apiDesc.TryGetMethodInfo(out var method)) return false;
                                    var groupName = method.DeclaringType.GetCustomAttributes(true)
                                        .OfType<ApiExplorerSettingsAttribute>()
                                        .FirstOrDefault()?.GroupName;
                                    return docName == groupName;
                                });
            });

            #endregion 二、基础框架服务注册

            #region 三、AutoMapper 对象映射工具注册

            builder.Services.AddSingleton<IMapper>(serviceProvider =>
            {
                ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
                var mapperConfig = AutoMapperConfig.RegisterMappings(loggerFactory);
                return mapperConfig.CreateMapper();
            });

            #endregion 三、AutoMapper 对象映射工具注册

            #region 四、全局配置类注册

            builder.Services.AddSingleton(new AppSettingsConfig(builder.Configuration));

            #endregion 四、全局配置类注册

            /*
             * 参考文档：
             * 1 https://www.cnblogs.com/TangQF/articles/18976094
             * 2 Serilog 完整使用指南 + 日志等级统计方案（适配你现有.NET8/PostgreSQL+SqlSugar项目）
             */
            Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .MinimumLevel.Override("SqlSugar", LogEventLevel.Information)
    .Enrich.FromLogContext()
    // 1) 全量日志：排除SqlSugar的SQL执行日志
    .WriteTo.Logger(lc => lc
        // 过滤掉来源为ISqlSugarClient的日志（SQL日志）
        .Filter.ByExcluding(Matching.WithProperty("SourceContext", "SqlSugar.ISqlSugarClient"))
        .WriteTo.Async(a => a.File(
            path: "logs/all/general-log-.log",
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 30,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"
        ))
    )
    // 2) SqlSugar SQL专用日志：只收集ISqlSugarClient输出的日志
    .WriteTo.Logger(lc => lc
        .Filter.ByIncludingOnly(Matching.WithProperty("SourceContext", "SqlSugar.ISqlSugarClient"))
        .WriteTo.Async(a => a.File(
            path: "logs/sql/sql-log-.log",
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 30,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] SqlSugar SQL：{Message:lj} {Properties:j}{NewLine}{Exception}"
        ))
    )
    .CreateLogger();

            builder.Host.UseSerilog();

            #region 五、SqlSugar ORM框架注册（PostgreSQL数据库）

            string? connectionStr = builder.Configuration.GetConnectionString("Default");

            builder.Services.AddSingleton<ISqlSugarClient>(serviceProvider =>
            {
                // 从DI容器获取ILogger，指定日志源名称SqlSugar
                var logger = serviceProvider.GetRequiredService<ILogger<ISqlSugarClient>>();

                var db = new SqlSugarClient(new ConnectionConfig()
                {
                    ConnectionString = connectionStr,
                    DbType = DbType.PostgreSQL,
                    IsAutoCloseConnection = true,
                    InitKeyType = InitKeyType.Attribute
                });

                // SQL执行前拦截
                db.Aop.OnLogExecuting = (sql, pars) =>
                {
                    // 拼接SQL参数
                    var paramDic = new Dictionary<string, object>();
                    foreach (var p in pars)
                    {
                        paramDic.Add(p.ParameterName, p.Value);
                    }

                    // 使用ILogger打印，自动带上SourceContext=SqlSugar
                    logger.LogInformation("执行数据库SQL：{SqlContent}", sql, new { SqlParams = paramDic });
                };

                // SQL执行异常日志
                db.Aop.OnError = (exp) =>
                {
                    logger.LogError(exp, "SQL执行异常");
                };

                return db;
            });

            #endregion 五、SqlSugar ORM框架注册（PostgreSQL数据库）


            // 1. 注册MQTT服务为单例
            builder.Services.AddSingleton<IMqttClientService, MqttClientService>();

            var app = builder.Build();

            // CodeFirst自动建库建表
            var dbClient = app.Services.GetRequiredService<ISqlSugarClient>();
            dbClient.InitCodeFirst();

            #region 八、请求管道中间件配置

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(opt =>
                {
                    opt.SwaggerEndpoint("/swagger/实验数据操作/swagger.json", "实验数据操作接口");
                    opt.SwaggerEndpoint("/swagger/实验定义和操作/swagger.json", "实验定义和操作接口");
                    opt.SwaggerEndpoint("/swagger/手动控制/swagger.json", "手动控制接口");
                    opt.SwaggerEndpoint("/swagger/物料定义和操作/swagger.json", "物料定义和操作接口");
                    opt.SwaggerEndpoint("/swagger/点位标定/swagger.json", "点位标定接口");
                    opt.SwaggerEndpoint("/swagger/其它后台功能/swagger.json", "其它后台功能接口");
                    opt.RoutePrefix = "swagger";
                });
            }
            // 开启默认文档（访问根路径时自动返回index.html）
            app.UseDefaultFiles(new DefaultFilesOptions
            {
                DefaultFileNames = new List<string> { "index.html" }
            });
            // 开启wwwroot静态文件访问（必须加）
            app.UseStaticFiles();
            // SPA路由回退：未匹配的请求返回index.html，交给前端路由处理
            app.MapFallbackToFile("index.html");
            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            #endregion 八、请求管道中间件配置

            // 2. 程序启动时初始化所有MQTT连接
            using (var scope = app.Services.CreateScope())
            {
                var mqttService = scope.ServiceProvider.GetRequiredService<IMqttClientService>();
                await mqttService.InitAllClientsAsync();
            }

            // 程序退出时统一断开MQTT
            app.Lifetime.ApplicationStopping.Register(async () =>
            {
                var mqttService = app.Services.GetRequiredService<IMqttClientService>();
                await mqttService.DisconnectAllAsync();
            });


            // 返回构建完成的Host，外部可调用Run启动服务
            return app;
        }

        // 保留原有Main方法（兼容单独启动后端场景，可选保留）
        //public static void Main(string[] args)
        //{
        //    var host = BuildWebHost(args);
        //    host.Run();
        //}
    }
}