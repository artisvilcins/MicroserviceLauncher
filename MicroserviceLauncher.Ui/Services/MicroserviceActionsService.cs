using System.Diagnostics;

namespace MicroserviceLauncher
{
    public class MicroserviceActionsService
    {
        public Process StartMicroservice(MicroserviceConfig config)
        {
            ProcessStartInfo startinfo = new ProcessStartInfo("dotnet", $"run --project {config.LaunchPath}");
            startinfo.CreateNoWindow = false;
            startinfo.UseShellExecute = true;
            startinfo.WindowStyle = ProcessWindowStyle.Minimized;

            return Process.Start(startinfo);
        }

        public void PullFromGit(MicroserviceConfig config)
        {
            if (string.IsNullOrWhiteSpace(config.GitPath))
            {
                return;
            }

            ProcessStartInfo gitUpdateStartInfo = new ProcessStartInfo("git", "pull");
            gitUpdateStartInfo.WorkingDirectory = config.GitPath;
            gitUpdateStartInfo.CreateNoWindow = false;
            gitUpdateStartInfo.UseShellExecute = true;

            var gitUpdateProcess = Process.Start(gitUpdateStartInfo);

            gitUpdateProcess.WaitForExit();
        }
    }
}
