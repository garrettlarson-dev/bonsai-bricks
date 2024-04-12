using Identity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppIdentityDbContext _context;
        private UserManager<AppUser> userManager;
        public HomeController(AppIdentityDbContext context, UserManager<AppUser> userMgr)
        {
            _context = context;
            userManager = userMgr;
        }

        //[Authorize(Roles = "Manager")]
        public IActionResult Index()
        {
            var product = _context.Products;
            return View(product);
        }

        public async Task<IActionResult> Privacy()
        {
            return View(Privacy);
        }
        
        public async Task<IActionResult> AboutMe()
        {
            return View(AboutMe);
        }
    }
}