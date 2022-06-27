using Doreto.Shared.Native;

using Microsoft.Extensions.Logging;

using System.Diagnostics;

namespace Doreto.Shared.DofusClient;


public class CharacterRegistrator
{
    private ILogger<CharacterRegistrator> _logger;

    public CharacterRegistrator(ILogger<CharacterRegistrator> logger)
    {
        _logger = logger;
        _logger.LogDebug("Initialized new character registrator !");
    }

    #region Public methods

    /// <summary>
    ///     Looks for a process with a certain name and/or having certain agruments
    /// </summary>
    /// <param name="processName">The process name for which to look for</param>
    /// <param name="processHavingArguments">The arguments that the process must have</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Process> WaitForProcessToSpawn(string processName, string? processHavingArguments = default, CancellationToken cancellationToken = default)
    {
        var output = default(Process);

        PeriodicTimer p = new(TimeSpan.FromSeconds(1));
        while (await p.WaitForNextTickAsync(cancellationToken) && output == null)
        {
            // Get all the processes by name
            Process[] processes = Process.GetProcessesByName(processName);

            // If we're looking for specific arguments, filter them
            // by only taking the ones that contain processHavingArguments
            if (!string.IsNullOrEmpty(processHavingArguments))
            {
                processes = processes.Where(p =>
                {
                    var result = ProcessCommandLine.Retrieve(p, out var processArguments);
                    return result == 0 && processArguments.Contains(processHavingArguments, StringComparison.InvariantCultureIgnoreCase);
                }).ToArray();
            }

            if (processes.Length > 0)
                output = processes.First();
        }

        return output!;
    }

    #endregion
}
