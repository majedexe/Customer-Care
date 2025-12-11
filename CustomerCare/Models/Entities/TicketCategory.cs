using System.ComponentModel.DataAnnotations;
using System.Net.Sockets;

namespace CustomerCare.Models.Entities
{
    public class TicketCategory
    {
        public int Id { get; set; }

        [Required, StringLength(50)]
        public string Name { get; set; }

        [StringLength(200)]
        public string? Description { get; set; }

        public ICollection<Ticket> Tickets { get; set; }
    }
}
