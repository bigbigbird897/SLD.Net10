namespace SLD.Net10.Model.ControllerModuleEntity.Background.ModelOfManageUser.DbEntity
{
    public class UserVo
    {
        //public string Id { get; set; } = string.Empty;
        public string Username { get; set; }= string.Empty;
        public string Password { get; set; }= string.Empty;
        public string[] Roles { get; set; } = Array.Empty<string>();
        public DateTime LastPasswordChangeTime {  get; set; }= DateTime.MinValue;
    }
    public class UserVoForLogin
    {
        //public string Id { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
    public class UserVoForAdd
    {
        //public string Id { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string[] Roles { get; set; } = Array.Empty<string>();
    }
}