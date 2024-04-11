using Identity.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace Identity.Components
{
    public class ProductFilterViewComponent : ViewComponent
    {
        private AppIdentityDbContext _context;

        public ProductFilterViewComponent(AppIdentityDbContext context)
        {
            _context = context;
        }
        
        public class FilterOptions
        {
            public List<string> Categories { get; set; }
            public List<string> Colors { get; set; }
        }

        public IViewComponentResult Invoke()
        {
            ViewBag.SelectedCategory = RouteData?.Values["category"];
            
            var categories = _context.Products
                .Select(x => x.Category)
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            ViewBag.SelectedColor = RouteData?.Values["color"];
            
            var colors = _context.Products
                .Select(x => x.PrimaryColor)
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            var filterOptions = new FilterOptions
            {
                Categories = categories,
                Colors = colors
            };

            return View("Default", filterOptions);
        }
    }
}