using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace SensorsReport.Frontend;
public class Program
{

    private const string EnvVariablePrefix = "SR_";
    public static void Main(string[] args)
    {
        new AppServices.DynamicDataGenerator().RunAndExitIf(args);

        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStaticWebAssets();
                webBuilder.UseStartup<Startup>();
            })
            .ConfigureAppConfiguration((builderContext, config) =>
            {
                config.AddJsonFile("appsettings.bundles.json");
                config.AddJsonFile("appsettings.machine.json", optional: true);

                config.AddEnvironmentVariables();
                config.AddEnvironmentVariables(EnvVariablePrefix);
            });
    }
}
