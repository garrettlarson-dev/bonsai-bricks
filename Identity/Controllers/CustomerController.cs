using Identity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Identity.Models.ViewModelsTwo;
using Identity.Models.ViewModels;
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
        
        public async Task<IActionResult> Index(string category, string primaryColor, int pageNum, int pageSize=6)
        {
            
            var products = new ProductViewModel
            {
                Products = _context.Products
                    .Skip(pageSize * (Math.Max(1, pageNum - 1))) // Skip for pagination based on selectedPageSize
                    .Take(pageSize).Where(x => (x.Category == category ||category == null) &&
                           (x.PrimaryColor == primaryColor || primaryColor == null)), // Take for pagination based on selectedPageSize
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
        
        
        
        [HttpPost]
        public IActionResult ConfirmOrder()
        {
            // Retrieve the cart from the session
            var cart = GetCurrentCart();
        
            // Here you should add the logic to process the cart and create an order.
            // This typically involves creating an order record in the database,
            // decrementing stock, processing payment, etc.

            // After order processing:
            // You might want to clear the cart:
            HttpContext.Session.Remove(CartSessionKey);
        
            // And then redirect to an order confirmation view or another appropriate view
            return View("OrderConfirmation"); // Make sure to create this view
        }
        
        

    }

}

