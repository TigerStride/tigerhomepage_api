/// <summary>
/// Represents the customer's contact information and inquiry message from the homepage Contact form
/// </summary> 
namespace ContactSvc.Dtos
{
    public class CustomerMessage
    {
        internal readonly int? Id = 0;

        public string? customerEmail { get; set; }
        public string? customerName { get; set; }
        public string? messageText { get; set; }
    }
}