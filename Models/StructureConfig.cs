using System.Collections.Generic;

namespace Gestion_SalleClasseEDT.Models
{
    public class StructureConfig
    {
        public static readonly List<MentionOption> Mentions = new()
        {
            new MentionOption { Id = 1, Name = "Informatique", Code = "IN" },
            new MentionOption { Id = 2, Name = "Management", Code = "MG" },
            new MentionOption { Id = 3, Name = "Multimédia", Code = "MM" }
        };

        public static readonly List<NiveauOption> Niveaux = new()
        {
            new NiveauOption { Id = 1, Name = "Licence 1", Type = "Licence" },
            new NiveauOption { Id = 2, Name = "Licence 2", Type = "Licence" },
            new NiveauOption { Id = 3, Name = "Licence 3", Type = "Licence" },
            new NiveauOption { Id = 4, Name = "Master 1", Type = "Master" },
            new NiveauOption { Id = 5, Name = "Master 2", Type = "Master" }
        };

        public static readonly Dictionary<int, List<string>> ParcoursByMentionAndType = new()
        {
            // Informatique
            { 1, new List<string> { "DA2I", "M2I", "SDIA", "SIGD" } },
            // Management
            { 2, new List<string> { "AES", "AE", "MEAS" } },
            // Multimédia
            { 3, new List<string> { "ICM", "RPO", "RCPO" } }
        };
    }

    public class MentionOption
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
    }

    public class NiveauOption
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; } // Licence or Master
    }
}
