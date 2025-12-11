using CustomerCare.Models.Enums;
using CustomerCare.Models.Identity;
using System.ComponentModel.DataAnnotations;

namespace CustomerCare.Models.Entities
{
    public class Ticket
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        public TicketPriority Priority { get; set; }

        public TicketStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        [Required]
        public string CreatedById { get; set; }
        public ApplicationUser CreatedBy { get; set; }

        public string? AssignedToId { get; set; }
        public ApplicationUser? AssignedTo { get; set; }

        public int? CategoryId { get; set; }
        public TicketCategory? Category { get; set; }

        public ICollection<TicketAttachment> Attachments { get; set; }
        public ICollection<TicketComment> Comments { get; set; }
    }
}
