using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ContactSvc.Dtos;
using ContactSvc.Settings;
using System.Threading.Tasks;

namespace ContactSvc.Data
{
    public class ContactDbContext : DbContext
    {
        private readonly IConfiguration _configuration;
        private readonly IAzureSecrets _azureSecrets;
        private readonly ILogger<ContactDbContext> _logger;
        private string _connectionString = string.Empty;

        public ContactDbContext(DbContextOptions<ContactDbContext> options, 
            IConfiguration configuration,
            IAzureSecrets azureSecrets,
            ILogger<ContactDbContext> logger) : base(options)
        {
            _configuration = configuration;
            _azureSecrets = azureSecrets;
            _logger = logger;
        }

        public DbSet<CustomerMessage> CustomerMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ContactDbContext).Assembly);
        }

        public async Task<string> CreateConnectionStringAsync()
        {
            try
            {
                _logger.LogInformation("Creating connection string...");
                string? conSetting = _configuration.GetConnectionString("DefaultConnection");
                if (conSetting != null)
                {
                    _logger.LogInformation("Fetching database settings from Azure Key Vault...");
                    DBSettings dbSettings = await _azureSecrets.GetDBSettingsAsync();

                    _connectionString = string.Format(conSetting, 
                        dbSettings.ServerName, dbSettings.DatabaseName, dbSettings.UserName, dbSettings.UserPassword);

                    Database.SetConnectionString(_connectionString);
                    _logger.LogInformation("Connection string created successfully.");
                }
                else
                {
                    _logger.LogWarning("DefaultConnection setting is null.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating connection string.");
                throw;
            }
            return _connectionString;
        }
    }
}