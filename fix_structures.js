const fs = require('fs');
let content = fs.readFileSync('Views/Home/Structures.cshtml', 'utf8');

content = content.replace('async function openAddFiliereModal() {', 'async function openAddFiliereModal() {\n            const mentions = await app.apiGet("/api/Mention");\n            const select = document.getElementById("IdMention");\n            select.innerHTML = \'\';\n            mentions.forEach(m => {\n                select.innerHTML += `<option value="${m.IdMention || m.idMention}">${m.NomMention || m.nomMention}</option>`;\n            });');

let oldFiliereMap = 'const filieresHtml = mentionFilieres.length > 0\r\n                    ? mentionFilieres.map(f => `<span class="inline-block bg-purple-50 text-purple-700 px-2 py-0.5 rounded text-xs font-bold mr-1 mb-1">${app.escapeHtml(app.readProp(f, "CodeFiliere") || app.readProp(f, "NomFiliere") || "")}</span>`).join("")\r\n                    : \'<span class="text-gray-400 text-xs italic">Aucune</span>\';';

// If CRLF wasn't matched, try LF
if (content.indexOf(oldFiliereMap) === -1) {
    oldFiliereMap = 'const filieresHtml = mentionFilieres.length > 0\n                    ? mentionFilieres.map(f => `<span class="inline-block bg-purple-50 text-purple-700 px-2 py-0.5 rounded text-xs font-bold mr-1 mb-1">${app.escapeHtml(app.readProp(f, "CodeFiliere") || app.readProp(f, "NomFiliere") || "")}</span>`).join("")\n                    : \'<span class="text-gray-400 text-xs italic">Aucune</span>\';';
}

let newFiliereMap = `const filieresHtml = mentionFilieres.length > 0
                    ? mentionFilieres.map(f => {
                        const fid = app.readProp(f, "IdFiliere") || app.readProp(f, "idFiliere");
                        const fname = app.escapeHtml(app.readProp(f, "CodeFiliere") || app.readProp(f, "NomFiliere") || "");
                        return \`<div class="inline-flex items-center bg-purple-50 text-purple-700 px-2 py-0.5 rounded text-xs font-bold mr-1 mb-1">
                            <span>\${fname}</span>
                            <button onclick="editFiliere(\${fid})" class="ml-2 text-blue-500 hover:text-blue-700"><i data-lucide="edit-2" class="w-3 h-3"></i></button>
                            <button onclick="deleteFiliere(\${fid})" class="ml-1 text-red-500 hover:text-red-700"><i data-lucide="trash-2" class="w-3 h-3"></i></button>
                        </div>\`;
                    }).join("")
                    : '<span class="text-gray-400 text-xs italic">Aucune</span>';`;

content = content.replace(oldFiliereMap, newFiliereMap);

let newMethods = `
        window.editFiliere = function(id) {
            alert("Modification de la filière " + id + " (À implémenter dans le backend)");
        };
        window.deleteFiliere = async function(id) {
            if(confirm("Voulez-vous supprimer cette filière ?")) {
                try {
                    const res = await fetch("/api/Filiere/" + id, { method: "DELETE" });
                    if(res.ok) { alert("Supprimée"); loadStructures(); }
                    else alert("Erreur lors de la suppression");
                } catch(e) { alert(e.message); }
            }
        };
`;
content = content.replace('document.addEventListener("DOMContentLoaded", function() {', newMethods + '\n        document.addEventListener("DOMContentLoaded", function() {');

fs.writeFileSync('Views/Home/Structures.cshtml', content);
console.log("Modifications effectuees.");