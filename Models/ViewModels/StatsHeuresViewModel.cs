using System.Collections.Generic;

namespace Gestion_SalleClasseEDT.Models.ViewModels
{
    /// <summary>
    /// Vue-modèle pour les statistiques de volume horaire par année académique.
    /// (Phase 1.6)
    /// </summary>
    public class StatsHeuresViewModel
    {
        public int IdAnnee { get; set; }
        public string AnneeLibelle { get; set; } = string.Empty;
        public List<ProfesseurStats> Professeurs { get; set; } = new();
    }

    public class ProfesseurStats
    {
        public int IdProfesseur { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Prenom { get; set; } = string.Empty;
        
        // Volumes Horaires assignés (AffectationMatiere)
        public int VolumeAssigneCm { get; set; }
        public int VolumeAssigneTd { get; set; }
        public int VolumeAssigneTp { get; set; }
        public int VolumeAssigneTotal => VolumeAssigneCm + VolumeAssigneTd + VolumeAssigneTp;

        // Volumes Horaires planifiés (Seances de statut différent de Annulee)
        public double VolumePlanifieCm { get; set; }
        public double VolumePlanifieTd { get; set; }
        public double VolumePlanifieTp { get; set; }
        public double VolumePlanifieTotal => VolumePlanifieCm + VolumePlanifieTd + VolumePlanifieTp;

        // Volumes Horaires réalisés (Seances de statut Realisee)
        public double VolumeRealiseCm { get; set; }
        public double VolumeRealiseTd { get; set; }
        public double VolumeRealiseTp { get; set; }
        public double VolumeRealiseTotal => VolumeRealiseCm + VolumeRealiseTd + VolumeRealiseTp;
    }
}
