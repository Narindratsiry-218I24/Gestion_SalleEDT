using System.Threading.Tasks;
using Gestion_SalleClasseEDT.Models;
using Gestion_SalleClasseEDT.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Gestion_SalleClasseEDT.Tests
{
    public class SubjectServiceTests
    {
        private EMITDbContext GetContext()
        {
            var options = new DbContextOptionsBuilder<EMITDbContext>()
                .UseInMemoryDatabase(databaseName: "test_subjects")
                .Options;
            return new EMITDbContext(options);
        }

        [Fact]
        public async Task CreateSubject_AddsSubject()
        {
            var ctx = GetContext();
            var svc = new SubjectService(ctx);
            var sub = new Subject { Code = "TST", Label = "Test Subject", Credits = 3, Type = "CM" };
            var created = await svc.CreateAsync(sub, "tester");
            Assert.NotNull(created);
            Assert.Equal("TST", created.Code);
        }
    }
}
