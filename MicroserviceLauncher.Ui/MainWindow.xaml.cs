using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MicroserviceLauncher.Ui.Models;
using MicroserviceLauncher.Ui.Services;

namespace MicroserviceLauncher.Ui;

public partial class MainWindow
{
    private readonly ObservableCollection<MicroserviceRow> _microserviceConfigs;

    private IEnumerable<MicroserviceRow> MicroserviceConfigs => _microserviceConfigs;

    private readonly MicroserviceActionsService _microserviceActions;


    private readonly Dictionary<MicroserviceRow, Process> _processes;
    private readonly MicroserviceConfigRepository _repo;

    public MainWindow()
    {
        InitializeComponent();

        _processes = new Dictionary<MicroserviceRow, Process>();
        _microserviceActions = new MicroserviceActionsService();

        _microserviceConfigs = new ObservableCollection<MicroserviceRow>();

        _repo = new MicroserviceConfigRepository();

        foreach (var service in _repo.GetMicroservices())
        {
            var conf = new MicroserviceRow
            {
                Name = service.Name,
                LaunchPath = service.LaunchPath,
                GitPath = service.GitPath,
                IsRunning = false,
                ProcessName = service.ProcessName,
                IsMandatory = service.IsMandatory
            };

            _microserviceConfigs.Add(conf);

            var top = 66;

            foreach (var gitFolder in _repo.GetGitFolders())
            {
                var newBtn = new Button
                {
                    Content = gitFolder.Name,
                    Height = 26,
                    Width = 150,
                    Margin = new Thickness(10, top, 0, 0),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top
                };

                newBtn.Click += (_, __) => PullFromFolders_Click(gitFolder);
                    
                GitLayoutRoot.Children.Add(newBtn);
                top += 40;
            }
        }

        foreach (var p in Process.GetProcesses())
        {
            foreach (var configItem in _microserviceConfigs)
            {
                if (!string.IsNullOrWhiteSpace(configItem.ProcessName) && p.ProcessName == configItem.ProcessName)
                {
                    p.EnableRaisingEvents = true;

                    _processes.Add(configItem, p);
                    p.Exited += (_, _) => { configItem.IsRunning = false; };
                    configItem.IsRunning = true;
                }
            }
        }


        foreach (var microserviceConfig in _microserviceConfigs)
        {
            microserviceConfig.IsRunningChange += change =>
            {
                if (change)
                {
                    StartMicroservice(microserviceConfig);
                }
                else
                {
                    if (!_processes.TryGetValue(microserviceConfig, out var process))
                    {
                        return;
                    }

                    process.Kill();
                    _processes.Remove(microserviceConfig);
                }
            };
        }

        MicroservicesGrid.ItemsSource = _microserviceConfigs;
    }

    private void StartMicroservice(MicroserviceRow microserviceConfig)
    {
        var newProcess = _microserviceActions.StartMicroservice(microserviceConfig);
        newProcess.EnableRaisingEvents = true;

        _processes.Add(microserviceConfig, newProcess);

        newProcess.Exited += (_, _) => { microserviceConfig.IsRunning = false; };
    }

    private void StartMandatoryServices_Click(object sender, RoutedEventArgs e)
    {
        foreach (var microservice in _microserviceConfigs)
        {
            if (!microservice.IsMandatory || microservice.IsRunning)
            {
                continue;
            }

            microservice.IsRunning = true;
            _microserviceActions.StartMicroservice(microservice);
        }

        MicroservicesGrid.Items.Refresh();
    }

    private void PullFromGit_Click(object sender, RoutedEventArgs e)
    {
        for (var vis = sender as Visual; vis != null; vis = VisualTreeHelper.GetParent(vis) as Visual)
        {
            if (vis is not DataGridRow row)
            {
                continue;
            }

            var item = (MicroserviceRow)row.Item;
            _microserviceActions.PullFromGit(item.GitPath);
            break;
        }
    }

    private void PullAllServices_Click(object sender, RoutedEventArgs e)
    {
        Parallel.ForEach(MicroserviceConfigs, new ParallelOptions { MaxDegreeOfParallelism = 4 },
            service => { _microserviceActions.PullFromGit(service.GitPath); });
    }

    private void StopAllServices_Click(object sender, RoutedEventArgs e)
    {
        foreach (var process in _processes.Where(process => process.Value != null))
        {
            process.Value.Kill();
            process.Key.IsRunning = false;
        }

        MicroservicesGrid.Items.Refresh();
    }

    private void PullFromAllFolders_Click(object sender, RoutedEventArgs e)
    {
        foreach (var gitFolder in _repo.GetGitFolders())
        {
            PullFromFolders_Click(gitFolder);
        }
    }
    
    private void PullFromFolders_Click(GitFolder folder)
    {
        var mainDir = new DirectoryInfo(folder.Path);

        var subDirs = mainDir.EnumerateDirectories();
        
        Parallel.ForEach(subDirs, new ParallelOptions { MaxDegreeOfParallelism = 5 },
            subDir =>
            {
                if (subDir.EnumerateDirectories().Any(s => s.Name.Contains(".git")))
                {
                    _microserviceActions.PullFromGit(subDir.FullName);
                }
            });
    }
}