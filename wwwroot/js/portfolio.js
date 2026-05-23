// Portfolio page logic

let searchTimeout = null;

// Load portfolio (authorization already checked in the <head>)
document.addEventListener('DOMContentLoaded', async () => {
    // Taking username from session
    try {
        const res = await fetch('/api/auth/me');
        if (res.ok) {
            const user = await res.json();
            const navUsername = document.getElementById('navUsername');
            if (navUsername) navUsername.textContent = user.username;
        }
    } catch { /* navbar will update auth.js */ }

    loadPortfolio();
    initCoinSearch();
});

// Loading and rendering a portfolio
async function loadPortfolio() {
    try {
        const res = await fetch('/api/portfolio');
        if (!res.ok) throw new Error();
        const data = await res.json();

        renderSummary(data.totalValue, data.totalProfit, data.totalProfitPercent);
        renderPortfolioTable(data.items);
    } catch {
        document.getElementById('portfolioTableBody').innerHTML = `
        <tr><td colspan="8" class="text-center py-4 text-danger">
            Error when loading portfolio
        </td></tr>`;
    }
}

// Summary cards
function renderSummary(totalValue, totalProfit, totalProfitPercent) {
    const profitColor = totalProfit >= 0 ? 'text-success' : 'text-danger';
    const profitIcon = totalProfit >= 0 ? '▲' : '▼';

    document.getElementById('totalValue').textContent = formatUSD(totalValue);
    document.getElementById('totalProfit').className = `h3 fw-bold ${profitColor}`;
    document.getElementById('totalProfit').textContent =
        `${profitIcon} ${formatUSD(Math.abs(totalProfit))}`;
    document.getElementById('totalProfitPercent').className = `h3 fw-bold ${profitColor}`;
    document.getElementById('totalProfitPercent').textContent =
        `${profitIcon} ${Math.abs(totalProfitPercent).toFixed(2)}%`;
}

// Table render
function renderPortfolioTable(items) {
    const tbody = document.getElementById('portfolioTableBody');

    if (!items || !items.length) {
        tbody.innerHTML = `
        <tr><td colspan="8" class="text-center py-5 text-secondary">
            <i class="bi bi-inbox fs-1 d-block mb-2"></i>
            Portfolio is empty. Add your first coin!
        </td></tr>`;
        return;
    }

    tbody.innerHTML = items.map(item => {
        const pl = item.profitLoss ?? (item.currentPrice - item.buyPrice) * item.amount;
        const plPct = item.profitLossPercent ?? (item.buyPrice > 0
            ? ((item.currentPrice - item.buyPrice) / item.buyPrice) * 100 : 0);
        const plClass = pl >= 0 ? 'text-success' : 'text-danger';
        const plIcon = pl >= 0 ? '▲' : '▼';
        const change24 = item.priceChange24h ?? 0;
        const change24Class = change24 >= 0 ? 'text-success' : 'text-danger';

        return `
        <tr>
            <td>
            <div class="d-flex align-items-center gap-2">
                ${item.image
                    ? `<img src="${item.image}" alt="${item.coinName}" width="28" height="28"
                        class="rounded-circle" loading="lazy" onerror="this.style.display='none'"/>`
                    : `<div style="width:28px;height:28px" class="rounded-circle bg-secondary"></div>`}
                <div>
                <span class="fw-semibold">${item.coinName}</span>
                <span class="text-secondary ms-1 small">${item.symbol?.toUpperCase()}</span>
                </div>
            </div>
            </td>
            <td class="text-end">${formatNumber(item.amount)}</td>
            <td class="text-end text-secondary">${formatUSD(item.buyPrice)}</td>
            <td class="text-end">${formatUSD(item.currentPrice)}</td>
            <td class="text-end fw-semibold">${formatUSD(item.totalValue ?? item.amount * item.currentPrice)}</td>
            <td class="text-end ${plClass}">${plIcon} ${formatUSD(Math.abs(pl))}<br/>
            <small>${Math.abs(plPct).toFixed(2)}%</small></td>
            <td class="text-end ${change24Class}">${change24 >= 0 ? '▲' : '▼'} ${Math.abs(change24).toFixed(2)}%</td>
            <td class="text-end">
            <button class="btn btn-outline-danger btn-sm"
                    onclick="deleteItem(${item.id})" title="Delete">
                <i class="bi bi-trash"></i>
            </button>
            </td>
        </tr>`;
    }).join('');
}

