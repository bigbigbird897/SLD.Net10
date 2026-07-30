using SLD.Net10.IService;
using SLD.Net10.IService.Entity;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace SLD.Net10.Service
{
    public class ExperimentRuntimePool : IExperimentRuntimePool
    {
        private readonly ConcurrentDictionary<long, ExperimentManager> _pool = new();

        public ExperimentManager? GetManager(long experimentId)
        {
            _pool.TryGetValue(experimentId, out var mgr);
            return mgr;
        }

        public void SetManager(long experimentId, ExperimentManager manager)
        {
            _pool[experimentId] = manager;
        }

        public void RemoveManager(long experimentId)
        {
            _pool.TryRemove(experimentId, out _);
        }
    }
}
