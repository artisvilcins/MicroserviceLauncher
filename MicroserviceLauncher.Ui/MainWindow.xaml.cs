using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MicroserviceLauncher.Ui.Models;
using MicroserviceLauncher.Ui.Services;

namespace MicroserviceLauncher.Ui
{
    public partial class MainWindow : Window
    {
        private readonly ObservableCollection<MicroserviceRow> _microserviceConfigs;

        private IEnumerable<MicroserviceRow> MicroserviceConfigs => _microserviceConfigs;

        private readonly MicroserviceActionsService _microserviceActions;


        private readonly Dictionary<MicroserviceRow, Process> _processes;

        public MainWindow()
        {
            InitializeComponent();

            _processes = new Dictionary<MicroserviceRow, Process>();
            _microserviceActions = new MicroserviceActionsService();

            _microserviceConfigs = new ObservableCollection<MicroserviceRow>();

            var repo = new MicroserviceConfigRepository();

            foreach (var service in repo.GetMicroservices())
            {
                var conf = new MicroserviceRow
                {
                    Name = service.Name,
                    LaunchPath = service.LaunchPath,
                    GitPath = service.GitPath,
                    IsRunning = false,
                    ProcessName = service.ProcessName
                };

                _microserviceConfigs.Add(conf);
            }

            foreach (var p in Process.GetProcesses())
            {
                foreach (var configItem in _microserviceConfigs)
                {
                    if (!string.IsNullOrWhiteSpace(configItem.ProcessName) && p.ProcessName == configItem.ProcessName)
                    {
                        p.EnableRaisingEvents = true;

                        _processes.Add(configItem, p);
                        p.Exited += (s, e) =>
                        {
                            configItem.IsRunning = false;
                        };
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
                        var newProcess = _microserviceActions.StartMicroservice(microserviceConfig);
                        newProcess.EnableRaisingEvents = true;

                        var processName = newProcess.ProcessName;

                        _processes.Add(microserviceConfig, newProcess);

                        newProcess.Exited += (s, e) =>
                        {
                            microserviceConfig.IsRunning = false;
                        };
                    }
                    else
                    {
                        if (_processes.TryGetValue(microserviceConfig, out Process process))
                        {
                            process.Kill();

                            _processes.Remove(microserviceConfig);
                        }
                    }
                };
            }

            MicroservicesGrid.ItemsSource = _microserviceConfigs;
        }

        private void PullFromGit_Click(object sender, RoutedEventArgs e)
        {
            for (var vis = sender as Visual; vis != null; vis = VisualTreeHelper.GetParent(vis) as Visual)
            {
                if (vis is DataGridRow)
                {
                    var row = (DataGridRow)vis;
                    var item = (MicroserviceRow)row.Item;

                    _microserviceActions.PullFromGit(item);

                    break;
                }
            }
        }

        private async void PullAllServices_Click(object sender, RoutedEventArgs e)
        {
            Parallel.ForEach(MicroserviceConfigs, new ParallelOptions { MaxDegreeOfParallelism = 4 },
                service =>
                {
                    _microserviceActions.PullFromGit(service);
                });
        }

        private void StopAllServices_Click(object sender, RoutedEventArgs e)
        {
            foreach (var process in _processes)
            {
                if (process.Value != null)
                {
                    process.Value.Kill();
                    process.Key.IsRunning = false;
                }
            }

            MicroservicesGrid.Items.Refresh();
        }
    }
}
