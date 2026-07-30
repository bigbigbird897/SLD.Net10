using Microsoft.Extensions.Logging;
using SLD.Net10.IService;
using SLD.Net10.IService.Entity;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SLD.Net10.Service
{
    /// <summary>
    /// 实验管理器实现
    /// </summary>
    public class ExperimentManager : IExperimentManager
    {
        //属性注入
        private readonly ILogger? _logger;
        private readonly RunContext _context = new();
        private Dictionary<string, ICommandCustomizeExecutor>? _executorDict;
        private List<ExperimentCommand>? _commandList;

        public ExperimentManager(ILogger<ExperimentManager> logger)
        {
            _logger= logger;
        }

        public async Task DismantleExecutors(IEnumerable<ICommandCustomizeExecutor> executors)
        {
            _executorDict = new Dictionary<string, ICommandCustomizeExecutor>();
            foreach (var exec in executors)
            {
                _executorDict[exec.MatchCommand] = exec;
            }
        }


        public RunStatus GetCurrentStatus() => _context.Status;

        /// <summary>加载一套实验命令方案</summary>
        public void LoadScheme(List<ExperimentCommand> commands)
        {
            _commandList = commands;
            _context.CurrentCommandIndex = 0;
            _context.ContextVariables.Clear(); // 每次加载方案清空全局变量池
        }

        /// <summary>
        /// 解析参数：把输入中 ${VarName} 替换为上下文中的实际值
        /// </summary>
        private Dictionary<string, object> ResolveInputParams(Dictionary<string, string> rawInput)
        {
            var resolved = new Dictionary<string, object>();
            foreach (var kv in rawInput)
            {
                string rawValue = kv.Value;
                // 匹配 ${xxx} 变量语法
                var match = Regex.Match(rawValue, @"^\$\{(\w+)\}$");
                if (match.Success)
                {
                    string varName = match.Groups[1].Value;
                    if (_context.ContextVariables.TryGetValue(varName, out var val))
                    {
                        resolved[kv.Key] = val;
                        _logger.LogInformation($"    参数解析：{kv.Key} = ${varName} → {val}");
                    }
                    else
                    {
                        throw new Exception($"引用变量[{varName}]不存在，请检查前面步骤输出");
                    }
                }
                else
                {
                    // 普通常量值
                    resolved[kv.Key] = rawValue;
                }
            }
            return resolved;
        }

        public async Task RunAsync()
        {
            if (_context.Status == RunStatus.Running)
            {
                _logger.LogInformation("错误：实验正在运行，不可重复启动");
                return;
            }
            if (_commandList == null || _commandList.Count == 0)
            {
                _logger.LogInformation("错误：没有加载实验命令");
                return;
            }

            _context.StopTokenSource = new CancellationTokenSource();
            CancellationToken token = _context.StopTokenSource.Token;
            _context.Status = RunStatus.Running;
            _context.PauseEvent.Set();

            _logger.LogInformation("===== 实验方案开始执行 =====");

            for (; _context.CurrentCommandIndex < _commandList.Count; _context.CurrentCommandIndex++)
            {
                token.ThrowIfCancellationRequested();

                var cmd = _commandList[_context.CurrentCommandIndex];
                _logger.LogInformation($"\n>>> 执行命令[{_context.CurrentCommandIndex + 1}] {cmd.CommandName}");

                // 暂停阻塞点
                _context.PauseEvent.Wait(token);

                // 1.解析参数：处理 ${变量} 引用，从上下文取上一步输出
                var resolvedParams = ResolveInputParams(cmd.InputParams);

                // 2.找到对应执行器执行
                if (!_executorDict.TryGetValue(cmd.CommandName, out var executor))
                {
                    throw new Exception($"找不到命令执行器 {cmd.CommandName}");
                }

                // 3.执行该实验命令
                StepExecuteResult stepResult = await executor.ExecuteAsync(resolvedParams, token);

                if (!stepResult.Success)
                {
                    _logger.LogInformation($"命令执行失败：{stepResult.Message}");
                    throw new Exception($"命令 {cmd.CommandName} 执行失败，实验终止");
                }

                // 4.【核心】把本步骤输出变量合并进全局上下文，供后面命令读取
                foreach (var outputKv in stepResult.OutputVars)
                {
                    _context.ContextVariables[outputKv.Key] = outputKv.Value;
                }
            }

            _logger.LogInformation("\n===== 全部实验命令执行完毕 =====");
            _context.Status = RunStatus.Idle;
            _context.StopTokenSource.Dispose();
        }

        public void Pause()
        {
            if (_context.Status != RunStatus.Running)
            {
                _logger.LogInformation("当前状态不可暂停");
                return;
            }
            _context.PauseEvent.Reset();
            _context.Status = RunStatus.Paused;
            _logger.LogInformation("实验已暂停，输入 resume 恢复");
        }

        public void Resume()
        {
            if (_context.Status != RunStatus.Paused)
            {
                _logger.LogInformation("当前状态不可恢复");
                return;
            }
            _context.PauseEvent.Set();
            _context.Status = RunStatus.Running;
            _logger.LogInformation("实验已恢复");
        }

        public void Stop()
        {
            if (_context.Status is RunStatus.Idle or RunStatus.Stopped)
            {
                _logger.LogInformation("无运行中实验");
                return;
            }
            _context.StopTokenSource?.Cancel();
            _context.PauseEvent.Set();
            _context.Status = RunStatus.Stopped;
            _logger.LogInformation("实验已强制终止");
        }
    }
}
