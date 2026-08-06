using System;
using System.Collections.Generic;
using System.Text;

namespace SLD.Net10.IService.Entity
{
    public enum RunStatus
    {
        Idle,      // 空闲
        Running,   // 运行中
        Paused,    // 已暂停
        Stopped    // 已终止
    }
}
