using Identity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Identity.Models.ViewModelsTwo;
using Identity.Models.ViewModels;
using Identity.CustomTagHelpers;
using Newtonsoft.Json;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace Identity.Controllers
{
    public class CustomerController : Controller
    {
        private readonly AppIdentityDbContext _context;
        private UserManager<AppUser> userManager;
        private const string CartSessionKey = "Cart";
        private readonly PredictionService _prediction;

        public CustomerController(AppIdentityDbContext context, UserManager<AppUser> userMgr, PredictionService prediction)
        {
            _context = context;
            userManager = userMgr;
            _prediction = prediction;
        }

        public IActionResult Index(string? category, string? primaryColor, int pageNum = 1)
        {
            int pageSize = 5;

            var blah = new ProductListViewModel()
            {

                Products = _context.Products
                    .Where(x => (x.Category == category ||category == null) &&
                                (x.PrimaryColor == primaryColor || primaryColor == null))
                    .OrderBy(x => x.Category)
                    .Skip((pageNum - 1) * pageSize)
                    .Take(pageSize),

                PaginationInfo = new PaginationInfo()
                {
                    CurrentPage = pageNum,
                    ItemsPerPage = pageSize,
                    TotalItems = category == null ? _context.Products.Count() : _context.Products.Where(x => x.Category == category).Count()
                },
                
                CurrentCategory = category,
                CurrentColor = primaryColor
            };
            return View(blah);
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
        
        [Authorize(Roles = "Customer")]
        public IActionResult Cart()
        {
            var cart = GetCurrentCart();
            if (cart.Lines.Count == 0)
            {
                return View("EmptyCart");
            }
            return View(cart);
        }
        
        [Authorize(Roles = "Customer")]
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
        
        public IActionResult LegoBuilder()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            // Check if the user is authenticated
            if (!User.Identity.IsAuthenticated)
            {
                string checkoutUrl = Url.Action("Checkout", "Customer");

                // Redirect to the Login action and pass the returnUrl
                return RedirectToAction("Login", "Account", new { returnUrl = checkoutUrl });
            }

            // If there is a logged-in user, check if they have the 'customer' role
            // This assumes that you have role-based authorization set up and that 'UserManager' and 'RoleManager' are available through dependency injection
            var user = await userManager.GetUserAsync(User);
            var isInRole = await userManager.IsInRoleAsync(user, "Customer");

            if (!isInRole)
            {
                return RedirectToAction("Index", "Home");
            }
            
            var cart = GetCurrentCart();
            if (cart.Lines.Count == 0)
            {
                return View("EmptyCart");
            }
            return View(cart);
        }
        

        
        [HttpPost]
        public async Task<IActionResult> PlaceOrder(string street, string city, string state, string zip, string country, string bank, string typeOfCard)
        {
            if (User == null)
            {
                // Handle the case where User is null
                return NotFound();
            }

            var user = await userManager.GetUserAsync(User); // Assuming you have a UserManager injected

            if (user == null)
            {
                // Handle the case where user is null
                return NotFound();
            }

            var customerId = user.CustomerId; // Now we know user is not null

            var cart = GetCurrentCart();
            
            var order = new Order
            {
                TransactionId = new Random().Next(100000, 999999),
                CustomerId = customerId,
                Date = DateOnly.FromDateTime(DateTime.Now),
                DayOfWeek = DateTime.Now.DayOfWeek.ToString(),
                Time = (byte)DateTime.Now.Hour,
                EntryMode = "CVC",
                Amount = cart.CalculateTotal(), // Calculate the amount based on the cart contents
                TypeOfTransaction = "Online",
                CountryOfTransaction = country,
                ShippingAddress = $"{street}, {city}, {state}, {zip}, {country}",
                Bank = bank,
                TypeOfCard = typeOfCard,
                Fraud = 0 // Determine how to set this value in your application logic
            };
            
            var fraud = _prediction.PredictFraud(order, user);
            order.Fraud = (byte) fraud;
            HttpContext.Session.Remove(CartSessionKey);
            _context.Orders.Add(order);
            _context.SaveChanges();
            if (fraud == 0)
            {
                return View("OrderConfirmation", order); // Redirect to an order confirmation page
            }
            return View("OrderPending", order);
        }
        
        // public IActionResult FraudCheck()
        // {
        //     // Retrieve the order from TempData
        //     var orderJson = TempData["Order"] as string;
        //     var order = JsonConvert.DeserializeObject<Order>(orderJson);
        //
        //     
        // }
        
            [HttpGet]
        public IActionResult OrderConfirmation()
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
            return View(); // Make sure to create this view
        }       
        
        [HttpGet]
        public IActionResult OrderPending()
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
            return View(); // Make sure to create this view
        }

    }

}

