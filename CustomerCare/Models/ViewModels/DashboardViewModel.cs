using CustomerCare.Models.Enums;

namespace CustomerCare.Models.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalTickets { get; set; }
        public int OpenTickets { get; set; }
        public int InProgressTickets { get; set; }
        public int ResolvedTickets { get; set; }
        public int ClosedTickets { get; set; }

        public List<PriorityCountViewModel> ByPriority { get; set; }
    }

    public class PriorityCountViewModel
    {
        public TicketPriority Priority { get; set; }
        public int Count { get; set; }
    }
}
