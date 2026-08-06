using SLD.Net10.IService.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace SLD.Net10.IService
{
    /// <summary>
    /// 实验运行实例池（单例，存放所有正在加载/运行的实验管理器）
    /// </summary>
    public interface IExperimentRuntimePool
    {
        /// <summary>根据实验Id获取运行实例，不存在返回null</summary>
        IExperimentManager? GetManager(string experimentId);

        /// <summary>存入实验管理器</summary>
        void SetManager(string experimentId, IExperimentManager manager);

        /// <summary>移除实验管理器</summary>
        void RemoveManager(string experimentId);
    }
}
