using Identity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Identity.Models.ViewModelsTwo;

namespace Identity.Controllers
{
    public class CustomerController : Controller
    {
        private readonly AppIdentityDbContext _context;

        public CustomerController(AppIdentityDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(string category, string primaryColor)
        {
            var products = _context.Products
                .Where(x => (x.Category == category ||category == null) &&
                           (x.PrimaryColor == primaryColor || primaryColor == null));
            
            
            return View(products);
        }
        
        public IActionResult ProductDescription(byte id, short price, string category, short numparts, string imglink)
        {
            var product = _context.Products.FirstOrDefault(p => p.ProductId == id);
            if (product == null)
            {
                return NotFound(); // or handle the case where the product is not found
            }

            var relatedProductIds = _context.items_rec
                .Where(item => item.product_ID == id)
                .Select(item => item.recommendation_1)
                .Union(_context.items_rec.Where(item => item.product_ID == id).Select(item => item.recommendation_2))
                .Union(_context.items_rec.Where(item => item.product_ID == id).Select(item => item.recommendation_3))
                .Union(_context.items_rec.Where(item => item.product_ID == id).Select(item => item.recommendation_4))
                .Union(_context.items_rec.Where(item => item.product_ID == id).Select(item => item.recommendation_5))
                .ToList();

            var relatedProducts = _context.Products.Where(p => relatedProductIds.Contains(p.ProductId)).ToList();

            var productRecViewModel = new ProductRecViewModel
            {
                ProductsName = _context.Products,
                ProductName2 = product,
                RelatedProducts = relatedProducts
            };

            return View(productRecViewModel);
        }


        
        public IActionResult Cart()
        {
            return View();
        }

    }

}

