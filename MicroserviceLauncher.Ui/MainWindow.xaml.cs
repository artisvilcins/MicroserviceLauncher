using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MicroserviceLauncher.Uiw
{
    public partial class MainWindow : Window
    {
        private readonly ObservableCollection<MicroserviceRow> microserviceConfigs;

        public IEnumerable<MicroserviceRow> MicroserviceConfigs { get { return microserviceConfigs; } }

        MicroserviceActionsService microserviceActions;


        Dictionary<MicroserviceRow, Process> processes;

        public MainWindow()
        {
            InitializeComponent();

            processes = new Dictionary<MicroserviceRow, Process>();
            microserviceActions = new MicroserviceActionsService();

            microserviceConfigs = new ObservableCollection<MicroserviceRow>();

            var repo = new MicroserviceConfigRepository();

            foreach (var service in repo.GetMicroservices())
            {
                var conf = new MicroserviceRow
                {
                    Name = service.Name,
                    LaunchPath = service.LaunchPath,
                    GitPath = service.GitPath,
                    IsRunning = false
                };

                microserviceConfigs.Add(conf);
            }

            foreach (var microserviceConfig in microserviceConfigs)
            {
                microserviceConfig.IsRunningChange += change =>
                {
                    if (change)
                    {
                        if (processes.TryGetValue(microserviceConfig, out Process process))
                        {
                            process.Start();
                        }
                        else
                        {
                            var newProcess = microserviceActions.StartMicroservice(microserviceConfig);
                            newProcess.EnableRaisingEvents = true;

                            processes.Add(microserviceConfig, newProcess);

                            newProcess.Exited += (s, e) =>
                            {
                                microserviceConfig.IsRunning = false;
                            };
                        }
                    }
                    else
                    {
                        if (processes.TryGetValue(microserviceConfig, out Process process))
                        {
                            process.Kill();
                        }
                    }
                };
            }

            MicroservicesGrid.ItemsSource = microserviceConfigs;
        }

        private void PullFromGit_Click(object sender, RoutedEventArgs e)
        {
            for (var vis = sender as Visual; vis != null; vis = VisualTreeHelper.GetParent(vis) as Visual)
            {
                if (vis is DataGridRow)
                {
                    var row = (DataGridRow)vis;
                    var item = (MicroserviceRow)row.Item;

                    microserviceActions.PullFromGit(item);

                    break;
                }
            }
        }

        private async void PullAllServices_Click(object sender, RoutedEventArgs e)
        {
            Parallel.ForEach(MicroserviceConfigs, new ParallelOptions { MaxDegreeOfParallelism = 4 },
                service =>
                {
                    microserviceActions.PullFromGit(service);
                });
        }

        private void StopAllServices_Click(object sender, RoutedEventArgs e)
        {
            foreach (var process in processes)
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
