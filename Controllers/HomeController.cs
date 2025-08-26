using Microsoft.AspNetCore.Mvc;


namespace PEA.Controllers;


public class HomeController : Controller
{
    public IActionResult Index() => View();
}