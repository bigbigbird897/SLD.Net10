using Autofac;
using Autofac.Extras.DynamicProxy;
using SLD.Net10.IService;
using SLD.Net10.Repository;
using SLD.Net10.Service;
using System.Reflection;

namespace SLD.Net10.Extension.ComponentConfigDetail
{
    /// <summary>
    /// Autofac批量注册模块
    /// 统一注册仓储层、业务服务层，开启AOP拦截、属性自动注入
    /// </summary>
    public class AutofacModuleConfig : Autofac.Module
    {
        /*
        开发调试笔记：
        1、DI容器相关报错排查：确认当前生效容器、完整异常堆栈定位问题
        2、项目接入Autofac标准三步配置流程
        3、依赖注入生命周期调试方案：通过HashCode对比实例，解决Controller中实例复用/新建异常
        4、支持控制器、服务类属性注入，无需全部构造函数传参
        */

        /// <summary>
        /// 重写Autofac模块加载方法，所有批量注册逻辑写在此处
        /// </summary>
        /// <param name="builder">Autofac容器构建器</param>
        protected override void Load(ContainerBuilder builder)
        {
            // 获取程序运行根目录
            var basePath = AppContext.BaseDirectory;

            // 拼接业务服务层dll物理路径
            var servicesDllFile = Path.Combine(basePath, "SLD.Net10.Service.dll");
            // 拼接仓储层dll物理路径
            var repositoryDllFile = Path.Combine(basePath, "SLD.Net10.Repository.dll");

            // 定义AOP拦截器集合（业务服务全局拦截）
            var aopTypes = new List<Type>() { typeof(ServiceAOPConfig) };
            // 单独注册AOP拦截器类至容器
            builder.RegisterType<ServiceAOPConfig>();

            #region 注册通用基础仓储泛型类 BaseRepository<T>

            builder.RegisterGeneric(typeof(BaseRepository<>))
                   .As(typeof(IBaseRepository<>))        // 实现接口绑定
                   .InstancePerDependency();               // 生命周期：瞬时，每次获取创建全新实例

            #endregion 注册通用基础仓储泛型类 BaseRepository<T>

            #region 注册通用基础业务泛型类 BaseServices<,>

            builder.RegisterGeneric(typeof(BaseServices<,>))
                   .As(typeof(IBaseServices<,>))
                   .EnableInterfaceInterceptors()          // 开启接口AOP动态代理
                   .InterceptedBy(aopTypes.ToArray())     // 指定使用ServiceAOP拦截器
                   .InstancePerDependency();                // 瞬时生命周期

            #endregion 注册通用基础业务泛型类 BaseServices<,>

            #region 批量扫描注册整个Service程序集所有业务服务

            // 加载Service层dll程序集
            var assemblysServices = Assembly.LoadFrom(servicesDllFile);
            builder.RegisterAssemblyTypes(assemblysServices)
                   .AsImplementedInterfaces()               // 自动匹配并注册所有实现的接口
                   .InstancePerDependency()                  // 瞬时生命周期
                   .PropertiesAutowired()                     // 开启属性自动注入
                   .EnableInterfaceInterceptors()            // 开启AOP代理
                   .InterceptedBy(aopTypes.ToArray());      // 全局使用ServiceAOP拦截

            #endregion 批量扫描注册整个Service程序集所有业务服务

            #region 批量扫描注册整个Repository程序集所有仓储类

            // 加载Repository仓储层dll程序集
            var assemblysRepository = Assembly.LoadFrom(repositoryDllFile);
            builder.RegisterAssemblyTypes(assemblysRepository)
                   .AsImplementedInterfaces()       // 绑定对应仓储接口
                   .PropertiesAutowired()             // 开启属性注入
                   .InstancePerDependency();          // 瞬时生命周期

            #endregion 批量扫描注册整个Repository程序集所有仓储类
        }
    }
}