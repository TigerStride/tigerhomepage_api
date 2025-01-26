using System.Threading.Tasks;
using ContactSvc.Dtos;
using Microsoft.Extensions.Logging;

namespace ContactSvc.Data
{
    public class ContactRepo
    {
        private readonly ContactDbContext _context;
        private readonly ILogger<ContactRepo> _logger;

        public ContactRepo(ContactDbContext context, ILogger<ContactRepo> logger)
        {
            _context = context;
            _logger = logger;
        }

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