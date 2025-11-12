using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ST10449392_CLDV6212_POE.Data;
using ST10449392_CLDV6212_POE.Models;

namespace ST10449392_CLDV6212_POE.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Show items in user's cart
        public async Task<IActionResult> Index()
        {
            var username = HttpContext.Session.GetString("Username");
            if (username == null) return RedirectToAction("Login", "Account");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            var cartItems = await _context.CartItems
                .Include(c => c.Product)
                .Where(c => c.UserId == user.UserId)
                .ToListAsync();

            return View(cartItems);
        }

        // Add a product to the cart
        public async Task<IActionResult> AddToCart(int productId)
        {
            var username = HttpContext.Session.GetString("Username");
            if (username == null) return RedirectToAction("Login", "Account");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(c => c.ProductId == productId && c.UserId == user.UserId);

            if (cartItem == null)
            {
                _context.CartItems.Add(new CartItem
                {
                    ProductId = productId,
                    UserId = user.UserId,
                    Quantity = 1
                });
            }
            else
            {
                cartItem.Quantity++;
                _context.CartItems.Update(cartItem);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // Remove an item
        public async Task<IActionResult> RemoveFromCart(int id)
        {
            var cartItem = await _context.CartItems.FindAsync(id);
            if (cartItem != null)
            {
                _context.CartItems.Remove(cartItem);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        // Confirm order
        public async Task<IActionResult> ConfirmOrder()
        {
            var username = HttpContext.Session.GetString("Username");
            if (username == null) return RedirectToAction("Login", "Account");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            var cartItems = await _context.CartItems
                .Include(c => c.Product)
                .Where(c => c.UserId == user.UserId)
                .ToListAsync();

            if (!cartItems.Any()) return RedirectToAction("Index");

            var order = new Order
            {
                UserId = user.UserId,
                Status = "Pending",
                OrderItems = cartItems.Select(c => new OrderItem
                {
                    ProductId = c.ProductId,
                    Quantity = c.Quantity
                }).ToList()
            };

            _context.Orders.Add(order);
            _context.CartItems.RemoveRange(cartItems);
            await _context.SaveChangesAsync();

            return RedirectToAction("OrderSuccess");
        }

        public IActionResult OrderSuccess()
        {
            return View();
        }
    }
}
