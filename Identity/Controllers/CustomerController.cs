using Identity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Identity.Models.ViewModels;

namespace Identity.Controllers
{
    public class CustomerController : Controller
    {
        private readonly AppIdentityDbContext _context;

        public CustomerController(AppIdentityDbContext context)
        {
            _context = context;
        }
        
        public async Task<IActionResult> Index(int pageNum, int pageSize=6)
        {
            
            var products = new ProductViewModel
            {
                Products = _context.Products
                    .Skip(pageSize * (Math.Max(1, pageNum - 1))) // Skip for pagination based on selectedPageSize
                    .Take(pageSize), // Take for pagination based on selectedPageSize
                PaginationInfo = new PaginationInfo
                {
                    CurrentPage = pageNum,
                    ItemsPerPage = pageSize,
                    TotalItems = _context.Products.Count()
                }
                
                // CurrentLegoType = legoType,
                // CurrentPageSize = pageSize
            };
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

