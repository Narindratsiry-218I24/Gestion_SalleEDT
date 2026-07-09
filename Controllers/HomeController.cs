using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Gestion_SalleClasseEDT.Models;
using Microsoft.EntityFrameworkCore;

namespace Gestion_SalleClasseEDT.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly EMITDbContext _context;

    public HomeController(ILogger<HomeController> logger, EMITDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult Dashboard()
    {
        return View();
    }

    public IActionResult Connexion()
    {
        return View();
    }

    public IActionResult Inscription()
    {
        return View();
    }

    public IActionResult Parametres()
    {
        return View();
    }

    public IActionResult Cours()
    {
        var cours = _context.Cours
            .Include(c => c.Matiere)
            .Include(c => c.Professeur)
            .Include(c => c.Classe).ThenInclude(cl => cl.Filiere)
            .ToList();
        return View(cours);
    }

    public IActionResult EDT()
    {
        return View();
    }

    public IActionResult Professeurs()
    {
        return View();
    }

    public IActionResult Requetes()
    {
        return View();
    }

    public IActionResult Salles()
    {
        return View();
    }

    public IActionResult Structures()
    {
        return View();
    }

    public IActionResult Validation()
    {
        return View();
    }

    public IActionResult CourseDetails(int id)
    {
        var course = _context.Cours
            .Include(c => c.Matiere)
            .Include(c => c.Professeur)
            .Include(c => c.Classe)
            .Include(c => c.Seances).ThenInclude(s => s.Salle)
            .FirstOrDefault(c => c.IdCours == id);
        
        if (course == null) return NotFound();
        return View(course);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
