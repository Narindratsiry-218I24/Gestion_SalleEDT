// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

window.app = window.app || {};

window.app.readProp = function (item, pascalName, camelName) {
    if (!item) return undefined;
    const fallback = camelName || pascalName.charAt(0).toLowerCase() + pascalName.slice(1);
    return item[pascalName] ?? item[fallback];
};

window.app.escapeHtml = function (value) {
    return String(value ?? "")
        .replaceAll("&", "&amp;")
        .replaceAll("<", "&lt;")
        .replaceAll(">", "&gt;")
        .replaceAll('"', "&quot;")
        .replaceAll("'", "&#039;");
};

async function appRequest(url, options) {
    const response = await fetch(url, options);
    if (!response.ok) {
        const message = await response.text();
        throw new Error(message || `Erreur HTTP ${response.status}`);
    }

    if (response.status === 204) return null;
    return response.json();
}

window.app.apiGet = function (url) {
    return appRequest(url, { headers: { "Accept": "application/json" } });
};

window.app.apiPost = function (url, data) {
    return appRequest(url, {
        method: "POST",
        headers: {
            "Accept": "application/json",
            "Content-Type": "application/json"
        },
        body: JSON.stringify(data)
    });
};

window.app.apiPut = function (url, data) {
    return appRequest(url, {
        method: "PUT",
        headers: {
            "Accept": "application/json",
            "Content-Type": "application/json"
        },
        body: JSON.stringify(data)
    });
};

window.app.apiDelete = function (url) {
    return appRequest(url, { method: "DELETE", headers: { "Accept": "application/json" } });
};

document.addEventListener("DOMContentLoaded", function () {
    if (typeof lucide !== "undefined") {
        lucide.createIcons();
    }
});

