using CustomerCare.Models.Entities;

namespace CustomerCare.Models.ViewModels
{
    public class TicketDetailsViewModel
    {
        public Ticket Ticket { get; set; }
        public string NewCommentMessage { get; set; }
        public bool NewCommentIsInternal { get; set; }
        public IFormFile? NewCommentAttachment { get; set; }
    }
}
