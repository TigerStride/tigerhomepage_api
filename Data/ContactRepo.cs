using ContactSvc.Dtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ContactSvc.Data
{
    public class ContactRepo
    {
        private readonly ContactDbContext _context;
        private readonly ILogger<ContactRepo> _logger;
        private readonly IConfiguration _config;

        private string _connectionString = string.Empty;

        public ContactRepo(ContactDbContext context, IConfiguration config, ILogger<ContactRepo> logger)
        {
            _context = context;
            _logger = logger;
            _config = config;
        }

        /// <summary>
        /// Connect the Entity Framework Core to the database.
        /// </summary>
        /// <param name="dbSettings">MySQL database connection settings, from Azure secrets</param>
        /// <returns>void</returns>
        public void Connect(DBSettings dbSettings)
        {
            try
            {
                _logger.LogInformation("Connecting to the database...");
                if (string.IsNullOrEmpty(_connectionString))
                {
                    CreateConnectionString(dbSettings);
                    _context.Database.SetConnectionString(_connectionString);
                }
                _logger.LogInformation("Connected to the database successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error connecting to the database.");
                throw;
            }
            return;
        }

        /// <summary>
        /// Create a valid MySQL connection string from the template found in DefaultConnection config
        /// </summary>
        /// <param name="dbSettings">MySQL database connection settings, from Azure secrets</param>
        /// <returns>void</returns>        
        private void CreateConnectionString(DBSettings dbSettings)
        {
            try
            {
                _logger.LogInformation("Creating connection string...");

                _connectionString = _config.GetConnectionString("DefaultConnection")
                    ?? throw new ArgumentNullException("DefaultConnection connection is missing.");

                // Reconfigure connection to use Azure Secret values
                _connectionString = string.Format(_connectionString,
                    dbSettings.ServerName, dbSettings.Port, dbSettings.DatabaseName, dbSettings.UserName, dbSettings.UserPassword);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating connection string.");
                throw;
            }
            return;
        }

        /// <summary>
        /// Save the customer message to the database.
        /// </summary>
        /// <param name="customerMessage">The inquiry message with the customer's contact info</param>
        /// <returns>void</returns>
        public async Task SaveCustomerMessageAsync(CustomerMessage customerMessage)
        {
            try
            {
                await _context.CustomerMessages.AddAsync(customerMessage);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Customer message saved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving customer message.");
                throw;
            }
        }
    }
}