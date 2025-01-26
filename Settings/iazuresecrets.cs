/// 
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ContactSvc.Dtos;

namespace ContactSvc.Settings
{
    public interface IAzureSecrets
    {
        Task<EmailSettings> GetEmailSettingsAsync(ILogger logger, IConfiguration configuration);
        Task<DBSettings> GetDBSettingsAsync(ILogger logger, IConfiguration configuration);
    }
}