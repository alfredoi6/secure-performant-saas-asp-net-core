using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Web.Factories;
using Web.Models;

namespace Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
	        var tenantId = User.FindFirst(TenantClaimTypes.TenantId)?.Value;
	        var tenantName = User.FindFirst(TenantClaimTypes.TenantName)?.Value;
	        var userId = User.FindFirst(TenantClaimTypes.UserId)?.Value;

	        var info = new TenantInfo
	        {
		        TenantId = tenantId,
		        TenantName = tenantName,
		        UserId = userId
	        };

            return View(info);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
