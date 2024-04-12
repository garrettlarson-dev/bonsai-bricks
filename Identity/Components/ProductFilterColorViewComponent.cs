using Identity.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace Identity.Components
{
    public class ProductFilterColorViewComponent : ViewComponent
    {
        private AppIdentityDbContext _context;

        public ProductFilterColorViewComponent(AppIdentityDbContext context)
        {
            _context = context;
        }
    
        // public class FilterOptions
        // {
        //     public List<string> Colors { get; set; }
        // }

        public IViewComponentResult Invoke()
        {
            ViewBag.SelectedColor = RouteData?.Values["color"];

            var colors = _context.Products
                .Select(x => x.PrimaryColor)
                .Distinct()
                .OrderBy(x => x);
            // Materialize the query to a list

            // var filterOptions = new FilterOptions
            // {
            //     Colors = colors
            // };
        
            return View(colors);
        }
    }

}