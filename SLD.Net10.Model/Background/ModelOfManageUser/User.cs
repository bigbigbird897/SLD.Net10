using SqlSugar;

namespace SLD.Net10.Model.Background.ModelOfManageUser
{
    public class User
    {
        /// <summary>
        /// 主键自增Id
        /// </summary>
        [SugarColumn(IsPrimaryKey = true)]
        public string Id { get; set; }

        [SugarColumn(IsPrimaryKey = true,Length = 50, IsNullable = false)]
        public string Username { get; set; }

        [SugarColumn(Length = 50, IsNullable = false)]
        // 密码最小长度8位
        public string Password { get; set; }

        // 数据库字段：text 或者 varchar(1000)
        //数据库存 varchar/text，SqlSugar 自动序列化 / 反序列化。
        [SugarColumn(IsJson = true, ColumnDataType = "text")]
        public string[] Roles { get; set; }= Array.Empty<string>();

        #region 密码过期90天、登录错误锁定（可选功能，开关放在配置文件）

        /// <summary>
        /// 最近一次密码修改时间
        /// 用于90天密码过期校验；如果功能关闭此字段不做判断
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public DateTime? LastPasswordChangeTime { get; set; }

        /// <summary>
        /// 账户是否被锁定
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public bool IsLocked { get; set; } = false;

        /// <summary>
        /// 账户锁定到期时间；null代表永久锁定，到时间自动解锁
        /// </summary>
        //[SugarColumn(IsNullable = true)]
        //public DateTime? LockEndTime { get; set; }

        /// <summary>
        /// 连续登录密码错误次数，达到5次触发锁定
        /// 登录成功后清零
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public int PasswordErrorCount { get; set; } = 0;

        #endregion
    }
}