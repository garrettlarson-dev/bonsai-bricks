using Identity.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace Identity.Components
{
    public class ProductFilterCategoryViewComponent : ViewComponent
    {
        private AppIdentityDbContext _context;

        public ProductFilterCategoryViewComponent(AppIdentityDbContext context)
        {
            _context = context;
        }
        
        // public class FilterOptions
        // {
        //     public List<string> Categories { get; set; }
        // }

        public IViewComponentResult Invoke()
        {
            ViewBag.SelectedCategory = RouteData?.Values["Category"];

            var categories = _context.Products
                    .Select(x => x.Category)
                    .Distinct()
                    .OrderBy(x => x)
                ;

            // var filterOptions = new FilterOptions
            // {
            //     Categories = categories
            // };

            return View(categories);
        }
    }
}