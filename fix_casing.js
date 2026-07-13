const fs = require('fs');

const path = 'Views/Home/Cours.cshtml';
let data = fs.readFileSync(path, 'utf8');

// replace f.idFiliere || f.IdFiliere
data = data.replace(/f\.idFiliere \|\| f\.IdFiliere/g, "window.app.readProp(f, 'IdFiliere')");
data = data.replace(/f\.nomFiliere \|\| f\.NomFiliere/g, "window.app.readProp(f, 'NomFiliere')");

// n.idNiveau || n.IdNiveau
data = data.replace(/n\.idNiveau \|\| n\.IdNiveau/g, "window.app.readProp(n, 'IdNiveau')");

// m.idAffectation || m.IdAffectation
data = data.replace(/m\.idAffectation \|\| m\.IdAffectation/g, "window.app.readProp(m, 'IdAffectation')");
data = data.replace(/m\.nomMatiere \|\| m\.NomMatiere/g, "window.app.readProp(m, 'NomMatiere')");
data = data.replace(/m\.professeurNom \|\| m\.ProfesseurNom/g, "window.app.readProp(m, 'ProfesseurNom')");

fs.writeFileSync(path, data, 'utf8');

const path2 = 'Views/Planification/Index.cshtml';
if (fs.existsSync(path2)) {
    let data2 = fs.readFileSync(path2, 'utf8');
    data2 = data2.replace(/s\.idSalle \|\| s\.IdSalle/g, "window.app.readProp(s, 'IdSalle')");
    data2 = data2.replace(/s\.nomSalle \|\| s\.NomSalle/g, "window.app.readProp(s, 'NomSalle')");
    data2 = data2.replace(/s\.capacite \|\| s\.Capacite/g, "window.app.readProp(s, 'Capacite')");
    data2 = data2.replace(/s\.codeBatiment \|\| s\.CodeBatiment/g, "window.app.readProp(s, 'CodeBatiment')");
    data2 = data2.replace(/s\.etage \|\| s\.Etage/g, "window.app.readProp(s, 'Etage')");
    fs.writeFileSync(path2, data2, 'utf8');
}

console.log('done');
