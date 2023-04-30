using System;
using System.Diagnostics;
using MicroserviceLauncher.Ui.Models;

namespace MicroserviceLauncher.Ui.Services
{
    public class MicroserviceActionsService
    {
        public Process StartMicroservice(MicroserviceConfig config)
        {
            if (config.LaunchPath.EndsWith(".csproj"))
            {
                return RunDotnetMicroservice(config.LaunchPath);
            }

            if (config.LaunchPath.EndsWith(".exe"))
            {
                return LaunchExeMicroservice(config.LaunchPath);
            }

            throw new ArgumentException("Not supported microservice launch type");
        }

        private static Process LaunchExeMicroservice(string launchPath)
        {
            return Process.Start(launchPath);
        }

        private static Process RunDotnetMicroservice(string launchPath)
        {
            DotnetRestore(launchPath);

            var startInfo = new ProcessStartInfo("dotnet", $"run --project {launchPath}")
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

        public void PullFromGit(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }
        
            var gitswitchStartInfo = new ProcessStartInfo("git", "switch -")
            {
                WorkingDirectory = path,
                CreateNoWindow = false,
                UseShellExecute = true,
                WindowStyle = ProcessWindowStyle.Minimized,
            };
            var gitswitchProcess = Process.Start(gitswitchStartInfo);
            gitswitchProcess.WaitForExit();
            
            
            var gitUpdateStartInfo = new ProcessStartInfo("git", "pull")
            {
                WorkingDirectory = path,
                CreateNoWindow = false,
                UseShellExecute = true,
                WindowStyle = ProcessWindowStyle.Minimized,
            };
            var gitUpdateProcess = Process.Start(gitUpdateStartInfo);
            gitUpdateProcess.WaitForExit();
        }
    }
}