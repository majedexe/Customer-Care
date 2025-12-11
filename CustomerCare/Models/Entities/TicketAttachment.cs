using System.ComponentModel.DataAnnotations;

namespace CustomerCare.Models.Entities
{
    public class TicketAttachment
    {
        public int Id { get; set; }

        [Required]
        public string FileName { get; set; }

        [Required]
        public string FilePath { get; set; }

        public DateTime UploadedAt { get; set; }

        public string? OriginalFileName { get; set; }

        public int TicketId { get; set; }
        public Ticket Ticket { get; set; }
    }
}
