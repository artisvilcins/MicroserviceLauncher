﻿namespace MicroserviceLauncher.Ui.Models
{
    public class MicroserviceConfig
    {
        public string Name { get; set; }
        public string GitPath { get; set; }
        public string LaunchPath { get; set; }
        public string ProcessName { get; set; }
        public bool IsMandatory { get; set; }
    }
}
