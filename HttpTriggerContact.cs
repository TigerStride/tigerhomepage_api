using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using ContactSvc.Dtos;
using MailKit.Net.Smtp;
using MimeKit;
// using Microsoft.Identity.Client;
using System.Threading.Tasks;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using ContactSvc.Settings;
using ContactSvc.Data;

namespace TigerStride.ContactSvc
{
    /// <summary>
    /// Implement the HttpTrigger for an Azure Function that responds to the POST of the contact form on the corporate homepage.
    /// </summary> 
    public partial class HttpTriggerContact
    {
        private readonly ILogger<HttpTriggerContact> _logger;  // Serilogger
        private readonly IConfiguration _configuration;
        private readonly IAzureSecrets _azureSecrets;
        private readonly ContactRepo _contactRepo;

        public HttpTriggerContact(ILogger<HttpTriggerContact> logger,
            IConfiguration configuration,
            IAzureSecrets azuresecrets,
            ContactRepo contactRepo)
        {
            _logger = logger;
            _configuration = configuration;
            _azureSecrets = azuresecrets;
            _contactRepo = contactRepo;
        }

        [Function("HttpTriggerContact")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
        {
            try
            {
                if (_logger == null)
                {
                    throw new ArgumentNullException(nameof(_logger));
                }
                _logger.LogInformation("----------------Begin Contact Function. -------------------");

                // Check the expected header
                string customHeader = req.Headers["X-Custom-Header"].ToString();
                string allowedHeader = "contact-inquiry";

                // Limit posts from our homepage unless dev
                if (string.IsNullOrEmpty(customHeader) || !customHeader.StartsWith(allowedHeader))
                {
                    throw new ArgumentException($"Invalid custom header: {customHeader}. HttpTriggerContact Post rejected.");
                }

                var form = await req.ReadFormAsync();
                var customerName = form["customerName"];
                var customerEmail = form["customerEmail"];
                var messageText = form["messageText"];

                var customerMessage = new CustomerMessage
                {
                    customerName = customerName,
                    customerEmail = customerEmail,
                    messageText = messageText
                };

                _logger.LogInformation($"Customer inquiry: {customerName}, {customerEmail}, {messageText}");

                // Get the email settings
                EmailSettings emailSettings = await _azureSecrets.GetEmailSettingsAsync();
                _logger.LogInformation($"Email settings: Svr:{emailSettings.SmtpServer}, Port:{emailSettings.SmtpPort}, User:{emailSettings.SmtpUsername}");

                // Save customer message to the database
                _logger.LogInformation($"Begin saving customer message to the database. {customerMessage}");
                DBSettings dBSettings = await _azureSecrets.GetDBSettingsAsync();
                _contactRepo.Connect(dBSettings);
                _logger.LogInformation("Connected.");
                await _contactRepo.SaveCustomerMessageAsync(customerMessage);
                _logger.LogInformation("End saving customer message to the database.");

                // Send email notification
                await SendContactFormEmailAsync(emailSettings, customerMessage);

                var response = new OkObjectResult(new { message = "Success" });

                // Add CORS header
                req.HttpContext.Response.Headers.Append("Access-Control-Allow-Origin", "https://www.tigerstridesolutions.com");
                req.HttpContext.Response.Headers.Append("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
                req.HttpContext.Response.Headers.Append("Access-Control-Allow-Headers", "Content-Type");

                _logger.LogInformation("Successful trigger execution.");
                return response;
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

        /// <summary>
        /// Sends an email notification for a new contact form submission.
        /// </summary>
        /// <param name="emailSettings">The email configuration settings.</param>
        /// <param name="customerMessage">The customer's contact form submission.</param>
        private async Task SendContactFormEmailAsync(EmailSettings emailSettings, CustomerMessage customerMessage)
        {
            _logger.LogInformation("Creating email message...");
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Tigerstride Contact Form", emailSettings.MailSender));
            message.To.Add(new MailboxAddress("Tigerstride Support", emailSettings.MailSender));
            message.Subject = $"New Contact Form Submission from {customerMessage.customerName}";

            var bodyBuilder = new BodyBuilder
            {
                TextBody = $"Name: {customerMessage.customerName}\nEmail: {customerMessage.customerEmail}\n\nMessage:\n{customerMessage.messageText}"
            };
            message.Body = bodyBuilder.ToMessageBody();

            _logger.LogInformation("Sending email...");
            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(emailSettings.SmtpServer, emailSettings.SmtpPort, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(emailSettings.SmtpUsername, emailSettings.SmtpPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
            _logger.LogInformation("Email sent successfully.");
        }
    }
}
