using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Gestion_SalleClasseEDT.Models;

namespace Gestion_SalleClasseEDT.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
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
        return View();
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

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
