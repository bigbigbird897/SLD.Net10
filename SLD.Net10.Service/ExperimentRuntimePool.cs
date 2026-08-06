using SLD.Net10.IService;
using System.Collections.Concurrent;

namespace SLD.Net10.Service
{
    public class ExperimentRuntimePool : IExperimentRuntimePool
    {
        private readonly ConcurrentDictionary<string, IExperimentManager> _pool = new();

        public void RemoveManager(string experimentId)
        {
            _pool.TryRemove(experimentId, out _);
        }

        IExperimentManager? IExperimentRuntimePool.GetManager(string experimentId)
        {
            _pool.TryGetValue(experimentId, out var mgr);
            return mgr;
        }

        public void SetManager(string experimentId, IExperimentManager manager)
        {
            _pool[experimentId] = manager;
        }
    }
}
