using System.Collections.Generic;
using System.Linq;
using MicroserviceLauncher.Ui.Models;
using Microsoft.Extensions.Configuration;

namespace MicroserviceLauncher.Ui.Services
{
    public class MicroserviceConfigRepository
    {
        private readonly IConfiguration _configuration;

        public MicroserviceConfigRepository()
        {
            _configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddJsonFile("appsettings.Development.json", true)
                .Build();
        }

        public List<MicroserviceConfig> GetMicroservices()
        {
            var section = _configuration.GetSection("microservices");

            return section.GetChildren().Select(item =>
                new MicroserviceConfig
                {
                    Name = item["name"],
                    GitPath = item["gitPath"],
                    LaunchPath = item["launchPath"],
                    ProcessName = item["processName"],
                    IsMandatory = !string.IsNullOrWhiteSpace(item["isMandatory"]) && bool.Parse(item["isMandatory"])
                }).ToList();
        }
    }
}