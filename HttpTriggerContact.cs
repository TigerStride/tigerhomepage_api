using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using ContactSvc.Dtos;
// using MailKit.Net.Smtp;
// using MimeKit;
// using Microsoft.Identity.Client;
using System.Threading.Tasks;
//using MailKit.Security;
using Microsoft.Extensions.Configuration;
using ContactSvc.Settings;
using ContactSvc.Data;

namespace TigerStride.ContactSvc
{
    /// <summary>
    /// Implement the HttpTrigger for an Azure Function that responds to the POST of the contact form on the corporate homepage.
    /// </summary> 
    public class HttpTriggerContact
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
                _logger.LogInformation("----------------Begin Contact Function. V.2.---------------");

                // Log the incoming request data
                _logger.LogInformation("Request Headers: {Headers}", req.Headers);
                _logger.LogInformation("Request Content-Type: {ContentType}", req.ContentType);

                // Check the expected header
                string customHeader = req.Headers["X-Custom-Header"].ToString();
                string allowedHeader = "contact-inquiry";
                // how can inquire if running in dev mode?  

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

                var response = new OkObjectResult(new { message = "Success" });

                // Add CORS header
                req.HttpContext.Response.Headers.Append("Access-Control-Allow-Origin", "https://www.tigerstridesolutions.com");
                req.HttpContext.Response.Headers.Append("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
                req.HttpContext.Response.Headers.Append("Access-Control-Allow-Headers", "Content-Type");

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

        // [Function("CheckHealth")]
        // public IActionResult CheckHealth([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
        // {
        //     try
        //     {
        //         _logger.LogInformation("Health check requested.");
        //         return new OkObjectResult(new { status = "Healthy" });
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogError(ex, "Health check failed.");
        //         return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        //     }
        // }
    }
}
