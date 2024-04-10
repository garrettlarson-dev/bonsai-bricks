using Identity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Controllers
{
    public class HomeController : Controller
    {
        private UserManager<AppUser> userManager;
        public HomeController(UserManager<AppUser> userMgr)
        {
            userManager = userMgr;
        }

        //[Authorize(Roles = "Manager")]
        public async Task<IActionResult> Index()
        {
            // AppUser user = await userManager.GetUserAsync(HttpContext.User);
            // string message = "Hello " + user.UserName;
            // return View((object)message);
            return View();
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