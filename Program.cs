/// <summary>
/// Implement an Azure Function as the backend of the Tigerstride corporate website.
/// For example, to respond to customer inquiries via email.
/// </summary>


using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using ContactSvc.Data;
using Microsoft.EntityFrameworkCore;
using ContactSvc.Settings;
using Microsoft.Extensions.Logging;


var host = new HostBuilder()
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();

        if (context.HostingEnvironment.IsDevelopment())
        {
            config.AddUserSecrets<Program>();
        }
    })
    .ConfigureFunctionsWebApplication()
    .ConfigureLogging((context, logging) =>
    {
        var logger = new LoggerConfiguration()
            .ReadFrom.Configuration(context.Configuration)
            .CreateLogger();

        logging.AddSerilog(logger);
    })
    .ConfigureServices((context, services) =>
    {
        services.AddSingleton<IAzureSecrets, AzureSecrets>();
        services.AddDbContext<ContactDbContext>((serviceProvider, options) =>
        {
            var config = serviceProvider.GetRequiredService<IConfiguration>();
            var connectionString = config.GetConnectionString("DefaultConnection");
            var version = config["DatabaseSettings:MySqlVersion"];
            options.UseMySql(connectionString, new MySqlServerVersion(version));
        });
        services.AddScoped<ContactRepo>();
    })
    .Build();

await host.RunAsync();

