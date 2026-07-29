using Autofac;
using Autofac.Extensions.DependencyInjection;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Serilog;
using Serilog.Events;
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
        public static IHost BuildWebHost(string[] args)
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
            #endregion

            #region 二、基础框架服务注册
            builder.Services.AddControllers();

            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("实验数据操作", new OpenApiInfo { Title = "实验数据操作接口", Version = "v1" });
                options.SwaggerDoc("实验定义和操作", new OpenApiInfo { Title = "实验定义和操作接口", Version = "v1" });
                options.SwaggerDoc("手动控制", new OpenApiInfo { Title = "手动控制接口", Version = "v1" });
                options.SwaggerDoc("物料定义和操作", new OpenApiInfo { Title = "物料定义和操作接口", Version = "v1" });
                options.SwaggerDoc("点位标定", new OpenApiInfo { Title = "点位标定接口", Version = "v1" });

                options.DocInclusionPredicate((docName, apiDesc) =>
                {
                    if (!apiDesc.TryGetMethodInfo(out var method)) return false;
                    var groupName = method.DeclaringType.GetCustomAttributes(true)
                        .OfType<ApiExplorerSettingsAttribute>()
                        .FirstOrDefault()?.GroupName;
                    return docName == groupName;
                });
            });
            #endregion

            #region 三、AutoMapper 对象映射工具注册
            builder.Services.AddSingleton<IMapper>(serviceProvider =>
            {
                ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
                var mapperConfig = AutoMapperConfig.RegisterMappings(loggerFactory);
                return mapperConfig.CreateMapper();
            });
            #endregion

            #region 四、全局配置类注册
            builder.Services.AddSingleton(new AppSettingsConfig(builder.Configuration));
            #endregion

            #region 五、SqlSugar ORM框架注册（PostgreSQL数据库）
            string? connectionStr = builder.Configuration.GetConnectionString("Default");

            builder.Services.AddSingleton<ISqlSugarClient>(sugar =>
            {
                var db = new SqlSugarClient(new ConnectionConfig()
                {
                    ConnectionString = connectionStr,
                    DbType = DbType.PostgreSQL,
                    IsAutoCloseConnection = true,
                    InitKeyType = InitKeyType.Attribute
                });

                db.Aop.OnLogExecuting = (sql, pars) =>
                {
                    //Console.WriteLine($"执行SQL：{sql}");
                };

                return db;
            });
            #endregion

            #region 六、Serilog 全局日志框架初始化
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File(
                    path: "logs/log-.txt",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
                )
                .CreateLogger();

            builder.Host.UseSerilog();
            #endregion

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
            #endregion

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
