using System;
using System.Collections.Generic;
using System.Text;

namespace SLD.Net10.Model.ControllerModuleEntity.Background.ModelOfManageUser
{
    public enum EnumRole
    {
        /// <summary>管理员：系统账号、角色、系统配置管理</summary>
        Admin=1,
        /// <summary>主管：业务审核、数据统计、业务管理</summary>
        Supervisor=2,
        /// <summary>操作员：日常业务执行操作</summary>
        Operator=3
    }
}
