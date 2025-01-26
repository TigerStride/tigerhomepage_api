namespace ContactSvc.Dtos
{
    /// <summary>
    /// Represents the database information needed to access the corporate database
    /// </summary> 
    public class DBSettings
    {
        public string? DatabaseName { get; set; }
        public string? UserName { get; set; }
        public string? UserPassword { get; set; }
        public string? ServerName { get; set; }
    }
}