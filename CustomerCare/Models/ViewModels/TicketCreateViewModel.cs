using CustomerCare.Models.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace CustomerCare.Models.ViewModels
{
    public class TicketCreateViewModel
    {
        [Required, StringLength(100)]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public TicketPriority Priority { get; set; }

        public int? CategoryId { get; set; }

        public IEnumerable<SelectListItem> Categories { get; set; }

        public IFormFile? Attachment { get; set; }
    }
}
