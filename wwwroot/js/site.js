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

window.app.exportToPdf = function (title, headers, rows, filename) {
    // Ensure jsPDF is loaded
    const { jsPDF } = window.jspdf;
    const doc = new jsPDF('p', 'pt');
    // Add logo
    const img = new Image();
    img.src = '/images/emit-logo.png';
    img.onload = function () {
        const ratio = img.width / img.height;
        const w = 80; // width in pt
        const h = w / ratio;
        doc.addImage(img, 'PNG', (doc.internal.pageSize.getWidth() - w) / 2, 20, w, h);
        doc.setFontSize(18);
        doc.text(title, doc.internal.pageSize.getWidth() / 2, 80, { align: 'center' });
        // AutoTable
        doc.autoTable({
            startY: 100,
            head: [headers],
            body: rows,
            theme: 'grid',
            headStyles: { fillColor: [31, 73, 125], textColor: 255 },
            alternateRowStyles: { fillColor: [240, 240, 240] },
            margin: { left: 40, right: 40 },
            didDrawPage: function (data) {
                // Footer page number
                const pageCount = doc.internal.getNumberOfPages();
                doc.setFontSize(10);
                doc.text(`Page ${data.pageNumber} of ${pageCount}`, doc.internal.pageSize.getWidth() - 40, doc.internal.pageSize.getHeight() - 20, { align: 'right' });
            }
        });
        doc.save(filename || (title.replace(/\s+/g, '_') + '.pdf'));
    };
};

window.app.exportToExcel = function (title, headers, rows, filename) {
    const ws = XLSX.utils.aoa_to_sheet([headers, ...rows]);
    const wb = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(wb, ws, title);
    const wbout = XLSX.write(wb, { bookType: 'xlsx', type: 'array' });
    const blob = new Blob([wbout], { type: "application/octet-stream" });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = filename || (title.replace(/\s+/g, '_') + '.xlsx');
    a.click();
    setTimeout(() => URL.revokeObjectURL(url), 100);
};

// Export functions for Cours view
function exportCoursPdf() {
    const title = "Export Cours";
    const headers = ["Matière","Enseignant","Classe / Parcours","Progression","Statut","Planification"];
    const rows = [];
    const table = document.querySelector('table');
    if (!table) return;
    const tbody = table.querySelector('tbody');
    tbody.querySelectorAll('tr').forEach(tr => {
        const cells = tr.querySelectorAll('td');
        if (cells.length === 0) return;
        const row = [];
        for (let i = 0; i < cells.length - 1; i++) {
            row.push(cells[i].innerText.trim());
        }
        rows.push(row);
    });
    window.app.exportToPdf(title, headers, rows);
}

function exportCoursExcel() {
    const title = "Export Cours";
    const headers = ["Matière","Enseignant","Classe / Parcours","Progression","Statut","Planification"];
    const rows = [];
    const table = document.querySelector('table');
    if (!table) return;
    const tbody = table.querySelector('tbody');
    tbody.querySelectorAll('tr').forEach(tr => {
        const cells = tr.querySelectorAll('td');
        if (cells.length === 0) return;
        const row = [];
        for (let i = 0; i < cells.length - 1; i++) {
            row.push(cells[i].innerText.trim());
        }
        rows.push(row);
    });
    window.app.exportToExcel(title, headers, rows);
}
// Export functions for Salles view
function exportSallesPdf() {
    const title = "Export Salles";
    const headers = ["Nom Salle","Bâtiment / Étage","Porte","Type","Capacité"];
    const rows = [];
    const table = document.querySelector('table');
    if (!table) return;
    const tbody = table.querySelector('tbody');
    tbody.querySelectorAll('tr').forEach(tr => {
        const cells = tr.querySelectorAll('td');
        if (cells.length === 0) return;
        const row = [];
        // exclude last cell (actions)
        for (let i = 0; i < cells.length - 1; i++) {
            row.push(cells[i].innerText.trim());
        }
        rows.push(row);
    });
    const timestamp = new Date().toISOString().replace(/[:.]/g, '-');
    window.app.exportToPdf(title, headers, rows, `${title}_${timestamp}.pdf`);
}

function exportSallesExcel() {
    const title = "Export Salles";
    const headers = ["Nom Salle","Bâtiment / Étage","Porte","Type","Capacité"];
    const rows = [];
    const table = document.querySelector('table');
    if (!table) return;
    const tbody = table.querySelector('tbody');
    tbody.querySelectorAll('tr').forEach(tr => {
        const cells = tr.querySelectorAll('td');
        if (cells.length === 0) return;
        const row = [];
        for (let i = 0; i < cells.length - 1; i++) {
            row.push(cells[i].innerText.trim());
        }
        rows.push(row);
    });
    const timestamp = new Date().toISOString().replace(/[:.]/g, '-');
    window.app.exportToExcel(title, headers, rows, `${title}_${timestamp}.xlsx`);
}
window.app.exportTablePdf = function (customTitle) {
    const title = customTitle || "Export";
    const table = document.querySelector('table');
    if (!table) return;
    const headers = [];
    const ths = table.querySelectorAll('thead th');
    ths.forEach((th, i) => {
        // Exclude last header which may be actions
        if (i < ths.length - 1) headers.push(th.innerText.trim());
    });
    const rows = [];
    const tbody = table.querySelector('tbody');
    tbody.querySelectorAll('tr').forEach(tr => {
        const cells = tr.querySelectorAll('td');
        if (cells.length === 0) return;
        const row = [];
        for (let i = 0; i < cells.length - 1; i++) {
            row.push(cells[i].innerText.trim());
        }
        rows.push(row);
    });
    const timestamp = new Date().toISOString().replace(/[:.]/g, '-');
    window.app.exportToPdf(title, headers, rows, `${title}_${timestamp}.pdf`);
};

window.app.exportTableExcel = function (customTitle) {
    const title = customTitle || "Export";
    const table = document.querySelector('table');
    if (!table) return;
    const headers = [];
    const ths = table.querySelectorAll('thead th');
    ths.forEach((th, i) => {
        if (i < ths.length - 1) headers.push(th.innerText.trim());
    });
    const rows = [];
    const tbody = table.querySelector('tbody');
    tbody.querySelectorAll('tr').forEach(tr => {
        const cells = tr.querySelectorAll('td');
        if (cells.length === 0) return;
        const row = [];
        for (let i = 0; i < cells.length - 1; i++) {
            row.push(cells[i].innerText.trim());
        }
        rows.push(row);
    });
    const timestamp = new Date().toISOString().replace(/[:.]/g, '-');
    window.app.exportToExcel(title, headers, rows, `${title}_${timestamp}.xlsx`);
};
