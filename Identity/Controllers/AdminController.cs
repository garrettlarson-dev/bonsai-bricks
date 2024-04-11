using Identity.Models;
using Microsoft.AspNetCore.Authorization;
using Identity.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Identity.Controllers
{
    
    public class AdminController : Controller
    {
        private UserManager<AppUser> userManager;
        private IPasswordHasher<AppUser> passwordHasher;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly AppIdentityDbContext _context;

        public AdminController(UserManager<AppUser> usrMgr, IPasswordHasher<AppUser> passwordHash, RoleManager<IdentityRole> roleMgr, AppIdentityDbContext context)
        {
            userManager = usrMgr;
            passwordHasher = passwordHash;
            roleManager = roleMgr;
            _context = context;
        }
        
        [Authorize]
        public IActionResult Index()
        {
            return View();
        }
        
        public async Task<IActionResult> Orders(int pageNum)
        {
            int pageSize = 100;
            var orders = new OrdersViewModel
            {
                Orders = _context.Orders
                    .OrderByDescending(o => o.Date)
                    .Skip(pageSize * (Math.Max(1, pageNum - 1))) // Skip for pagination based on selectedPageSize
                    .Take(pageSize), // Take for pagination based on selectedPageSize
                PaginationInfo = new PaginationInfo
                {
                    CurrentPage = pageNum,
                    ItemsPerPage = pageSize,
                    TotalItems = _context.Orders.Count()
                }
            };
            return View(orders);
        }

        [Authorize]
        public IActionResult AllUsers()
        {
            return View(userManager.Users);
        }

        [Authorize]
        public async Task<IActionResult> Create()
        {
            var lastCustomerId = await userManager.Users.MaxAsync(u => (int?)u.CustomerId) ?? 0; // If there are no users, default to 0
            // Use a ViewBag or ViewData to pass the last CustomerId to the view.
            ViewBag.LastCustomerId = lastCustomerId + 1; // Increment by 1 to get the next ID
            ViewBag.RoleList = new SelectList(
                await roleManager.Roles.OrderByDescending(r => r.Name).ToListAsync(),
                "Name",
                "Name"
            );
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(User user, string[] Roles)
        {
            if (ModelState.IsValid)
            {
                AppUser appUser = new AppUser
                {
                    UserName = user.Name,
                    Email = user.Email,
                    CustomerId = user.CustomerId,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    BirthDate = user.BirthDate,
                    CountryOfResidence = user.CountryOfResidence,
                    Gender = user.Gender,
                    Age = user.Age,
                    //TwoFactorEnabled = true
                };

                IdentityResult result = await userManager.CreateAsync(appUser, user.Password);
                
                if (result.Succeeded)
                {
                    // Assign roles to the user
                    foreach (var roleName in Roles)
                    {
                        if (!await userManager.IsInRoleAsync(appUser, roleName))
                        {
                            var roleResult = await userManager.AddToRoleAsync(appUser, roleName);
                            if (!roleResult.Succeeded)
                            {
                                // Handle error
                                Errors(roleResult);
                            }
                        }
                    }

                    // Redirect or handle success
                    return RedirectToAction("AllUsers");
                }
                else
                {
                    // Handle failure
                    foreach (IdentityError error in result.Errors)
                        ModelState.AddModelError("", error.Description);
                }
            }
            // Repopulate roles list if there's a return to form
            ViewBag.RoleList = new SelectList(await roleManager.Roles.ToListAsync(), "Name", "Name");
            return View(user);
        }

        [Authorize]
        public async Task<IActionResult> Update(string id)
        {
            AppUser user = await userManager.FindByIdAsync(id);
            if (user != null)
            {
                ViewBag.RoleList = new SelectList(
                    await roleManager.Roles.OrderByDescending(r => r.Name).ToListAsync(),
                    "Name",
                    "Name"
                );                
                var userRoles = await userManager.GetRolesAsync(user);
                var isCustomer = await userManager.IsInRoleAsync(user, "Customer");
                if (isCustomer)
                    ViewBag.CurrentRole = "Customer";
                else
                    ViewBag.CurrentRole = "Admin";

                return View(user);
            }
            return RedirectToAction("AllUsers");
        }

        [HttpPost]
        public async Task<IActionResult> Update(string id, string email, string password, string firstName, string lastName, DateOnly birthDate, string countryOfResidence, string gender, double age, string[] Roles)
        {
            AppUser user = await userManager.FindByIdAsync(id);
            if (user != null)
            {
                if (!string.IsNullOrEmpty(email))
                    user.Email = email;
                else
                    ModelState.AddModelError("", "Email cannot be empty");

                if (!string.IsNullOrEmpty(password))
                    user.PasswordHash = passwordHasher.HashPassword(user, password);
                else
                    ModelState.AddModelError("", "Password cannot be empty");

                if (!string.IsNullOrEmpty(firstName))
                    user.FirstName = firstName;
                else
                    ModelState.AddModelError("", "First Name cannot be empty");
               
                if (!string.IsNullOrEmpty(lastName))
                    user.LastName = lastName;
                else
                    ModelState.AddModelError("", "Last Name cannot be empty");
                
                if (!string.IsNullOrEmpty(countryOfResidence))
                    user.CountryOfResidence = countryOfResidence;
                else
                    ModelState.AddModelError("", "Country of Residence cannot be empty");
                
                if (!string.IsNullOrEmpty(birthDate.ToString()))
                    user.BirthDate = birthDate;
                else
                    ModelState.AddModelError("", "Birth Date cannot be empty");

                if (!string.IsNullOrEmpty(gender))
                    user.Gender = gender;
                else
                    ModelState.AddModelError("", "Gender cannot be empty");
                
                if (age != 0)
                    user.Age = age;
                else
                    ModelState.AddModelError("", "Age cannot be empty");
                
                if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(password))
                {
                    IdentityResult result = await userManager.UpdateAsync(user);
                    if (result.Succeeded)
                    {
                        // Assign roles to the user
                        foreach (var roleName in Roles)
                        {
                            if (!await userManager.IsInRoleAsync(user, roleName))
                            {
                                var roleResult = await userManager.AddToRoleAsync(user, roleName);
                                if (!roleResult.Succeeded)
                                {
                                    // Handle error
                                    Errors(roleResult);
                                }
                            }
                        }

                        // Redirect or handle success
                        return RedirectToAction("AllUsers");
                    }
                    else
                    {
                        // Handle failure
                        foreach (IdentityError error in result.Errors)
                            ModelState.AddModelError("", error.Description);
                    }
                }
            }
            else
                ModelState.AddModelError("", "User Not Found");
            return View(user);
        }

        private void Errors(IdentityResult result)
        {
            foreach (IdentityError error in result.Errors)
                ModelState.AddModelError("", error.Description);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            AppUser user = await userManager.FindByIdAsync(id);
            if (user != null)
            {
                IdentityResult result = await userManager.DeleteAsync(user);
                if (result.Succeeded)
                    return RedirectToAction("AllUsers");
                else
                    Errors(result);
            }
            else
                ModelState.AddModelError("", "User Not Found");
            return View("AllUsers", userManager.Users);
        }
        
        [HttpGet]
        public IActionResult EditProducts(int id)
        {
            var product = _context.Products.FirstOrDefault(p => p.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }
        
        [HttpPost]
        public IActionResult EditProducts(Product product)
        {
            if (ModelState.IsValid)
            {
                // Update the product in the database
                _context.Products.Update(product);
                _context.SaveChanges();
        
                // Redirect to the index page
                return RedirectToAction("Index", "Customer");
            }

            // If the model state is not valid, return the view with the validation errors
            return View(product);
        }
        
        [HttpGet]
        public IActionResult AddProducts()
        {
            return View();
        }

        [HttpPost]
        public IActionResult AddProducts(Product product)
        {
            if (ModelState.IsValid)
            {
                byte maxProductId = _context.Products.Max(p => p.ProductId);

                // Ensure that the new ProductID doesn't exceed 255
                if (maxProductId == byte.MaxValue)
                {
                    // Handle the case where the maximum ProductID has reached the maximum value
                    // You could throw an exception or handle it in another way
                    return BadRequest("Maximum ProductID reached.");
                }

                // Assign the new ProductID as the maximum ProductID + 1
                product.ProductId = (byte)(maxProductId + 1);
                
                // Add the product to your database
                _context.Products.Add(product);
                _context.SaveChanges();
        
                return RedirectToAction("Index", "Customer");
            }

            return View(product); // If model state is not valid, return the form with errors
        }
        
        [HttpGet]
        public IActionResult DeleteProduct(int id)
        {
            var product = _context.Products.FirstOrDefault(p => p.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }
        
        [HttpPost]
        public IActionResult DeleteProduct(Product product)
        {
            if (ModelState.IsValid)
            {
                // Update the product in the database
                _context.Products.Remove(product);
                _context.SaveChanges();
        
                // Redirect to the index page
                return RedirectToAction("Index", "Customer");
            }

            // If the model state is not valid, return the view with the validation errors
            return View(product);
        }
        

    }
}
