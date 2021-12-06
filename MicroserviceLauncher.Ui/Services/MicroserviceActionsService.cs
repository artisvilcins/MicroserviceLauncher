using System.Diagnostics;
using MicroserviceLauncher.Ui.Models;

namespace MicroserviceLauncher.Ui.Services
{
    public class MicroserviceActionsService
    {
        public Process StartMicroservice(MicroserviceConfig config)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo("dotnet", $"run --project {config.LaunchPath}")
            {
                CreateNoWindow = false,
                UseShellExecute = true,
                WindowStyle = ProcessWindowStyle.Minimized
            };

            return Process.Start(startInfo);
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
