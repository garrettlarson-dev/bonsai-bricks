using Identity.Models;
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
        private readonly AppIdentityDbContext _context;

        public AdminController(UserManager<AppUser> usrMgr, IPasswordHasher<AppUser> passwordHash, AppIdentityDbContext context)
        {
            userManager = usrMgr;
            passwordHasher = passwordHash;
            _context = context;
        }
        
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult AllUsers()
        {
            return View(userManager.Users);
        }

        public async Task<IActionResult> Create()
        {
            var lastCustomerId = await userManager.Users.MaxAsync(u => (int?)u.CustomerId) ?? 0; // If there are no users, default to 0
            // Use a ViewBag or ViewData to pass the last CustomerId to the view.
            ViewBag.LastCustomerId = lastCustomerId + 1; // Increment by 1 to get the next ID
            
            // Fetch the roles from the database
            var roles = await roleManager.Roles.ToListAsync();

            // Create a SelectList, assuming your roles have 'Id' and 'Name' properties
            ViewBag.RoleList = new SelectList(roles, "Name", "Name");
            
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
                    var token = await userManager.GenerateEmailConfirmationTokenAsync(appUser);
                    var confirmationLink = Url.Action("ConfirmEmail", "Email", new { token, email = user.Email }, Request.Scheme);
                    EmailHelper emailHelper = new EmailHelper();
                    bool emailResponse = emailHelper.SendEmail(user.Email, confirmationLink);
             
                    if (emailResponse)
                        return RedirectToAction("Index");
                    else
                    {
                        // log email failed 
                    }
                }
                
                // Assign selected roles to the user
                foreach (var roleName in Roles)
                {
                    if (!await userManager.IsInRoleAsync(appUser, roleName))
                    {
                        var roleResult = await userManager.AddToRoleAsync(appUser, roleName);
                        if (!roleResult.Succeeded)
                        {
                            // Handle error
                        }
                    }
                }
                
                
                if (result.Succeeded)
                    return RedirectToAction("AllUsers");
                else
                {
                    foreach (IdentityError error in result.Errors)
                        ModelState.AddModelError("", error.Description);
                }
            }
            return View(user);
        }

        public async Task<IActionResult> Update(string id)
        {
            AppUser user = await userManager.FindByIdAsync(id);
            if (user != null)
                return View(user);
            else
                return RedirectToAction("AllUsers");
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
        
        

        [HttpPost]
        public async Task<IActionResult> Update(string id, string email, string password, string firstName, string lastName, DateOnly birthDate, string countryOfResidence, string gender, double age)
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
                        return RedirectToAction("AllUsers");
                    else
                        Errors(result);
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
        
        
    }
}
