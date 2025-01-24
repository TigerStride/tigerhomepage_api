    /// <summary>
    /// Implement an Azure Function as the backend of the Tigerstride corporate website.
    /// For example, to respond to customer inquiries via email.
    /// </summary>


using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

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
    .Build();

host.Run();

