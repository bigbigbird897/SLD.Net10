using SqlSugar;
using System;
using System.Collections.Generic;
using System.Text;

namespace SLD.Net10.Model
{
    public class UserVo
    {
        /// <summary>
        /// 用户名，唯一，非空，长度50
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 年龄，允许为空
        /// </summary>
        public int? Age { get; set; }
    }
}
