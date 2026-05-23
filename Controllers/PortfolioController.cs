using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using CryptoTracker.Data;
using CryptoTracker.Models;

namespace CryptoTracker.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PortfolioController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IHttpClientFactory _httpClientFactory;
    private const string SessionUserIdKey = "UserId";

    public PortfolioController(AppDbContext db, IHttpClientFactory httpClientFactory)
    {
        _db = db;
        _httpClientFactory = httpClientFactory;
    }

    // Helper method to get userId from the session
    private int? GetCurrentUserId() => HttpContext.Session.GetInt32(SessionUserIdKey);

    // GET /api/portfolio — получить весь портфель с актуальными ценами
    [HttpGet]
    public async Task<IActionResult> GetPortfolio()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(new { message = "Log in into the system" });

        var items = await _db.PortfolioItems
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.PurchasedAt)
            .ToListAsync();

        if (!items.Any())
            return Ok(new { items = new List<PortfolioResponseDto>(), totalValue = 0m, totalProfit = 0m, totalProfitPercent = 0m });

        // Getting actual data from CoinGecko
        var coinIds = string.Join(",", items.Select(i => i.CoinId).Distinct());
        var priceMap = await FetchCurrentPrices(coinIds);

        var result = items.Select(item =>
        {
            priceMap.TryGetValue(item.CoinId, out var coinData);
            return new PortfolioResponseDto
            {
                Id = item.Id,
                CoinId = item.CoinId,
                CoinName = item.CoinName,
                Symbol = item.Symbol,
                Image = coinData?.Image ?? "",
                Amount = item.Amount,
                BuyPrice = item.BuyPrice,
                CurrentPrice = coinData?.CurrentPrice ?? item.BuyPrice,
                PriceChange24h = coinData?.PriceChange24h ?? 0,
                PurchasedAt = item.PurchasedAt
            };
        }).ToList();

        var totalValue = result.Sum(r => r.TotalValue);
        var totalCost = items.Sum(i => i.Amount * i.BuyPrice);
        var totalProfit = totalValue - totalCost;
        var totalProfitPercent = totalCost > 0 ? (totalProfit / totalCost) * 100 : 0;

        return Ok(new
        {
            items = result,
            totalValue,
            totalProfit,
            totalProfitPercent
        });
    }

    [HttpPost]
    public async Task<IActionResult> AddItem([FromBody] AddPortfolioItemDto dto)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(new { message = "Log in into the system" });

        if (string.IsNullOrWhiteSpace(dto.CoinId))
            return BadRequest(new { message = "Specify coin" });

        if (dto.Amount <= 0)
            return BadRequest(new { message = "Amount should be more than 0" });

        if (dto.BuyPrice < 0)
            return BadRequest(new { message = "Buying price cannot be negative" });

        var item = new PortfolioItem
        {
            UserId = userId.Value,
            CoinId = dto.CoinId,
            CoinName = dto.CoinName,
            Symbol = dto.Symbol.ToUpper(),
            Amount = dto.Amount,
            BuyPrice = dto.BuyPrice,
            PurchasedAt = DateTime.UtcNow
        };

        _db.PortfolioItems.Add(item);
        await _db.SaveChangesAsync();

        return Created($"/api/portfolio/{item.Id}", new { message = "Coin added", id = item.Id });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteItem(int id)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(new { message = "Log in into the system" });

        var item = await _db.PortfolioItems
            .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

        if (item == null)
            return NotFound(new { message = "Item was not found" });

        _db.PortfolioItems.Remove(item);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Item was deleted" });
    }

    // Get prices from CoinGecko and return Dictionary<coinId, data>
    private async Task<Dictionary<string, CoinMarketDto>> FetchCurrentPrices(string coinIds)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("CoinGecko");
            var url = $"coins/markets?vs_currency=usd&ids={coinIds}&order=market_cap_desc&sparkline=false";
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var coins = JsonSerializer.Deserialize<List<CoinMarketDto>>(json) ?? new();
            return coins.ToDictionary(c => c.Id);
        }
        catch
        {
            return new Dictionary<string, CoinMarketDto>();
        }
    }
}
