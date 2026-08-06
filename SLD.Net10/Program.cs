using Autofac;
using Autofac.Extensions.DependencyInjection;
using AutoMapper;
using ConnectionModbusRtuWithTcp;
using ConnectionModbusRtuWithTcp.Entity;
using ConnectionMqtt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;
using Serilog.Events;
using Serilog.Filters;
using SLD.Net10.ComponentConfig;
using SLD.Net10.Extension.ComponentConfig;
using SLD.Net10.Extension.FrameHelper;
using SLD.Net10.IService;
using SLD.Net10.Repository;
using SLD.Net10.Service;
using SqlSugar;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Security.Claims;
using System.Text;

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

            #region Autofac依赖注入容器配置
            builder
                .Host.UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureContainer<ContainerBuilder>(containerBuilder =>
                {
                    containerBuilder.RegisterModule<AutofacModuleConfig>();
                    containerBuilder.RegisterModule<AutofacPropertityModuleRegConfig>();
                });

            builder.Services.Replace(
                ServiceDescriptor.Transient<IControllerActivator, ServiceBasedControllerActivator>()
            );
            #endregion Autofac依赖注入容器配置

            #region 基础框架服务注册
            builder.Services.AddControllers();

            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc(
                    "实验数据操作",
                    new OpenApiInfo { Title = "实验数据操作接口", Version = "v1" }
                );
                options.SwaggerDoc(
                    "实验定义和操作",
                    new OpenApiInfo { Title = "实验定义和操作接口", Version = "v1" }
                );
                options.SwaggerDoc(
                    "手动控制",
                    new OpenApiInfo { Title = "手动控制接口", Version = "v1" }
                );
                options.SwaggerDoc(
                    "物料定义和操作",
                    new OpenApiInfo { Title = "物料定义和操作接口", Version = "v1" }
                );
                options.SwaggerDoc(
                    "点位标定",
                    new OpenApiInfo { Title = "点位标定接口", Version = "v1" }
                );
                options.SwaggerDoc(
                    "其它后台功能",
                    new OpenApiInfo { Title = "其它后台功能接口", Version = "v1" }
                );

                options.DocInclusionPredicate(
                    (docName, apiDesc) =>
                    {
                        if (!apiDesc.TryGetMethodInfo(out var method))
                            return false;
                        var groupName = method
                            .DeclaringType.GetCustomAttributes(true)
                            .OfType<ApiExplorerSettingsAttribute>()
                            .FirstOrDefault()
                            ?.GroupName;
                        return docName == groupName;
                    }
                );
                // 注册自动Tag分组过滤器（核心）
                options.OperationFilter<SwaggerTagGroupFilterConfig>();
                // 定义 JWT 授权方案
                /*
                 * Bearer 是 HTTP 认证方案（Authentication Scheme）名称
规范全称：Bearer Token（RFC 6750 标准）
                参考：
                1 https://www.youtube.com/watch?v=sj9TLoyOvfw&t=1301s
                就是swagger发送报文的时候，没有携带token
                 */
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = """
                    请输入 Token，格式：{你的 token}
                    示例:eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
                    """
                    /*
                     * """
                    请输入 Token，格式：Bearer {你的 token}
                    示例：Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
                    """
                     */
                });
                options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
                });
            });
            #endregion 基础框架服务注册

            #region AutoMapper 对象映射工具注册
            builder.Services.AddSingleton<IMapper>(serviceProvider =>
            {
                ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
                var mapperConfig = AutoMapperConfig.RegisterMappings(loggerFactory);
                return mapperConfig.CreateMapper();
            });
            #endregion AutoMapper 对象映射工具注册

            #region 全局配置类注册
            builder.Services.AddSingleton(new AppSettingsHelper(builder.Configuration));
            #endregion 全局配置类注册

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
                //.Enrich.FromLogContext()
                // 全量日志：排除SqlSugar的SQL执行日志
                .WriteTo.Logger(lc =>
                    lc
                        // 过滤掉来源为ISqlSugarClient的日志（SQL日志）
                        .Filter.ByExcluding(
                            Matching.WithProperty("SourceContext", "SqlSugar.ISqlSugarClient")
                        )
                        .WriteTo.Async(a =>
                            a.File(
                                path: "logs/all/general-log-.log",
                                rollingInterval: RollingInterval.Day,
                                retainedFileCountLimit: 30,
                                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"
                            )
                        )
                )
                // SqlSugar SQL专用日志：只收集ISqlSugarClient输出的日志
                .WriteTo.Logger(lc =>
                    lc.Filter.ByIncludingOnly(
                            Matching.WithProperty("SourceContext", "SqlSugar.ISqlSugarClient")
                        )
                        .WriteTo.Async(a =>
                            a.File(
                                path: "logs/sql/sql-log-.log",
                                rollingInterval: RollingInterval.Day,
                                retainedFileCountLimit: 30,
                                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] SqlSugar SQL：{Message:lj} {Properties:j}{NewLine}{Exception}"
                            )
                        )
                )
                .CreateLogger();

            builder.Host.UseSerilog();

            #region SqlSugar ORM框架注册（PostgreSQL数据库）
            string? connectionStr = builder.Configuration.GetConnectionString("Default");

            builder.Services.AddSingleton<ISqlSugarClient>(serviceProvider =>
            {
                // 从DI容器获取ILogger，指定日志源名称SqlSugar
                var logger = serviceProvider.GetRequiredService<ILogger<ISqlSugarClient>>();

                var db = new SqlSugarClient(
                    new ConnectionConfig()
                    {
                        ConnectionString = connectionStr,
                        DbType = DbType.PostgreSQL,
                        IsAutoCloseConnection = true,
                        InitKeyType = InitKeyType.Attribute,
                    }
                );

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
                    logger.LogInformation(
                        "执行数据库SQL：{SqlContent}",
                        sql,
                        new { SqlParams = paramDic }
                    );
                };

                // SQL执行异常日志
                db.Aop.OnError = (exp) =>
                {
                    logger.LogError(exp, "SQL执行异常");
                };

                return db;
            });
            #endregion SqlSugar ORM框架注册（PostgreSQL数据库）


            // 注册MQTT服务为单例
            builder.Services.AddSingleton<IMqttClientService, MqttClientService>();

            // 在Program读取appsettings中所有Modbus设备配置
            var modbusDeviceList =
                builder
                    .Configuration.GetSection("ModbusRtuDevices")
                    .Get<List<ModbusRtuWithTcpClientConfig>>()
                ?? new List<ModbusRtuWithTcpClientConfig>();

            // 注册Modbus服务，把预读取好的设备列表传入构造
            builder.Services.AddSingleton<IModbusRtuWithTcpClient>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<ModbusRtuWithTcpClient>>();
                return new ModbusRtuWithTcpClient(logger, modbusDeviceList);
            });

            //builder.Services.AddSingleton<IExperimentRuntimePool, ExperimentRuntimePool>();
            // 读取Jwt配置
            var jwtSection = builder.Configuration.GetSection("Jwt");
            string secretKey = jwtSection["SecretKey"]!;
            byte[] keyBytes = Encoding.UTF8.GetBytes(secretKey);
            // 注册认证服务
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        // 开启验证
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true, // 校验过期时间
                        ValidateIssuerSigningKey = true,

                        ValidIssuer = jwtSection["Issuer"],
                        ValidAudience = jwtSection["Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(keyBytes),

                        // 可选：允许服务器时间偏差（防止客户端时区误差）
                        ClockSkew = TimeSpan.FromSeconds(30)
                    };

                    // JWT认证事件 + Serilog日志输出
                    options.Events = new JwtBearerEvents
                    {
                        // 收到请求，读取Authorization头
                        OnMessageReceived = context =>
                        {
                            context.Request.Headers.TryGetValue("Authorization", out var authHeader);
                            Log.Information("[JWT]收到请求 Authorization={AuthHeader}", authHeader);
                            return Task.CompletedTask;
                        },

                        // Token校验失败（签名错误、iss/aud不匹配、过期等）
                        OnAuthenticationFailed = context =>
                        {
                            Log.Error(context.Exception, "[JWT]Token校验失败");
                            return Task.CompletedTask;
                        },

                        // Token校验成功，可以拿到用户信息
                        OnTokenValidated = context =>
                        {
                            var userId = context.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
                            var userName = context.Principal.FindFirstValue(ClaimTypes.Name);
                            // ✅多角色，取全部Role Claim
                            var roleClaims = context.Principal.FindAll(ClaimTypes.Role);
                            var userRoles = roleClaims.Select(c => c.Value).ToList();
                            Log.Information("[JWT]认证成功，UserId={UserId},UserName={UserName},Roles={Roles}",
        userId, userName, string.Join(",", userRoles));
                            return Task.CompletedTask;
                        },

                        // 需要返回401 Unauthorized时触发
                        OnChallenge = context =>
                        {
                            Log.Warning("[JWT]鉴权质询，即将返回401，Error={Error},Desc={Desc}", context.Error, context.ErrorDescription);
                            return Task.CompletedTask;
                        }
                    };
                });

            builder.Services.AddScoped<JwtHelper>();
            //注入仓储
            builder.Services.AddScoped(typeof(Repository<>));


            var app = builder.Build();

            // 顺序很重要！先认证，再授权
            app.UseAuthentication();
            app.UseAuthorization();

            // CodeFirst自动建库建表
            var dbClient = app.Services.GetRequiredService<ISqlSugarClient>();
            dbClient.InitCodeFirst();

            #region 请求管道中间件配置
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(opt =>
                {
                    opt.SwaggerEndpoint("/swagger/实验数据操作/swagger.json", "实验数据操作接口");
                    opt.SwaggerEndpoint(
                        "/swagger/实验定义和操作/swagger.json",
                        "实验定义和操作接口"
                    );
                    opt.SwaggerEndpoint("/swagger/手动控制/swagger.json", "手动控制接口");
                    opt.SwaggerEndpoint(
                        "/swagger/物料定义和操作/swagger.json",
                        "物料定义和操作接口"
                    );
                    opt.SwaggerEndpoint("/swagger/点位标定/swagger.json", "点位标定接口");
                    opt.SwaggerEndpoint("/swagger/其它后台功能/swagger.json", "其它后台功能接口");
                    opt.RoutePrefix = "swagger";
                });
            }
            // 开启默认文档（访问根路径时自动返回index.html）
            app.UseDefaultFiles(
                new DefaultFilesOptions { DefaultFileNames = new List<string> { "index.html" } }
            );
            // 开启wwwroot静态文件访问
            app.UseStaticFiles();
            // SPA路由回退：未匹配的请求返回index.html，交给前端路由处理
            app.MapFallbackToFile("index.html");
            app.UseHttpsRedirection();
            app.MapControllers();
            #endregion 请求管道中间件配置

            // 程序启动时初始化所有MQTT连接
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

            // 启动时打印已加载的Modbus设备
            using (var scope = app.Services.CreateScope())
            {
                var modbusClient =
                    scope.ServiceProvider.GetRequiredService<IModbusRtuWithTcpClient>();
                var deviceCodes = modbusClient.GetAllDeviceCodes();
                Log.Logger.Information(
                    "已加载Modbus设备列表：{Devices}",
                    string.Join(",", deviceCodes)
                );
            }

            // 返回构建完成的Host，外部可调用Run启动服务
            return app;
        }

        //保留原有Main方法（兼容单独启动后端场景，可选保留）
        /*
         * public static void Main(string[] args)
        {
            var host = BuildWebHost(args);
            host.Run();
        }
         */
    }
}
