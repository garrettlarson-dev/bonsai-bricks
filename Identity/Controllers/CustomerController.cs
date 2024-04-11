using Identity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Identity.CustomTagHelpers;

namespace Identity.Controllers
{
    public class CustomerController : Controller
    {
        private readonly AppIdentityDbContext _context;
        private const string CartSessionKey = "Cart";

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
        
        
        [HttpPost]
        public IActionResult AddToCart(byte productId, int quantity = 1)
        {
            var product = _context.Products.Find(productId);
            if (product == null)
            {
                return NotFound();
            }

            AddOrUpdateCartLine(product, quantity);

            return RedirectToAction("Cart");
        }

        private void AddOrUpdateCartLine(Product product, int quantity)
        {
            var cart = GetCurrentCart();

            var line = cart.Lines.FirstOrDefault(l => l.Product.ProductId == product.ProductId);
            if (line != null)
            {
                // Update quantity if product exists
                line.Quantity += quantity;
            }
            else
            {
                // Add new line if product does not exist in cart
                cart.Lines.Add(new Cart.CartLine
                {
                    Product = product,
                    Quantity = quantity
                });
            }

            HttpContext.Session.SetObjectAsJson(CartSessionKey, cart);
        }
        
        public IActionResult Cart()
        {
            var cart = GetCurrentCart();
            return View(cart);
        }
        
        private Cart GetCurrentCart()
        {
            var cart = HttpContext.Session.GetObjectFromJson<Cart>(CartSessionKey);
            if (cart == null)
            {
                cart = new Cart();
                HttpContext.Session.SetObjectAsJson(CartSessionKey, cart);
            }
            return cart;
        }
        
        [HttpPost]
        public IActionResult UpdateCart(List<Cart.CartLine> items)
        {
            var cart = GetCurrentCart();
            
            foreach (var item in items)
            {
                var line = cart.Lines.FirstOrDefault(l => l.Product.ProductId == item.Product.ProductId);
                if (line != null)
                {
                    line.Quantity = item.Quantity;
                }
            }

            HttpContext.Session.SetObjectAsJson(CartSessionKey, cart);

            return RedirectToAction("Cart");
        }

        [HttpPost]
        public IActionResult RemoveFromCart(byte productId)
        {
            var cart = GetCurrentCart();

            var line = cart.Lines.FirstOrDefault(l => l.Product.ProductId == productId);
            if (line != null)
            {
                cart.Lines.Remove(line);
            }

            HttpContext.Session.SetObjectAsJson(CartSessionKey, cart);

            return RedirectToAction("Cart");
        }
        
        

    }

}