async function deleteItem(id) {
    if (!confirm('Are you sure you want to delete this item from portfolio?')) return;
    try {
        const res = await fetch(`/api/portfolio/${id}`, { method: 'DELETE' });
        if (res.ok) loadPortfolio();
        else alert('Deletion error');
    } catch {
        alert('Connection error');
    }
}

async function addCoin() {
    const coinId = document.getElementById('selectedCoinId').value;
    const coinName = document.getElementById('selectedCoinName').value;
    const symbol = document.getElementById('selectedCoinSymbol').value;
    const amount = parseFloat(document.getElementById('coinAmount').value);
    const buyPrice = parseFloat(document.getElementById('coinBuyPrice').value);
    const btn = document.getElementById('addBtn');

    if (!coinId || !coinName) { showAddAlert('Select coin from the list', 'danger'); return; }
    if (!amount || amount <= 0) { showAddAlert('Enter correct amount', 'danger'); return; }
    if (buyPrice < 0 || isNaN(buyPrice)) { showAddAlert('Enter correct buying price', 'danger'); return; }

    btn.disabled = true;
    btn.innerHTML = '<span class="spinner-border spinner-border-sm me-1"></span>Adding...';

    try {
        const res = await fetch('/api/portfolio', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ coinId, coinName, symbol, amount, buyPrice: buyPrice || 0 })
        });
        const data = await res.json();

        if (res.ok) {
            bootstrap.Modal.getInstance(document.getElementById('addModal')).hide();
            resetAddForm();
            loadPortfolio();
        } else {
            showAddAlert(data.message || 'Addition error', 'danger');
        }
    } catch {
        showAddAlert('Connection error', 'danger');
    } finally {
        btn.disabled = false;
        btn.innerHTML = '<i class="bi bi-plus-lg"></i> Add';
    }
}

function resetAddForm() {
    ['coinSearch', 'coinAmount', 'coinBuyPrice', 'selectedCoinId', 'selectedCoinName', 'selectedCoinSymbol']
        .forEach(id => document.getElementById(id).value = '');
    document.getElementById('addAlertBox').classList.add('d-none');
    document.getElementById('coinSearchResults').classList.add('d-none');
}

function showAddAlert(msg, type) {
    const box = document.getElementById('addAlertBox');
    box.className = `alert alert-${type}`;
    box.textContent = msg;
    box.classList.remove('d-none');
}

// Searching for coin during text enter (debounce 400ms)
function initCoinSearch() {
    const input = document.getElementById('coinSearch');
    const results = document.getElementById('coinSearchResults');

    input?.addEventListener('input', () => {
        clearTimeout(searchTimeout);
        const q = input.value.trim();

        if (q.length < 2) { results.classList.add('d-none'); return; }

        searchTimeout = setTimeout(async () => {
            try {
                const res = await fetch(`/api/crypto/search?query=${encodeURIComponent(q)}`);
                const coins = await res.json();

                if (!coins.length) {
                    results.innerHTML = '<div class="list-group-item bg-secondary text-secondary">Nothing was found</div>';
                } else {
                    results.innerHTML = coins.map(c => `
                <button type="button"
                class="list-group-item list-group-item-action bg-secondary text-light border-0"
                onclick="selectCoin('${c.id}','${c.name}','${c.symbol}')">
                    <img src="${c.thumb}" width="18" height="18" class="me-2 rounded-circle"
                        onerror="this.style.display='none'" />
                    ${c.name} <span class="text-secondary">${c.symbol?.toUpperCase()}</span>
                </button>`).join('');
                }
                results.classList.remove('d-none');
            } catch {
                results.classList.add('d-none');
            }
        }, 400);
    });

    // Hide results when clicking out
    document.addEventListener('click', e => {
        if (!input?.contains(e.target) && !results?.contains(e.target))
            results?.classList.add('d-none');
    });
}

// Select coin from dropdown menu
function selectCoin(id, name, symbol) {
    document.getElementById('selectedCoinId').value = id;
    document.getElementById('selectedCoinName').value = name;
    document.getElementById('selectedCoinSymbol').value = symbol;
    document.getElementById('coinSearch').value = `${name} (${symbol.toUpperCase()})`;
    document.getElementById('coinSearchResults').classList.add('d-none');
}
