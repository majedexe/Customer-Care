using CustomerCare.Models.Identity;
using System.ComponentModel.DataAnnotations;

namespace CustomerCare.Models.Entities
{
    public class TicketComment
    {
        public int Id { get; set; }

        [Required]
        public string Message { get; set; }

        public DateTime CreatedAt { get; set; }

        public int TicketId { get; set; }
        public Ticket Ticket { get; set; }

        [Required]
        public string CreatedById { get; set; }
        public ApplicationUser CreatedBy { get; set; }
        public bool IsInternal { get; set; }
    }
}
