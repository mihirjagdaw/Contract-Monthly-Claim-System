using Azure.Data.Tables;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ST10449392_CLDV6212_POE.Data;
using ST10449392_CLDV6212_POE.Models;
using ST10449392_CLDV6212_POE.Services;

namespace ST10449392_CLDV6212_POE.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly TableStorageService _tableStorageService;

        public CartController(ApplicationDbContext context, TableStorageService tableStorageService)
        {
            _context = context;
            _tableStorageService = tableStorageService;
        }

        // Show items in user's cart
        public async Task<IActionResult> Index()
        {
            var username = HttpContext.Session.GetString("Username");
            if (username == null) return RedirectToAction("Login", "Account");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            var cartItems = await _context.CartItems.Where(c => c.UserId == user.UserId).ToListAsync();

            foreach (var item in cartItems)
            {
                string partitionKey = "Product";
                string rowKey = item.ProductRowKey;
                item.Product = await _tableStorageService.GetProductAsync(partitionKey, rowKey);
            }

            return View(cartItems);
        }

        // Add a product to the cart
        public async Task<IActionResult> AddToCart(string productRowKey)
        {
            var username = HttpContext.Session.GetString("Username");
            if (username == null) return RedirectToAction("Login", "Account");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

            // Fetch product details from Azure Table before adding to cart
            string partitionKey = "Product"; // Your fixed partition key
            var product = await _tableStorageService.GetProductAsync(partitionKey, productRowKey);

            if (product == null)
            {
                // Handle product not found case properly, e.g. show error or redirect
                return NotFound("Product not found");
            }

            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(c => c.ProductRowKey == productRowKey && c.UserId == user.UserId);

            if (cartItem == null)
            {
                _context.CartItems.Add(new CartItem
                {
                    ProductRowKey = productRowKey,
                    UserId = user.UserId,
                    Quantity = 1,
                    ProductName= product.Product_Name // Set ProductName to avoid NULL insert error
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
                .Where(c => c.UserId == user.UserId)
                .ToListAsync();

            if (!cartItems.Any()) return RedirectToAction("Index");

            var order = new Order
            {
                UserId = user.UserId,
                Status = "Pending",
                OrderDate = DateTime.Now,
                OrderItems = cartItems.Select(c => new OrderItem
                {
                    ProductId= c.CartItemId,  // Use string key here
                    ProductRowKey = c.ProductRowKey,
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
