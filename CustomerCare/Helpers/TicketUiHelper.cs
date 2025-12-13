using CustomerCare.Models.Enums;

namespace CustomerCare.Helpers
{
    public static class TicketUiHelper
    {
        public static string GetStatusBadgeClass(TicketStatus status)
        {
            return status switch
            {
                TicketStatus.Open => "badge bg-primary",
                TicketStatus.InProgress => "badge bg-warning text-dark",
                TicketStatus.Resolved => "badge bg-success",
                TicketStatus.Closed => "badge bg-secondary",
                _ => "badge bg-light text-dark"
            };
        }

        public static string GetPriorityBadgeClass(TicketPriority priority)
        {
            return priority switch
            {
                TicketPriority.Critical => "badge bg-danger",
                TicketPriority.High => "badge bg-warning text-dark",
                TicketPriority.Medium => "badge bg-info text-dark",
                TicketPriority.Low => "badge bg-light text-dark",
                _ => "badge bg-light text-dark"
            };
        }
    }
}
