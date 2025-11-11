using Microsoft.EntityFrameworkCore;
using ST10449392_CLDV6212_POE.Data;
using ST10449392_CLDV6212_POE.Models;

namespace ST10449392_CLDV6212_POE.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;

        public UserService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User?> ValidateUserAsync(string username, string password)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username && u.PasswordHash == password);
        }
    }
}
