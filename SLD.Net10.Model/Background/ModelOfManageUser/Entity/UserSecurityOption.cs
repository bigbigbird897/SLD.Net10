using System;
using System.Collections.Generic;
using System.Text;

namespace SLD.Net10.Model.Background.ModelOfManageUser.Entity
{
    public class UserSecurityOption
    {
        public bool EnablePasswordHave8Bit { get; set; }
        public bool EnablePasswordExpire { get; set; }
        public int PasswordExpireDay { get; set; }
        public bool EnableLoginFailLock { get; set; }
        public int MaxPasswordErrorCount { get; set; }
        //public int LockMinutes { get; set; }
    }
}
