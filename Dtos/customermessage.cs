using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// Represents the customer's contact information and inquiry message from the homepage Contact form
/// </summary> 
namespace ContactSvc.Dtos
{
    [Table("customer_message")]
    public class CustomerMessage
    {
        [Key]
        [Column("Id")]
        internal readonly int? Id = null;

        [Column("customerEmail")]
        public string? customerEmail { get; set; }

        [Column("customerName")]
        public string? customerName { get; set; }

        [Column("messageText")]
        public string? messageText { get; set; }

    }
}