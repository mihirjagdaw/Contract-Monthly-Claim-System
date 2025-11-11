using ST10449392_CLDV6212_POE.Models;

namespace ST10449392_CLDV6212_POE.Services
{
    public interface IUserService
    {
        Task<User?> ValidateUserAsync(string username, string password);
    }
}
