using SLD.Net10.Model;
using SLD.Net10.Model.Business.ModelOfManageUser;
using SqlSugar;

namespace SLD.Net10.Extension.ComponentConfig
{
    /// <summary>
    /// SqlSugar CodeFirst 扩展静态类
    /// 封装自动创建数据库、根据实体自动建表逻辑
    /// </summary>
    public static class SqlSugarCodeFirstConfig
    {
        /// <summary>
        /// ISqlSugarClient 扩展方法，执行CodeFirst初始化
        /// 1. 不存在数据库则自动创建库
        /// 2. 根据传入的实体类自动创建数据表，表已存在则不做操作
        /// </summary>
        /// <param name="db">SqlSugar数据库操作实例（this标识为扩展方法）</param>
        public static void InitCodeFirst(this ISqlSugarClient db)
        {
            // 自动创建数据库，库不存在才会执行创建
            db.DbMaintenance.CreateDatabase();

            // 定义需要CodeFirst生成数据表的实体类型数组
            Type[] entities =
            {
                typeof(User)
            };

            // 根据实体批量建表，自动读取实体[SugarTable]、[SugarColumn]特性生成字段、主键、自增等约束
            db.CodeFirst.InitTables(entities);
        }
    }
}