using System;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using ContactSvc.Dtos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TigerStride.ContactSvc;

public static class AzureSettings
{
    public static async Task<EmailSettings> GetEmailSettingsAsync(ILogger<HttpTriggerContact>  logger, IConfiguration configuration)
    {
        try
        {
            // Create a SecretClient using DefaultAzureCredential
            string? keyVaultUrl = configuration["AzureSettings:KeyVaultURL"];
            if (string.IsNullOrEmpty(keyVaultUrl))
            {
                throw new ArgumentNullException(nameof(keyVaultUrl), "KeyVaultUrl cannot be null or empty.");
            }
            var client = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());

            // Fetch the email secrets
            logger.LogInformation($"Starting to fetch email settings from Azure {keyVaultUrl}.");
            EmailSettings emailSettings = new EmailSettings();
            emailSettings.SmtpServer = await GetSecretAsync(client, "MailSvr");
            emailSettings.SmtpPort = int.TryParse(await GetSecretAsync(client, "MailPort"), out int port) ? port : 587;
            emailSettings.SmtpUsername = await GetSecretAsync(client, "MailUser");
            emailSettings.SmtpPassword = await GetSecretAsync(client, "MailPwd");
            emailSettings.MailSender = await GetSecretAsync(client, "MailSender");

            Console.WriteLine($"Email settings: Svr:{emailSettings.SmtpServer}, Port:{emailSettings.SmtpPort}, User:{emailSettings.SmtpUsername}");
            logger.LogInformation($"Email settings: Svr:{emailSettings.SmtpServer}, Port:{emailSettings.SmtpPort}, User:{emailSettings.SmtpUsername}");
            //logger.LogInformation($"Email settings: {emailSettings}");

            return emailSettings;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetEmailSettings: {ex.Message}");
            logger.LogError(ex, "Error fetching email parms from Azure.");
            throw;
        }
    }

    private static async Task<string> GetSecretAsync(SecretClient client, string secretName)
    {
        KeyVaultSecret secret = await client.GetSecretAsync(secretName);
        return secret.Value;
    }
}
