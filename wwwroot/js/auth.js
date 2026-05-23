// Common auth uitilities
// This file is included on each page

// Checking user session and refreshing navbar
async function initAuth() {
    try {
        const res = await fetch('/api/auth/me');
        if (res.ok) {
            const data = await res.json();

            localStorage.setItem('username', data.username);
            updateNavbar(data.username, true);

            return data;
        } else {
            localStorage.removeItem('username');
            updateNavbar(null, false);

            return null;
        }
    } catch {
        updateNavbar(null, false);
        return null;
    }
}

// Refreshing navbar depending on auth status
function updateNavbar(username, isLoggedIn) {
    const authButtons = document.getElementById('authButtons');
    const userInfo = document.getElementById('userInfo');
    const navUsername = document.getElementById('navUsername');

    if (isLoggedIn && username) {
        authButtons?.classList.add('d-none');
        userInfo?.classList.remove('d-none');
        userInfo?.classList.add('d-flex');

        if (navUsername) navUsername.textContent = username;
    } else {
        authButtons?.classList.remove('d-none');
        userInfo?.classList.add('d-none');
    }
}

async function logout() {
    await fetch('/api/auth/logout', { method: 'POST' });

    localStorage.removeItem('username');
    window.location.href = '/';
}

async function checkAuthRedirect(redirectTo) {
    const res = await fetch('/api/auth/me');

    if (res.ok) {
        window.location.href = redirectTo;
    }
}

async function requireAuth() {
    const res = await fetch('/api/auth/me');

    if (!res.ok) {
        window.location.href = '/login.html';
        return null;
    }

    return await res.json();
}

function togglePass(inputId) {
    const el = document.getElementById(inputId);
    el.type = el.type === 'password' ? 'text' : 'password';
}

function formatUSD(n) {
    if (n === null || n === undefined) {
        return '—';
    }

    return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD',
        minimumFractionDigits: n < 1 ? 4 : 2, maximumFractionDigits: n < 1 ? 6 : 2 }).format(n);
}

function formatNumber(n, digits = 6) {
    return new Intl.NumberFormat('ru-RU', { maximumFractionDigits: digits }).format(n);
}

// Initialization when loading page
document.addEventListener('DOMContentLoaded', () => {
    // Refreshing username from localStorage without waiting for fetch
    const cached = localStorage.getItem('username');

    if (cached) {
        const navUsername = document.getElementById('navUsername');
        if (navUsername) navUsername.textContent = cached;
    }

    initAuth();
});
