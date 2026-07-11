const fs = require('fs');
let c1 = fs.readFileSync('Views/Home/Matieres.cshtml', 'utf8');
let c2 = fs.readFileSync('Views/Shared/_MatiereForm.cshtml', 'utf8');

c1 = c1.replace(/else if\(nomMention\.toLowerCase\(\)\.includes\("multim"\).*?mentionStatic\s*=\s*".*?";/, 'else if(nomMention.toLowerCase().includes("multim")) mentionStatic = "Multimédia";');
c2 = c2.replace(/<option value="Multim.*?">Multim.*?<\/option>/g, '<option value="Multimédia">Multimédia</option>');
c2 = c2.replace(/"Multim.*?":/g, '"Multimédia":');

fs.writeFileSync('Views/Home/Matieres.cshtml', c1);
fs.writeFileSync('Views/Shared/_MatiereForm.cshtml', c2);
