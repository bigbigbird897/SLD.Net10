using System;
using System.Collections.Generic;
using System.Text;

namespace SLD.Net10.Model.Background.ModelOfManageUser.Entity
{
    public static class RoleConst
    {
        /// <summary>管理员：系统账号、角色、系统配置管理</summary>
        public const string Admin = "Admin";
        /// <summary>主管：业务审核、数据统计、业务管理</summary>
        public const string Supervisor = "Supervisor";
        /// <summary>操作员：日常业务执行操作</summary>
        public const string Operator = "Operator";
    }
}
