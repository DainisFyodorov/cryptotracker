namespace CryptoTracker.Models;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;  // bcrypt for hashing
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // List of coins in portfolio
    public List<PortfolioItem> Portfolio { get; set; } = new();
}
