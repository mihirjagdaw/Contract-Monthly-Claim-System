using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ST10449392_CLDV6212_POE.Data;
using ST10449392_CLDV6212_POE.Models;

namespace ST10449392_CLDV6212_POE.Controllers
{
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Admin: view all orders
        public async Task<IActionResult> Index()
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin") return RedirectToAction("AccessDenied");

            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        // Customer: view their own orders
        public async Task<IActionResult> CustomerOrders()
        {
            var username = HttpContext.Session.GetString("Username");
            if (username == null) return RedirectToAction("Login", "Account");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null) return RedirectToAction("Login", "Account");

            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Where(o => o.UserId == user.UserId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        // Admin: view order details
        public async Task<IActionResult> Details(int id)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin") return RedirectToAction("AccessDenied");

            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null) return NotFound();

            return View(order);
        }

        // Admin: update order status
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int orderId, string status)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin") return RedirectToAction("AccessDenied");

            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return NotFound();

            order.Status = status;
            _context.Update(order);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // Optional Access Denied page
        public IActionResult AccessDenied()
        {
            return View();
        }

        // Customer: view details of their own order
        public async Task<IActionResult> CustomerOrderDetails(int id)
        {
            var username = HttpContext.Session.GetString("Username");
            if (username == null) return RedirectToAction("Login", "Account");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null) return RedirectToAction("Login", "Account");

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.OrderId == id && o.UserId == user.UserId);

            if (order == null) return NotFound();

            return View(order);
        }
    }
}
