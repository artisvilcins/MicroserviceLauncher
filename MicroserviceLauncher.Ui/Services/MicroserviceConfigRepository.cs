using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace MicroserviceLauncher
{
    public class MicroserviceConfigRepository
    {
        private readonly IConfiguration _configuration;

        public MicroserviceConfigRepository()
        {
            _configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        }

        public List<MicroserviceConfig> GetMicroservices()
        {
            var section = _configuration.GetSection("microservices");

            var services = new List<MicroserviceConfig>();

            foreach (var item in section.GetChildren())
            {
                services.Add(new MicroserviceConfig
                {
                    Name = item["name"],
                    GitPath = item["gitPath"],
                    LaunchPath = item["launchPath"]
                });
            }

            return services;
        }
    }
}
