using AutoMapper;
using Microsoft.Extensions.Logging;
using SLD.Net10.Model.Background.ModelOfManageUser;

namespace SLD.Net10.Extension.ComponentConfig
{
    /// <summary>
    /// AutoMapper 全局映射配置静态类
    /// 统一加载实体与DTO之间的映射规则，支持传入日志工厂捕获映射异常
    /// </summary>
    public class AutoMapperConfig
    {
        /// <summary>
        /// 无日志工厂重载方法（兼容历史旧代码调用）
        /// </summary>
        /// <returns>AutoMapper全局配置实例</returns>
        public static MapperConfiguration RegisterMappings()
        {
            // 调用带日志参数的重载，日志工厂传null
            return RegisterMappings(null);
        }

        /// <summary>
        /// 带日志工厂重载方法（推荐使用）
        /// </summary>
        /// <param name="loggerFactory">日志工厂，用于记录AutoMapper映射过程中的异常信息</param>
        /// <returns>AutoMapper全局配置实例</returns>
        public static MapperConfiguration RegisterMappings(ILoggerFactory? loggerFactory)
        {
            // 实例化Mapper配置对象
            return new MapperConfiguration(cfg =>
            {
                // 加载自定义映射规则类（包含所有实体、DTO之间的转换映射）
                cfg.AddProfile(new AutoMapperCustomProfile());
            }, loggerFactory); // 传入日志工厂，映射报错时自动输出日志
        }
    }

    /// <summary>
    /// AutoMapper 自定义映射配置类
    /// 继承 Profile，统一配置实体与Vo视图对象之间的双向转换规则
    /// </summary>
    public class AutoMapperCustomProfile : Profile
    {
        /// <summary>
        /// 构造函数：所有对象映射关系在此统一定义
        /// 程序初始化时由 AutoMapperConfig 自动加载执行
        /// </summary>
        public AutoMapperCustomProfile()
        {
            // 实体 User 转 前端视图对象 UserVo
            CreateMap<User, UserVo>();
            // 前端视图对象 UserVo 反向转回数据库实体 User
            CreateMap<UserVo, User>();
        }
    }
}