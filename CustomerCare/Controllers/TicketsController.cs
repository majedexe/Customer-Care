using CustomerCare.Data;
using CustomerCare.Models.Entities;
using CustomerCare.Models.Enums;
using CustomerCare.Models.Identity;
using CustomerCare.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CustomerCare.Controllers
{
    [Authorize]
    public class TicketsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public TicketsController(ApplicationDbContext context,UserManager<ApplicationUser> userManager,IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _env = env;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var tickets = await _context.Tickets
                .Include(t => t.Category)
                .Where(t => t.CreatedById == userId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return View(tickets);
        }


        private async Task<TicketAttachment> SaveAttachmentAsync(int ticketId, IFormFile file)
        {
            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "tickets");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var extension = Path.GetExtension(file.FileName);
            var uniqueName = $"{Guid.NewGuid()}{extension}";
            var physicalPath = Path.Combine(uploadsFolder, uniqueName);

            using (var stream = new FileStream(physicalPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return new TicketAttachment
            {
                TicketId = ticketId,
                FileName = uniqueName,
                OriginalFileName = file.FileName,
                FilePath = $"/uploads/tickets/{uniqueName}",
                UploadedAt = DateTime.UtcNow
            };
        }

        private async Task<CommentAttachment> SaveCommentAttachmentAsync(int commentId, IFormFile file)
        {
            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "comments");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var extension = Path.GetExtension(file.FileName);
            var uniqueName = $"{Guid.NewGuid()}{extension}";
            var physicalPath = Path.Combine(uploadsFolder, uniqueName);

            using (var stream = new FileStream(physicalPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return new CommentAttachment
            {
                TicketCommentId = commentId,
                FileName = uniqueName,
                OriginalFileName = file.FileName,
                FilePath = $"/uploads/comments/{uniqueName}",
                UploadedAt = DateTime.UtcNow
            };
        }

        public async Task<IActionResult> Create()
        {
            var vm = new TicketCreateViewModel
            {
                Priority = TicketPriority.Medium,
                Categories = await _context.TicketCategories
                    .OrderBy(c => c.Name)
                    .Select(c => new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = c.Name
                    })
                    .ToListAsync()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TicketCreateViewModel model)
        {
            // 1) Check if server-side validation passed
            if (!ModelState.IsValid)
            {
                // Collect errors just to see them in the view
                var allErrors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                TempData["TicketErrorDebug"] = "ModelState invalid: " + string.Join(" | ", allErrors);

                model.Categories = await _context.TicketCategories
                    .OrderBy(c => c.Name)
                    .Select(c => new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = c.Name
                    })
                    .ToListAsync();

                return View(model);
            }

            // 2) Get current user
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                TempData["TicketErrorDebug"] = "UserId is null/empty – are you logged in?";
                model.Categories = await _context.TicketCategories
                    .OrderBy(c => c.Name)
                    .Select(c => new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = c.Name
                    })
                    .ToListAsync();

                return View(model);
            }

            var ticket = new Ticket
            {
                Title = model.Title,
                Description = model.Description,
                Priority = model.Priority,
                Status = TicketStatus.Open,
                CategoryId = model.CategoryId,
                CreatedAt = DateTime.UtcNow,
                CreatedById = userId
            };

            _context.Tickets.Add(ticket);

            try
            {
                var affected = await _context.SaveChangesAsync();
                TempData["TicketSuccessDebug"] = $"Ticket saved. Rows affected: {affected}. New Id: {ticket.Id}";
            }
            catch (Exception ex)
            {
                TempData["TicketErrorDebug"] = "Error saving ticket: " + ex.Message;
                model.Categories = await _context.TicketCategories
                    .OrderBy(c => c.Name)
                    .Select(c => new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = c.Name
                    })
                    .ToListAsync();

                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }



        [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Employee)]
        public async Task<IActionResult> Manage(TicketStatus? status,TicketPriority? priority,int? categoryId,
            string? search,int page = 1,int pageSize = 10)
        {
            var query = _context.Tickets
                .Include(t => t.Category)
                .Include(t => t.CreatedBy)
                .Include(t => t.AssignedTo)
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(t => t.Status == status.Value);
            }

            if (priority.HasValue)
            {
                query = query.Where(t => t.Priority == priority.Value);
            }

            if (categoryId.HasValue)
            {
                query = query.Where(t => t.CategoryId == categoryId.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                query = query.Where(t =>
                    t.Title.Contains(search) ||
                    t.Description.Contains(search) ||
                    t.CreatedBy.Email.Contains(search));
            }

            query = query.OrderByDescending(t => t.CreatedAt);

            var totalCount = await query.CountAsync();

            var tickets = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var categories = await _context.TicketCategories
                .OrderBy(c => c.Name)
                .ToListAsync();

            var pagedResult = new PagedResult<Ticket>
            {
                Items = tickets,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };

            ViewBag.Categories = categories;
            ViewBag.SelectedStatus = status;
            ViewBag.SelectedPriority = priority;
            ViewBag.SelectedCategoryId = categoryId;
            ViewBag.Search = search;

            return View(pagedResult);
        }

        [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Employee)]
        public async Task<IActionResult> MyAssigned(TicketStatus? status)
        {
            var userId = _userManager.GetUserId(User);

            var query = _context.Tickets
                .Include(t => t.Category)
                .Include(t => t.CreatedBy)
                .Where(t => t.AssignedToId == userId);

            if (status.HasValue)
            {
                query = query.Where(t => t.Status == status.Value);
            }

            var tickets = await query
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            ViewBag.SelectedStatus = status;

            return View(tickets);
        }


        [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Employee)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignToMe(int id)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null) return NotFound();

            var userId = _userManager.GetUserId(User);

            ticket.AssignedToId = userId;
            ticket.Status = TicketStatus.InProgress;
            ticket.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Manage));
        }

        [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Employee)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeStatus(int id, TicketStatus status)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null) return NotFound();

            ticket.Status = status;
            ticket.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Manage));
        }

        public async Task<IActionResult> Details(int id)
        {
            var ticket = await _context.Tickets
                .Include(t => t.Category)
                .Include(t => t.CreatedBy)
                .Include(t => t.AssignedTo)
                .Include(t => t.Attachments)
                .Include(t => t.Comments)
                    .ThenInclude(c => c.CreatedBy)
                .Include(t => t.Comments)
                    .ThenInclude(c => c.Attachments)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (ticket == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            var isStaff = User.IsInRole(RoleNames.Admin) || User.IsInRole(RoleNames.Employee);

            if (!isStaff && ticket.CreatedById != userId)
            {
                return Forbid();
            }

            var vm = new TicketDetailsViewModel
            {
                Ticket = ticket
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int id, TicketDetailsViewModel model)
        {
            var ticket = await _context.Tickets
                .Include(t => t.Comments)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (ticket == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            var isStaff = User.IsInRole(RoleNames.Admin) || User.IsInRole(RoleNames.Employee);

            if (string.IsNullOrWhiteSpace(model.NewCommentMessage))
            {
                ModelState.AddModelError("NewCommentMessage", "Comment cannot be empty.");
            }

            if (!ModelState.IsValid)
            {
                ticket = await _context.Tickets
                    .Include(t => t.Category)
                    .Include(t => t.CreatedBy)
                    .Include(t => t.AssignedTo)
                    .Include(t => t.Attachments)
                    .Include(t => t.Comments)
                        .ThenInclude(c => c.CreatedBy)
                    .Include(t => t.Comments)
                        .ThenInclude(c => c.Attachments)
                    .FirstOrDefaultAsync(t => t.Id == id);

                var vm = new TicketDetailsViewModel { Ticket = ticket };
                return View("Details", vm);
            }

            var comment = new TicketComment
            {
                TicketId = id,
                Message = model.NewCommentMessage.Trim(),
                CreatedAt = DateTime.UtcNow,
                CreatedById = userId,
                IsInternal = isStaff && model.NewCommentIsInternal
            };

            _context.TicketComments.Add(comment);
            await _context.SaveChangesAsync();

            if (model.NewCommentAttachment != null && model.NewCommentAttachment.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".pdf" };
                var ext = Path.GetExtension(model.NewCommentAttachment.FileName).ToLowerInvariant();

                if (allowedExtensions.Contains(ext))
                {
                    var attachment = await SaveCommentAttachmentAsync(comment.Id, model.NewCommentAttachment);
                    _context.CommentAttachments.Add(attachment);
                    await _context.SaveChangesAsync();
                }
                else
                {
                }
            }

            return RedirectToAction(nameof(Details), new { id });
        }

    }
}
