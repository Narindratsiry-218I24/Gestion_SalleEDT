using System;
using System.IO;
using System.Threading.Tasks;
using Gestion_SalleClasseEDT.Controllers;
using Gestion_SalleClasseEDT.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Gestion_SalleClasseEDT.Tests
{
    class TestWebHostEnv : IWebHostEnvironment
    {
        public string WebRootPath { get; set; }
        public string EnvironmentName { get; set; }
        public string ApplicationName { get; set; }
        public IFileProvider ContentRootFileProvider { get; set; }
        public IFileProvider WebRootFileProvider { get; set; }
        public string ContentRootPath { get; set; }
    }

    public class ProfesseurUploadTests
    {
        private EMITDbContext GetContext()
        {
            var options = new DbContextOptionsBuilder<EMITDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new EMITDbContext(options);
        }

        [Fact]
        public async Task CreateProfesseur_WithValidFiles_SavesUrls()
        {
            var ctx = GetContext();
            var env = new TestWebHostEnv { WebRootPath = Path.Combine(Path.GetTempPath(), "webroot_test") };
            Directory.CreateDirectory(env.WebRootPath);

            var controller = new ProfesseursController(ctx, env);

            var model = new Professeur { Nom = "Dupont", Prenom = "Jean", Email = "jean.dupont@test.local" };

            // Create fake photo
            var photoBytes = new byte[100];
            new Random().NextBytes(photoBytes);
            var photoStream = new MemoryStream(photoBytes);
            IFormFile photo = new FormFile(photoStream, 0, photoBytes.Length, "photoFile", "photo.jpg") { Headers = new HeaderDictionary(), ContentType = "image/jpeg" };

            // Create fake cv
            var cvBytes = new byte[100];
            new Random().NextBytes(cvBytes);
            var cvStream = new MemoryStream(cvBytes);
            IFormFile cv = new FormFile(cvStream, 0, cvBytes.Length, "cvFile", "cv.pdf") { Headers = new HeaderDictionary(), ContentType = "application/pdf" };

            var result = await controller.Create(model, photo, cv) as IActionResult;

            // Verify saved
            var saved = await ctx.Professeurs.FirstOrDefaultAsync(p => p.Email == "jean.dupont@test.local");
            Assert.NotNull(saved);
            Assert.False(string.IsNullOrEmpty(saved.PhotoUrl));
            Assert.False(string.IsNullOrEmpty(saved.CvUrl));

            // Clean
            Directory.Delete(env.WebRootPath, true);
        }

        [Fact]
        public async Task CreateProfesseur_InvalidPhoto_Rejected()
        {
            var ctx = GetContext();
            var env = new TestWebHostEnv { WebRootPath = Path.Combine(Path.GetTempPath(), "webroot_test2") };
            Directory.CreateDirectory(env.WebRootPath);

            var controller = new ProfesseursController(ctx, env);
            var model = new Professeur { Nom = "Test", Prenom = "T", Email = "test@local" };

            // invalid photo extension
            var bytes = new byte[100];
            new Random().NextBytes(bytes);
            var stream = new MemoryStream(bytes);
            IFormFile badPhoto = new FormFile(stream, 0, bytes.Length, "photoFile", "photo.bmp") { Headers = new HeaderDictionary(), ContentType = "image/bmp" };

            var view = await controller.Create(model, badPhoto, null) as ViewResult;
            Assert.NotNull(view);
            Assert.False(controller.ModelState.IsValid);

            Directory.Delete(env.WebRootPath, true);
        }
    }
}
