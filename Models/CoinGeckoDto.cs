using System.Text.Json.Serialization;

namespace CryptoTracker.Models;

// One element from /coins/markets
public class CoinMarketDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("symbol")]
    public string Symbol { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("image")]
    public string Image { get; set; } = string.Empty;

    [JsonPropertyName("current_price")]
    public decimal CurrentPrice { get; set; }

    [JsonPropertyName("market_cap")]
    public long MarketCap { get; set; }

    [JsonPropertyName("market_cap_rank")]
    public int MarketCapRank { get; set; }

    [JsonPropertyName("price_change_percentage_24h")]
    public decimal PriceChange24h { get; set; }

    [JsonPropertyName("total_volume")]
    public long TotalVolume { get; set; }

    [JsonPropertyName("high_24h")]
    public decimal High24h { get; set; }

    [JsonPropertyName("low_24h")]
    public decimal Low24h { get; set; }
}

// DTO for portfolio with data from CoinGecko
public class PortfolioResponseDto
{
    public int Id { get; set; }
    public string CoinId { get; set; } = string.Empty;
    public string CoinName { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal BuyPrice { get; set; }
    public decimal CurrentPrice { get; set; }
    public decimal PriceChange24h { get; set; }
    public decimal TotalValue => Amount * CurrentPrice;
    public decimal ProfitLoss => (CurrentPrice - BuyPrice) * Amount;
    public decimal ProfitLossPercent => BuyPrice > 0
        ? ((CurrentPrice - BuyPrice) / BuyPrice) * 100 : 0;
    public DateTime PurchasedAt { get; set; }
}

// DTO for adding coin to portofolio (incoming request)
public class AddPortfolioItemDto
{
    public string CoinId { get; set; } = string.Empty;
    public string CoinName { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal BuyPrice { get; set; }
}
