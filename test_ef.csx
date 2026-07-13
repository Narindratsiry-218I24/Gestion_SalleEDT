using System;
using Microsoft.EntityFrameworkCore;
using Gestion_SalleClasseEDT.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var services = new ServiceCollection();
services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));
services.AddDbContext<EMITDbContext>(options => {
    options.UseNpgsql("Server=localhost;Port=5432;Database=EMIT_EDT_DB;User Id=postgres;Password=kifeko;");
    options.EnableSensitiveDataLogging();
});
var provider = services.BuildServiceProvider();
var db = provider.GetRequiredService<EMITDbContext>();

var professeur = new Professeur {
    Nom = "Test",
    Prenom = "Test",
    Email = "test@emit.mg",
    Telephone = "0340000000",
    Grade = "Assistant",
    Matricule = "PROF-9999",
    Departement = "INFO",
    Statut = "Actif",
    IdUtilisateur = 1,
    SpecialitesSecondaires = "[]"
};
db.Professeurs.Add(professeur);
Console.WriteLine("State before save: " + db.Entry(professeur).State);

try {
    db.SaveChanges();
    Console.WriteLine("Saved successfully. ID = " + professeur.IdProfesseur);
} catch (Exception ex) {
    Console.WriteLine("Error saving: " + ex.Message);
    if (ex.InnerException != null) Console.WriteLine("Inner: " + ex.InnerException.Message);
}
