using CustomerCare.Data;
using CustomerCare.Models.Enums;
using CustomerCare.Models.Identity;
using CustomerCare.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CustomerCare.Controllers
{
    [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Employee)]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var vm = new DashboardViewModel();

            vm.TotalTickets = await _context.Tickets.CountAsync();
            vm.OpenTickets = await _context.Tickets.CountAsync(t => t.Status == TicketStatus.Open);
            vm.InProgressTickets = await _context.Tickets.CountAsync(t => t.Status == TicketStatus.InProgress);
            vm.ResolvedTickets = await _context.Tickets.CountAsync(t => t.Status == TicketStatus.Resolved);
            vm.ClosedTickets = await _context.Tickets.CountAsync(t => t.Status == TicketStatus.Closed);

            vm.ByPriority = await _context.Tickets
                .GroupBy(t => t.Priority)
                .Select(g => new PriorityCountViewModel
                {
                    Priority = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            return View(vm);
        }
    }
}
