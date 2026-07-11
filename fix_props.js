const fs = require('fs');

let c = fs.readFileSync('Views/Shared/_MatiereForm.cshtml', 'utf8');

c = c.replace(/const filteredFilieres = filieresList\.filter\(f => \{[\s\S]*?\}\);/g, `const allowedParcoursLower = allowedParcours.map(p => p.toLowerCase());
        const filteredFilieres = filieresList.filter(f => {
            const code = (app.readProp(f, 'CodeFiliere') || f.CodeFiliere || "").trim().toLowerCase();
            return allowedParcoursLower.includes(code);
        });`);

c = c.replace(/filteredFilieres\.forEach\(item => \{[\s\S]*?\}\);/g, `filteredFilieres.forEach(item => {
            const opt = document.createElement('option');
            opt.value = app.readProp(item, 'IdFiliere') || item.IdFiliere;
            const codeFiliere = app.readProp(item, 'CodeFiliere') || item.CodeFiliere;
            const nomFiliere = app.readProp(item, 'NomFiliere') || item.NomFiliere;
            opt.textContent = \`\${codeFiliere} - \${nomFiliere}\`;
            selectParcours.appendChild(opt);
        });`);

c = c.replace(/const niveauObj = niveauxList\.find.*?;/g, `const niveauObj = niveauxList.find(n => (app.readProp(n, 'CodeNiveau') || n.CodeNiveau || "").trim().toUpperCase() === codeNiveauCherche);`);

fs.writeFileSync('Views/Shared/_MatiereForm.cshtml', c);

// And edit Matieres.cshtml
let c2 = fs.readFileSync('Views/Home/Matieres.cshtml', 'utf8');

c2 = c2.replace(/const filiereObj = filieresList\.find\(f => f\.IdFiliere === matiere\.IdFiliere\);/g, `const idF = app.readProp(matiere, 'IdFiliere') || matiere.IdFiliere;
            const filiereObj = filieresList.find(f => (app.readProp(f, 'IdFiliere') || f.IdFiliere) === idF);`);

c2 = c2.replace(/if\(filiereObj && filiereObj\.Mention\) {/g, `const mentionObj = app.readProp(filiereObj, 'Mention') || filiereObj.Mention;
            if(filiereObj && mentionObj) {`);

c2 = c2.replace(/const nomMention = filiereObj\.Mention\.NomMention;/g, `const nomMention = app.readProp(mentionObj, 'NomMention') || mentionObj.NomMention;`);

c2 = c2.replace(/const semestreObj = semestresList\.find\(s => s\.IdRefSemestre === matiere\.IdRefSemestre\);/g, `const idRS = app.readProp(matiere, 'IdRefSemestre') || matiere.IdRefSemestre;
            const semestreObj = semestresList.find(s => (app.readProp(s, 'IdRefSemestre') || s.IdRefSemestre) === idRS);`);

c2 = c2.replace(/if\(semestreObj && semestreObj\.Niveau\) {/g, `const nivObj = app.readProp(semestreObj, 'Niveau') || semestreObj.Niveau;
            if(semestreObj && nivObj) {`);

c2 = c2.replace(/const codeNiv = semestreObj\.Niveau\.CodeNiveau \|\| "";/g, `const codeNiv = (app.readProp(nivObj, 'CodeNiveau') || nivObj.CodeNiveau || "").trim().toUpperCase();`);

c2 = c2.replace(/document\.getElementById\('idMatiere'\)\.value = matiere\.IdMatiere;/g, `document.getElementById('idMatiere').value = app.readProp(matiere, 'IdMatiere') || matiere.IdMatiere;`);
c2 = c2.replace(/document\.getElementById\('codeMatiere'\)\.value = matiere\.CodeMatiere/g, `document.getElementById('codeMatiere').value = app.readProp(matiere, 'CodeMatiere') || matiere.CodeMatiere`);
c2 = c2.replace(/document\.getElementById\('nomMatiere'\)\.value = matiere\.NomMatiere/g, `document.getElementById('nomMatiere').value = app.readProp(matiere, 'NomMatiere') || matiere.NomMatiere`);
c2 = c2.replace(/document\.getElementById\('idFiliere'\)\.value = matiere\.IdFiliere/g, `document.getElementById('idFiliere').value = idF || matiere.IdFiliere`);
c2 = c2.replace(/document\.getElementById\('idRefSemestre'\)\.value = matiere\.IdRefSemestre/g, `document.getElementById('idRefSemestre').value = idRS || matiere.IdRefSemestre`);
c2 = c2.replace(/document\.getElementById\('credit'\)\.value = matiere\.Credit/g, `document.getElementById('credit').value = app.readProp(matiere, 'Credit') || matiere.Credit`);
c2 = c2.replace(/document\.getElementById\('volumeHoraire'\)\.value = matiere\.VolumeHoraire/g, `document.getElementById('volumeHoraire').value = app.readProp(matiere, 'VolumeHoraire') || matiere.VolumeHoraire`);
c2 = c2.replace(/document\.getElementById\('idProfesseurResponsable'\)\.value = matiere\.IdProfesseurResponsable/g, `document.getElementById('idProfesseurResponsable').value = app.readProp(matiere, 'IdProfesseurResponsable') || matiere.IdProfesseurResponsable`);

fs.writeFileSync('Views/Home/Matieres.cshtml', c2);

console.log("Done");
