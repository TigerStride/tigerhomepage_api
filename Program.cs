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
        services.AddScoped<ContactRepo>();
        services.AddDbContext<ContactDbContext>((serviceProvider, options) =>
        {
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var logger = serviceProvider.GetRequiredService<ILogger<ContactDbContext>>();
            var dbContext = serviceProvider.GetRequiredService<ContactDbContext>();

            var connectionString = dbContext.CreateConnectionString();
            var dbVersion = new MySqlServerVersion(new Version(configuration["DBSettings:MySqlVersion"] ?? "8.0.25"));
            options.UseMySql(connectionString, dbVersion);

            logger.LogInformation($"Using connection string: {connectionString}");
        });
    })
    .Build();

host.Run();

