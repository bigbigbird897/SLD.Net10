using SqlSugar;

namespace SLD.Net10.Model
{
    public class User
    {
        /// <summary>
        /// 主键自增Id
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long Id { get; set; }

        /// <summary>
        /// 用户名，唯一，非空，长度50
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = false)]
        public string UserName { get; set; }

        /// <summary>
        /// 年龄，允许为空
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public int? Age { get; set; }
    }
}