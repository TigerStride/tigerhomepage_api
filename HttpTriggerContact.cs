using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using ContactSvc.Dtos;
// using MailKit.Net.Smtp;
// using MimeKit;
// using Microsoft.Identity.Client;
using System.Threading.Tasks;
//using MailKit.Security;
using Microsoft.Extensions.Configuration;

namespace TigerStride.ContactSvc
{
    public class HttpTriggerContact
    {
        private readonly ILogger<HttpTriggerContact> _logger;
        private readonly IConfiguration _configuration;

        public HttpTriggerContact(ILogger<HttpTriggerContact> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        [Function("HttpTriggerContact")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
        {
            try
            {
                if (_logger == null)
                {
                    throw new ArgumentNullException(nameof(_logger));
                }
                _logger.LogInformation("----------------Begin Contact Function.--------------------");
                var form = await req.ReadFormAsync();
                var customerName = form["customerName"];
                var customerEmail = form["customerEmail"];
                var messageText = form["messageText"];

                // var customerName = customerMessage.customerName;
                // var customerEmail = customerMessage.customerEmail;
                // var messageText = customerMessage.messageText;

                _logger.LogInformation($"Customer inquiry: {customerName}, {customerEmail}, {messageText}");

                // Get the email settings
                EmailSettings emailSettings = await AzureSettings.GetEmailSettingsAsync(_logger, _configuration);
                _logger.LogInformation($"Email settings: Svr:{emailSettings.SmtpServer}, Port:{emailSettings.SmtpPort}, User:{emailSettings.SmtpUsername}");

                // // Create the email message
                // var message = new MimeMessage();
                // message.From.Add(new MailboxAddress("CompanyName", emailSettings.MailSender));
                // message.To.Add(new MailboxAddress(customerName, customerEmail));
                // message.Subject = "Customer Inquiry";
                // message.Body = new TextPart("plain")
                // {
                //     Text = $"Name: {customerName}\nEmail: {customerEmail}\nMessage: {messageText}"
                // };

                // // Authenticate using OAuth 2.0
                // var cca = ConfidentialClientApplicationBuilder.Create("clientId")
                //     .WithClientSecret("clientSecret")
                //     .WithAuthority(new Uri($"https://login.microsoftonline.com/tenantId"))
                //     .Build();

                // var result = await cca.AcquireTokenForClient(new[] { "https://outlook.office365.com/.default" }).ExecuteAsync();

                // using var client = new SmtpClient();
                // await client.ConnectAsync(emailSettings.SmtpServer, emailSettings.SmtpPort, SecureSocketOptions.StartTls);
                // await client.AuthenticateAsync(new SaslMechanismOAuth2(emailSettings.SmtpUsername, result.AccessToken));
                // await client.SendAsync(message);
                // await client.DisconnectAsync(true);

                _logger.LogInformation("Email sent successfully.");
                return new OkObjectResult("Email sent successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email.");
                return new BadRequestObjectResult("Error sending email.");
            }
            finally
            {
                _logger?.LogInformation("----------------End Contact Function.--------------------");
            }
        }
    }
}
