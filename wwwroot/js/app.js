// Main page logic

let allCoins = [];  // Caching loaded coins to search

// Loading and rendering price tables
async function loadPrices() {
    try {
        const res = await fetch('/api/crypto/prices?perPage=20');
        if (!res.ok) throw new Error('API Error');

        allCoins = await res.json();
        renderTable(allCoins);
        document.getElementById('updateBadge').innerHTML =
            '<i class="bi bi-circle-fill"></i> Actual';
    } catch (err) {
        document.getElementById('cryptoTableBody').innerHTML = `
        <tr><td colspan="7" class="text-center py-4 text-danger">
            <i class="bi bi-exclamation-triangle fs-3"></i>
            <p class="mt-2">Unable to load the data. Check the connection.</p>
            <button class="btn btn-outline-warning btn-sm" onclick="loadPrices()">Retry</button>
        </td></tr>`;
    }
}

function renderTable(coins) {
    const tbody = document.getElementById('cryptoTableBody');
    if (!coins.length) {
        tbody.innerHTML = '<tr><td colspan="7" class="text-center py-4 text-secondary">Nothing was found</td></tr>';
        return;
    }

    tbody.innerHTML = coins.map(coin => {
        const change = coin.priceChange24h ?? coin.price_change_percentage_24h ?? 0;
        const changeClass = change >= 0 ? 'text-success' : 'text-danger';
        const changeIcon = change >= 0 ? '▲' : '▼';
        const changeFormatted = `${changeIcon} ${Math.abs(change).toFixed(2)}%`;

        return `
        <tr>
            <td class="text-secondary">${coin.marketCapRank ?? coin.market_cap_rank}</td>
            <td>
                <div class="d-flex align-items-center gap-2">
                    <img src="${coin.image}" alt="${coin.name}" width="28" height="28"
                    class="rounded-circle" loading="lazy"
                    onerror="this.style.display='none'" />
                    <div>
                        <span class="fw-semibold">${coin.name}</span>
                        <span class="text-secondary ms-1 small">${coin.symbol?.toUpperCase()}</span>
                    </div>
                </div>
            </td>
            <td class="text-end fw-bold">${formatUSD(coin.currentPrice ?? coin.current_price)}</td>
            <td class="text-end ${changeClass}">${changeFormatted}</td>
            <td class="text-end d-none d-md-table-cell text-secondary">
                $${formatLarge(coin.totalVolume ?? coin.total_volume)}
            </td>
            <td class="text-end d-none d-lg-table-cell text-secondary">
                ${formatUSD(coin.high24h ?? coin.high_24h)}
            </td>
            <td class="text-end d-none d-lg-table-cell text-secondary">
                ${formatUSD(coin.low24h ?? coin.low_24h)}
            </td>
        </tr>`;
    }).join('');
}

// Formatting large numbers
function formatLarge(n) {
    if (!n) return '—';
    if (n >= 1e9) return (n / 1e9).toFixed(2) + 'B';
    if (n >= 1e6) return (n / 1e6).toFixed(2) + 'M';
    return n.toLocaleString();
}

// Search rhrough table on client
document.addEventListener('DOMContentLoaded', () => {
    const searchInput = document.getElementById('searchInput');
    if (searchInput) {
        searchInput.addEventListener('input', () => {
            const q = searchInput.value.trim().toLowerCase();
            if (!q) { renderTable(allCoins); return; }
            const filtered = allCoins.filter(c =>
                c.name.toLowerCase().includes(q) ||
                c.symbol.toLowerCase().includes(q)
            );
            renderTable(filtered);
        });
    }

    loadPrices();
    // Autorefresh once per 2 mins
    setInterval(loadPrices, 2 * 60 * 1000);
});
