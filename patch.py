import re

with open('Views/Home/Cours.cshtml', 'r', encoding='utf-8') as f:
    text = f.read()

# On supprime le bloc de soumission existant
text = re.sub(r'// --- Soumission du formulaire de cr.*?(?=</script>)', '', text, flags=re.DOTALL)

js_to_add = """
        // --- Soumission du formulaire de création ---
        document.getElementById('createCourseForm').addEventListener('submit', async function(e) {
            e.preventDefault();
            
            const matSelect = document.getElementById('courseMatiereId');
            const affId = parseInt(matSelect.value);
            if (!affId) return;

            const typeSelect = document.getElementById('courseType');
            const type = typeSelect.value || 'CM';

            const payload = {
                IdAffectation: affId,
                TypeCours: type,
                Mode: 'Presentiel',
                Statut: 'Cree'
            };

            const btn = this.querySelector('button[type="submit"]');
            btn.disabled = true;
            btn.innerHTML = '<i data-lucide="loader-2" class="w-4 h-4 animate-spin inline mr-1"></i> Création...';

            try {
                const response = await fetch('/api/Cours', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
                    },
                    body: JSON.stringify(payload)
                });

                if (response.ok) {
                    const createdCourse = await response.json();
                    
                    Swal.fire({
                        icon: 'success',
                        title: 'Cours créé avec succès !',
                        text: 'Passage à l\\'étape de planification...',
                        timer: 1500,
                        showConfirmButton: false
                    });

                    closeCreateCourseModal();

                    setTimeout(() => {
                        const courseId = createdCourse.idCours || createdCourse.IdCours || createdCourse.id;
                        if (typeof openNewPlanificationModal === 'function') {
                            openNewPlanificationModal(courseId);
                        } else {
                            window.location.href = '/Planification?courseId=' + courseId;
                        }
                    }, 1500);
                } else {
                    const error = await response.json();
                    throw new Error(error.Message || 'Erreur lors de la création');
                }
            } catch (err) {
                Swal.fire('Erreur', err.message, 'error');
            } finally {
                btn.disabled = false;
                btn.innerHTML = 'Créer et Planifier';
            }
        });

        // --- Fonctions AJAX pour les filtres ---
        async function loadNiveaux() {
            try {
                const res = await fetch('/api/Matiere/Niveaux');
                const niveaux = await res.json();
                const sel = document.getElementById('courseNiveauId');
                sel.innerHTML = '<option value="">Sélectionnez un niveau</option>';
                niveaux.forEach(n => {
                    sel.innerHTML += `<option value="${n.idNiveau || n.IdNiveau}">${n.codeNiveau || n.CodeNiveau}</option>`;
                });
            } catch (e) {
                console.error(e);
            }
        }

        async function loadFilieres() {
            const niveauId = document.getElementById('courseNiveauId').value;
            const sel = document.getElementById('courseFiliereId');
            sel.innerHTML = '<option value="">Sélectionnez une filière</option>';
            sel.disabled = true;
            document.getElementById('courseMatiereId').innerHTML = '<option value="">Sélectionnez d\\'abord niveau et filière</option>';
            document.getElementById('courseMatiereId').disabled = true;

            if (!niveauId) return;

            try {
                const res = await fetch(`/api/Matiere/Filieres?niveauId=${niveauId}`);
                const filieres = await res.json();
                filieres.forEach(f => {
                    sel.innerHTML += `<option value="${f.idFiliere || f.IdFiliere}">${f.nomFiliere || f.NomFiliere}</option>`;
                });
                sel.disabled = false;
            } catch(e) {}
        }

        let currentMatieresOptions = [];

        async function loadMatieresNonPlanifiees() {
            const niveauId = document.getElementById('courseNiveauId').value;
            const filiereId = document.getElementById('courseFiliereId').value;
            const sel = document.getElementById('courseMatiereId');
            
            sel.innerHTML = '<option value="">Recherche des matières...</option>';
            sel.disabled = true;

            if (!niveauId || !filiereId) return;

            try {
                const res = await fetch(`/api/Matiere/NonPlanifiees?filiereId=${filiereId}&niveauId=${niveauId}`);
                const data = await res.json();
                currentMatieresOptions = data;

                sel.innerHTML = '<option value="">Sélectionnez une matière à créer en cours</option>';
                data.forEach(m => {
                    sel.innerHTML += `<option value="${m.idAffectation || m.IdAffectation}">${m.nomMatiere || m.NomMatiere} - Prof: ${m.professeurNom || m.ProfesseurNom}</option>`;
                });
                sel.disabled = false;
            } catch(e) {
                sel.innerHTML = '<option value="">Erreur de chargement</option>';
            }
        }

        function updateCreationSummary() {
            const matSelect = document.getElementById('courseMatiereId');
            const typeSelect = document.getElementById('courseType');
            
            if (!matSelect.value) {
                document.getElementById('creationSummary').innerHTML = 'Veuillez sélectionner une matière.';
                return;
            }

            const affId = parseInt(matSelect.value);
            const m = currentMatieresOptions.find(x => (x.idAffectation || x.IdAffectation) === affId);
            const type = typeSelect.value || 'CM';
            
            if (m) {
                let vol = 0;
                if (type === 'CM') vol = m.heuresCm || m.HeuresCm;
                else if (type === 'TD') vol = m.heuresTd || m.HeuresTd;
                else if (type === 'TP') vol = m.heuresTp || m.HeuresTp;
                
                document.getElementById('creationSummary').innerHTML = `
                    Matière : <strong class="text-indigo-700">${m.nomMatiere || m.NomMatiere} (${type})</strong> <br/>
                    Volume récupéré : <strong class="text-indigo-700">${vol}h</strong> <br/>
                    Enseignant assigné par admin : <strong class="text-indigo-700">${m.professeurNom || m.ProfesseurNom}</strong>
                `;
            }
        }

        document.addEventListener('DOMContentLoaded', () => {
            loadNiveaux();
        });
"""

text = text.replace('</script>', js_to_add + '\n    </script>')

with open('Views/Home/Cours.cshtml', 'w', encoding='utf-8') as f:
    f.write(text)

print("Patch applied successfully.")
