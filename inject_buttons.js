const fs = require('fs');
const path = require('path');

const files = [
    'Views/Home/Matieres.cshtml',
    'Views/Home/Cours.cshtml',
    'Views/Home/Salles.cshtml',
    'Views/Home/EDT.cshtml',
    'Views/Home/Requetes.cshtml',
    'Views/Home/Validation.cshtml'
];

const exportHtml = (title) => `
                <button type="button" onclick="app.exportTablePdf('${title}')" class="h-10 inline-flex items-center justify-center gap-2 rounded-md border border-gray-200 bg-white px-4 text-sm font-medium text-red-600 shadow-sm hover:bg-red-50" title="Exporter PDF">
                    <i data-lucide="file-text" class="h-4 w-4"></i> PDF
                </button>
                <button type="button" onclick="app.exportTableExcel('${title}')" class="h-10 inline-flex items-center justify-center gap-2 rounded-md border border-gray-200 bg-white px-4 text-sm font-medium text-green-600 shadow-sm hover:bg-green-50" title="Exporter Excel">
                    <i data-lucide="file-spreadsheet" class="h-4 w-4"></i> Excel
                </button>`;

files.forEach(file => {
    let content = fs.readFileSync(file, 'utf8');
    const titleMatch = content.match(/ViewBag\.Title\s*=\s*"([^"]+)"/);
    const title = titleMatch ? titleMatch[1] : 'Export';
    
    // Most views have a section with buttons like "Nouveau Cours" or similar. We will find a good place to insert.
    // Let's insert before the search input or after the page title.
    if (!content.includes('exportTablePdf')) {
        // Try to insert after the page title or next to existing buttons
        // Let's find `<div class="flex flex-col gap-3 sm:flex-row sm:items-center">` or similar
        const insertPoint = /<div class="[^"]*flex[^"]*">[^<]*<button/i;
        content = content.replace(/(<div class="[^"]*flex[^"]*gap[^"]*">[^<]*<button)/i, `<div class="flex flex-col gap-3 sm:flex-row sm:items-center">${exportHtml(title)}` + "$1");
        // if the regex didn't match, we can just prepend it before `<div class="grid grid-cols-1`
        if (content === fs.readFileSync(file, 'utf8')) {
           content = content.replace(/(<div class="grid grid-cols-1)/, `<div class="flex gap-2 mb-4">${exportHtml(title)}</div>$1`);
        }
        fs.writeFileSync(file, content, 'utf8');
    }
});
console.log('done');
