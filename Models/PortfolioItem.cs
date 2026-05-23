namespace CryptoTracker.Models;

public class PortfolioItem
{
    public int Id { get; set; }
    
    public int UserId { get; set; }
    
    // ex. "bitcoin"
    public string CoinId { get; set; } = string.Empty;
    
    // ex "Bitcoin"
    public string CoinName { get; set; } = string.Empty;

    // ex "BTC"
    public string Symbol { get; set; } = string.Empty;

    // how much Coin bought
    public decimal Amount { get; set; }

    // buying price in USD
    public decimal BuyPrice { get; set; }

    public DateTime PurchasedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
}
