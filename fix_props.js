const fs = require('fs');

const path = 'Views/Professeurs/Index.cshtml';
let data = fs.readFileSync(path, 'utf8');

// Replace the first foreach
data = data.replace(
    /@foreach \(var p in Model\)\s*\{\s*var profData = JsonSerializer\.Serialize\(new \{[\s\S]*?IdUtilisateur = p\.IdUtilisateur\s*\}\);\s*var statusClass[\s\S]*?var statusIcon =[\s\S]*?"â Œ";/g,
    `@foreach (var item in profsWithJson)
                {
                    var p = item.Prof;
                    var profData = item.JsonData;

                    var statusClass = p.Statut == "Actif" ? "bg-green-100 text-green-800" :
                                      p.Statut == "En congé" ? "bg-yellow-100 text-yellow-800" :
                                      "bg-red-100 text-red-800";
                    var statusIcon = p.Statut == "Actif" ? "✅" :
                                     p.Statut == "En congé" ? "⚠️" :
                                     "❌";`
);

// Replace the second foreach
data = data.replace(
    /@foreach \(var p in Model\)\s*\{\s*var profData = JsonSerializer\.Serialize\(new \{[\s\S]*?CapaciteHoraireMax = p\.CapaciteHoraireMax, HeuresEffectuees = p\.HeuresEffectuees\s*\}\);\s*var statusIcon = p\.Statut == "Actif" \? "âœ…" : p\.Statut == "En congÃ©" \? "âš ï¸ " : "â Œ";/g,
    `@foreach (var item in profsWithJson)
        {
            var p = item.Prof;
            var profData = item.JsonData;

            var statusIcon = p.Statut == "Actif" ? "✅" : p.Statut == "En congé" ? "⚠️" : "❌";`
);

fs.writeFileSync(path, data, 'utf8');
console.log('done');
