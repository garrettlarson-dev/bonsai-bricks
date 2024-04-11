using Identity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Identity.Controllers
{
    public class CustomerController : Controller
    {
        private readonly AppIdentityDbContext _context;

        public CustomerController(AppIdentityDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var products = _context.Products.ToList();
            return View(products);
        }
        
        public IActionResult ProductDescription(int id)
        {
            var product = _context.Products.AsEnumerable().FirstOrDefault(p => p.ProductId == id);
            if (product == null)
            {
                return NotFound(); // or handle the case where the product is not found
            }
            return View(product);
        }
        
        public IActionResult Cart()
        {
            return View();
        }

    }

}

