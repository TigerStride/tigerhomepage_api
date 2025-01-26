using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using ContactSvc.Dtos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;


namespace ContactSvc.Settings;

/// <summary>
/// Retrieves secrets from the Azure Key Vault.
/// </summary>
public class AzureSecrets : IAzureSecrets
{
    /// <summary>
    /// Retrieves Email settings from the Azure Key Vault.
    /// </summary>
    /// <param name="logger">The logger instance for logging information and errors.</param>
    /// <param name="configuration">The configuration instance to access application settings.</param>
    /// <returns>The email settings as an EmailSettings object.</returns>
    public async Task<EmailSettings> GetEmailSettingsAsync(ILogger logger, IConfiguration configuration)
    {
        try
        {
            string? keyVaultUrl = configuration["AzureSettings:KeyVaultURL"];
            if (string.IsNullOrEmpty(keyVaultUrl))
            {
                throw new ArgumentNullException(nameof(keyVaultUrl), "KeyVaultUrl cannot be null or empty.");
            }

            // Create a SecretClient using DefaultAzureCredential
            var client = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());

            // Fetch the email secrets
            logger.LogInformation($"Starting to fetch email settings from Azure {keyVaultUrl}.");
            EmailSettings emailSettings = new EmailSettings();
            emailSettings.SmtpServer = await GetSecretAsync(client, "MailSvr");
            emailSettings.SmtpPort = int.TryParse(await GetSecretAsync(client, "MailPort"), out int port) ? port : 587;
            emailSettings.SmtpUsername = await GetSecretAsync(client, "MailUser");
            emailSettings.SmtpPassword = await GetSecretAsync(client, "MailPwd");
            emailSettings.MailSender = await GetSecretAsync(client, "MailSender");

            logger.LogInformation($"Email settings: Svr:{emailSettings.SmtpServer}, Port:{emailSettings.SmtpPort}, User:{emailSettings.SmtpUsername}");
            return emailSettings;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching email parms from Azure.");
            throw;
        }
    }

    /// <summary>
    /// Retrieves Database settings from the Azure Key Vault.
    /// </summary>
    /// <param name="logger">The logger instance for logging information and errors.</param>
    /// <param name="configuration">The configuration instance to access application settings.</param>
    /// <returns>The database settings as a DBSettings object.</returns>
    public async Task<DBSettings> GetDBSettingsAsync(ILogger logger, IConfiguration configuration)
    {
        try
        {
            string? keyVaultUrl = configuration["AzureSettings:KeyVaultURL"];
            if (string.IsNullOrEmpty(keyVaultUrl))
            {
                throw new ArgumentNullException(nameof(keyVaultUrl), "KeyVaultUrl cannot be null or empty.");
            }

            // Create a SecretClient using DefaultAzureCredential
            var client = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());

            // Fetch the email secrets
            logger.LogInformation($"Starting to fetch database settings from Azure {keyVaultUrl}.");
            DBSettings settings = new DBSettings();
            settings.ServerName = await GetSecretAsync(client, "DBServer");
            settings.DatabaseName = await GetSecretAsync(client, "DBName");
            settings.UserName = await GetSecretAsync(client, "DBUser");
            settings.UserPassword = await GetSecretAsync(client, "DBPwd");


            logger.LogInformation($"Database settings: Svr:{settings.ServerName}, User:{settings.UserName}");
            return settings;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching database settings from Azure.");
            throw;
        }
    }
    
    /// <summary>
    /// Retrieves a secret value from Azure Key Vault.
    /// </summary>
    /// <param name="client">The SecretClient instance used to interact with Azure Key Vault.</param>
    /// <param name="secretName">The name of the secret to retrieve.</param>
    /// <returns>The value of the secret as a string.</returns>
    private static async Task<string> GetSecretAsync(SecretClient client, string secretName)
    {
        KeyVaultSecret secret = await client.GetSecretAsync(secretName);
        return secret.Value;
    }
}
