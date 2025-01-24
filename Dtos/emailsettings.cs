namespace ContactSvc.Dtos
{
    /// <summary>
    /// Represents the email information needed to forward the customer's inquiry to the corporate mailbox
    /// </summary> 
    public class EmailSettings
    {
        public string? SmtpServer { get; set; }
        public int SmtpPort { get; set; }
        public string? SmtpUsername { get; set; }
        public string? SmtpPassword { get; set; }
        public string? MailSender { get; set; }
    }
}