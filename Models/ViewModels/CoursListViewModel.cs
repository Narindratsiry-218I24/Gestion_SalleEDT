using System.Collections.Generic;
using Gestion_SalleClasseEDT.Models;

namespace Gestion_SalleClasseEDT.Models.ViewModels
{
    public class CoursListViewModel
    {
        public List<Cours> Cours { get; set; } = new List<Cours>();
        public int TotalItems { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        
        public string? FilterMention { get; set; }
        public string? FilterNiveau { get; set; }
        public string? FilterParcours { get; set; }
        public string? SearchTerm { get; set; }
        public string? FilterStatut { get; set; }
        
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
    }

    public class FilterOptionsViewModel
    {
        public List<Mention> Mentions { get; set; } = new List<Mention>();
        public List<Niveau> Niveaux { get; set; } = new List<Niveau>();
        public List<Filiere> Filieres { get; set; } = new List<Filiere>();
        public List<string> Statuts { get; set; } = new List<string>
        {
            "Cree", "EnAttente", "Planifie", "EnCours", "Suspendu", "Termine", "Archive"
        };
    }
}