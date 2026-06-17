using System;
using System.Threading.Tasks;
using Gestion_SalleClasseEDT.Models;
using Gestion_SalleClasseEDT.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Gestion_SalleClasseEDT.Tests
{
    public class SchedulingServiceTests
    {
        private EMITDbContext GetContext()
        {
            var options = new DbContextOptionsBuilder<EMITDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new EMITDbContext(options);
        }

        [Fact]
        public async Task CreateSchedule_BlocksRoomConflict()
        {
            var ctx = GetContext();
            // Seed a salle and a cours + creneau to conflict
            var salle = new Salle { IdSalle = 100, NomSalle = "A101", Capacite = 30, CodeBatiment = "A", NumeroPorte = "101", TypeSalle = "Amphi" };
            ctx.Salles.Add(salle);
            await ctx.SaveChangesAsync();

            var svc = new SchedulingService(ctx);

            var s1 = new Schedule { Id = 1, SalleId = 100, Date = DateTime.Today, HeureDebut = new TimeSpan(9,0,0), HeureFin = new TimeSpan(10,0,0), SessionType = "CM" };
            await svc.CreateScheduleAsync(s1);

            var s2 = new Schedule { Id = 2, SalleId = 100, Date = DateTime.Today, HeureDebut = new TimeSpan(9,30,0), HeureFin = new TimeSpan(10,30,0), SessionType = "CM" };

            await Assert.ThrowsAsync<Exception>(async () => await svc.CreateScheduleAsync(s2));
        }
    }
}
