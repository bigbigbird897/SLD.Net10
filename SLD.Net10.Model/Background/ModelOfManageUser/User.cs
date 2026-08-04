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

        [SugarColumn(Length = 50, IsNullable = false)]
        public string Username { get; set; }

        [SugarColumn(Length = 50, IsNullable = false)]
        public string Password { get; set; }
    }
}