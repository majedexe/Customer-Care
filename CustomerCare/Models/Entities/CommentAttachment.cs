using System.ComponentModel.DataAnnotations;

namespace CustomerCare.Models.Entities
{
    public class CommentAttachment
    {
        public int Id { get; set; }

        [Required]
        public string FileName { get; set; }
        [Required]
        public string FilePath { get; set; }
        public DateTime UploadedAt { get; set; }
        public string? OriginalFileName { get; set; }
        public int TicketCommentId { get; set; }
        public TicketComment TicketComment { get; set; }
    }
}
