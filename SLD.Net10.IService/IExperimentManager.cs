using SLD.Net10.IService.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SLD.Net10.IService
{
    /// <summary>
    /// 实验管理器接口：原有暂停恢复终止全部保留，新增变量解析传递
    /// </summary>
    public interface IExperimentManager
    {
        RunStatus GetCurrentStatus();

        Task DismantleExecutors(IEnumerable<ICommandCustomizeExecutor> executors);

        void LoadScheme(List<ExperimentCommand> commands);

        Task RunAsync();

        void Pause();

        void Resume();

        void Stop();
    }
}
