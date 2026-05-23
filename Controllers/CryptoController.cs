using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using CryptoTracker.Models;

namespace CryptoTracker.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CryptoController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<CryptoController> _logger;

    // Simple in-memory кэш — to avoid getting data from CoinGecko on each request (each page refresh)
    private static List<CoinMarketDto>? _cachedPrices;
    private static DateTime _cacheExpiry = DateTime.MinValue;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(2);

    public CryptoController(IHttpClientFactory httpClientFactory, ILogger<CryptoController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    // GET /api/crypto/prices?page=1&perPage=20
    [HttpGet("prices")]
    public async Task<IActionResult> GetPrices(
        [FromQuery] int page = 1,
        [FromQuery] int perPage = 20,
        [FromQuery] string currency = "usd")
    {
        // Возвращаем кэш если он свежий
        if (_cachedPrices != null && DateTime.UtcNow < _cacheExpiry)
            return Ok(_cachedPrices);

        try
        {
            var client = _httpClientFactory.CreateClient("CoinGecko");
            var url = $"coins/markets?vs_currency={currency}&order=market_cap_desc" +
                      $"&per_page={perPage}&page={page}&sparkline=false" +
                      $"&price_change_percentage=24h";

            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var coins = JsonSerializer.Deserialize<List<CoinMarketDto>>(json);

            if (coins == null)
                return StatusCode(500, new { message = "Unable to retrieve data from CoinGecko" });

            // Сохраняем в кэш
            _cachedPrices = coins;
            _cacheExpiry = DateTime.UtcNow.Add(CacheDuration);

            return Ok(coins);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error during request to CoinGecko API");
            // Возвращаем старый кэш если есть
            if (_cachedPrices != null)
                return Ok(_cachedPrices);
            return StatusCode(503, new { message = "CoinGecko API is unavailable. Try again later." });
        }
    }

    // GET /api/crypto/prices/by-ids?ids=bitcoin,ethereum,solana
    [HttpGet("prices/by-ids")]
    public async Task<IActionResult> GetPricesByIds([FromQuery] string ids, [FromQuery] string currency = "usd")
    {
        if (string.IsNullOrWhiteSpace(ids))
            return BadRequest(new { message = "Specify ids of coins" });

        try
        {
            var client = _httpClientFactory.CreateClient("CoinGecko");
            var url = $"coins/markets?vs_currency={currency}&ids={ids}&order=market_cap_desc" +
                      $"&sparkline=false&price_change_percentage=24h";

            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var coins = JsonSerializer.Deserialize<List<CoinMarketDto>>(json);

            return Ok(coins ?? new List<CoinMarketDto>());
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error during request to CoinGecko API");
            return StatusCode(503, new { message = "CoinGecko API is unavailable. Try again later." });
        }
    }

    // GET /api/crypto/search?query=bit
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string query)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            return BadRequest(new { message = "Enter at least 2 symbols for search" });

        try
        {
            var client = _httpClientFactory.CreateClient("CoinGecko");
            var response = await client.GetAsync($"search?query={Uri.EscapeDataString(query)}");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var coins = doc.RootElement.GetProperty("coins");

            // Берём первые 10 результатов поиска
            var results = new List<object>();
            int count = 0;
            foreach (var coin in coins.EnumerateArray())
            {
                if (count++ >= 10) break;
                results.Add(new
                {
                    id = coin.GetProperty("id").GetString(),
                    name = coin.GetProperty("name").GetString(),
                    symbol = coin.GetProperty("symbol").GetString(),
                    thumb = coin.GetProperty("thumb").GetString()
                });
            }

            return Ok(results);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error during search in CoinGecko API");
            return StatusCode(503, new { message = "CoinGecko API is unavailable. Try again later." });
        }
    }
}
