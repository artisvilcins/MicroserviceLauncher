using System.Diagnostics;
using MicroserviceLauncher.Ui.Models;

namespace MicroserviceLauncher.Ui.Services
{
    public class MicroserviceActionsService
    {
        public Process StartMicroservice(MicroserviceConfig config)
        {
            DotnetRestore(config.LaunchPath);
            
            var startInfo = new ProcessStartInfo("dotnet", $"run --project {config.LaunchPath}")
            {
                CreateNoWindow = false,
                UseShellExecute = true,
                WindowStyle = ProcessWindowStyle.Minimized,
            };

            return Process.Start(startInfo);
        }

        private static void DotnetRestore(string launchPath)
        {
            var startInfo = new ProcessStartInfo("dotnet", $"restore --project {launchPath}")
            {
                CreateNoWindow = false,
                UseShellExecute = true,
                WindowStyle = ProcessWindowStyle.Minimized,
            };
            
            var process = Process.Start(startInfo);
            process.WaitForExit();
        }

        public void PullFromGit(MicroserviceConfig config)
        {
            if (string.IsNullOrWhiteSpace(config.GitPath))
            {
                return;
            }

            ProcessStartInfo gitUpdateStartInfo = new ProcessStartInfo("git", "pull")
            {
                WorkingDirectory = config.GitPath,
                CreateNoWindow = false,
                UseShellExecute = true
            };

            var gitUpdateProcess = Process.Start(gitUpdateStartInfo);

            gitUpdateProcess.WaitForExit();
        }
    }
}
