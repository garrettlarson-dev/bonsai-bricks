using Identity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Identity.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private const string CartSessionKey = "Cart";
        private UserManager<AppUser> userManager;
        private SignInManager<AppUser> signInManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private IPasswordHasher<AppUser> passwordHasher;

        public AccountController(UserManager<AppUser> usrMgr, SignInManager<AppUser> signInMan, RoleManager<IdentityRole> roleMngr)
        {
            userManager = usrMgr;
            signInManager = signInMan;
            roleManager = roleMngr;
        }

        [AllowAnonymous]
        public IActionResult Login(string returnUrl)
        {
            Login login = new Login();
            login.ReturnUrl = returnUrl;
            return View(login);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(Login login)
        {
            if (ModelState.IsValid)
            {
                AppUser appUser = await userManager.FindByEmailAsync(login.Email);
                if (appUser != null)
                {
                    await signInManager.SignOutAsync();
                    Microsoft.AspNetCore.Identity.SignInResult result = await signInManager.PasswordSignInAsync(appUser, login.Password, login.Remember, false);

                    if (result.Succeeded)
                    {
                        // Check if the user is an Admin
                        if (await userManager.IsInRoleAsync(appUser, "Admin"))
                        {
                            return RedirectToAction("Index", "Admin");
                        }
                        else // User is a Customer
                        {
                            return RedirectToAction("Index", "Home");
                        }
                    }
                    // If login fails, show error message
                    ModelState.AddModelError(nameof(login.Email), "Login Failed: Invalid Email or password");
                }
                else
                {
                    // If user is not found, show error message
                    ModelState.AddModelError(nameof(login.Email), "Login Failed: Invalid Email or password");
                }
            }
            // If model state is invalid, return the login view
            return View(login);
        }


        // GET: /Account/Register
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Register()
        {
            var lastCustomerId = await userManager.Users.MaxAsync(u => (int?)u.CustomerId) ?? 0; // If there are no users, default to 0
            // Use a ViewBag or ViewData to pass the last CustomerId to the view.
            ViewBag.LastCustomerId = lastCustomerId + 1; // Increment by 1 to get the next ID
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(User model)
        {
            if (ModelState.IsValid)
            {
                var user = new AppUser
                {
                    UserName = model.Name,
                    Email = model.Email,
                    CustomerId = model.CustomerId,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    BirthDate = model.BirthDate,
                    CountryOfResidence = model.CountryOfResidence,
                    Gender = model.Gender,
                    Age = model.Age,
                };

                var result = await userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Assign the role "Customer" to the user
                    await userManager.AddToRoleAsync(user, "Customer");

                    // Sign in the user
                    await signInManager.SignInAsync(user, isPersistent: false);

                    // Redirect to Home/Index after successful registration
                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        
        [Authorize]
public async Task<IActionResult> UpdateCustomer()
{
    var user = await userManager.GetUserAsync(User);
    if (user != null)
        return View(user);
    else
        return NotFound(); // Handle the case where the user is not found
}

[HttpPost]
public async Task<IActionResult> UpdateCustomer(AppUser model)
{
    if (!ModelState.IsValid)
    {
        return View(model);
    }

    var user = await userManager.GetUserAsync(User);
    if (user == null)
    {
        return NotFound(); // Handle the case where the user is not found
    }

    // Update user properties
    user.Email = model.Email;
    user.FirstName = model.FirstName;
    user.LastName = model.LastName;
    user.CountryOfResidence = model.CountryOfResidence;
    user.BirthDate = model.BirthDate;
    user.Gender = model.Gender;
    user.Age = model.Age;

    // Update the user's password if provided
    if (!string.IsNullOrEmpty(model.PasswordHash))
    {
        user.PasswordHash = passwordHasher.HashPassword(user, model.PasswordHash);
    }

    var result = await userManager.UpdateAsync(user);
    if (result.Succeeded)
    {
        return RedirectToAction("Index", "Home"); // Redirect to home page after successful update
    }
    else
    {
        // Handle the case where the update fails
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError("", error.Description);
        }
        return View(model);
    }
}

        
        private void Errors(IdentityResult result)
        {
            foreach (IdentityError error in result.Errors)
                ModelState.AddModelError("", error.Description);
        }

        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            HttpContext.Session.Remove(CartSessionKey);
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        /*[AllowAnonymous]
        public IActionResult GoogleLogin()
        {
            string redirectUrl = Url.Action("GoogleResponse", "Account");
            var properties = signInManager.ConfigureExternalAuthenticationProperties("Google", redirectUrl);
            return new ChallengeResult("Google", properties);
        }

        [AllowAnonymous]
        public async Task<IActionResult> GoogleResponse()
        {
            ExternalLoginInfo info = await signInManager.GetExternalLoginInfoAsync();
            if (info == null)
                return RedirectToAction(nameof(Login));

            var result = await signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, false);
            string[] userInfo = { info.Principal.FindFirst(ClaimTypes.Name).Value, info.Principal.FindFirst(ClaimTypes.Email).Value };
            if (result.Succeeded)
                return View(userInfo);
            else
            {
                AppUser user = new AppUser
                {
                    Email = info.Principal.FindFirst(ClaimTypes.Email).Value,
                    UserName = info.Principal.FindFirst(ClaimTypes.Email).Value
                };

                IdentityResult identResult = await userManager.CreateAsync(user);
                if (identResult.Succeeded)
                {
                    identResult = await userManager.AddLoginAsync(user, info);
                    if (identResult.Succeeded)
                    {
                        await signInManager.SignInAsync(user, false);
                        return View(userInfo);
                    }
                }
                return AccessDenied();
            }
        }*/

        [AllowAnonymous]
        public async Task<IActionResult> LoginTwoStep(string email, string returnUrl)
        {
            var user = await userManager.FindByEmailAsync(email);

            var token = await userManager.GenerateTwoFactorTokenAsync(user, "Email");

            EmailHelper emailHelper = new EmailHelper();
            bool emailResponse = emailHelper.SendEmailTwoFactorCode(user.Email, token);

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> LoginTwoStep(TwoFactor twoFactor, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(twoFactor.TwoFactorCode);
            }

            var result = await signInManager.TwoFactorSignInAsync("Email", twoFactor.TwoFactorCode, false, false);
            if (result.Succeeded)
            {
                return Redirect(returnUrl ?? "/");
            }
            else
            {
                ModelState.AddModelError("", "Invalid Login Attempt");
                return View();
            }
        }

        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([Required] string email)
        {
            if (!ModelState.IsValid)
                return View(email);

            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
                return RedirectToAction(nameof(ForgotPasswordConfirmation));

            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var link = Url.Action("ResetPassword", "Account", new { token, email = user.Email }, Request.Scheme);

            EmailHelper emailHelper = new EmailHelper();
            bool emailResponse = emailHelper.SendEmailPasswordReset(user.Email, link);

            if (emailResponse)
                return RedirectToAction("ForgotPasswordConfirmation");
            else
            {
                // log email failed 
            }
            return View(email);
        }

        [AllowAnonymous]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult ResetPassword(string token, string email)
        {
            var model = new ResetPassword { Token = token, Email = email };
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(ResetPassword resetPassword)
        {
            if (!ModelState.IsValid)
                return View(resetPassword);

            var user = await userManager.FindByEmailAsync(resetPassword.Email);
            if (user == null)
                RedirectToAction("ResetPasswordConfirmation");

            var resetPassResult = await userManager.ResetPasswordAsync(user, resetPassword.Token, resetPassword.Password);
            if (!resetPassResult.Succeeded)
            {
                foreach (var error in resetPassResult.Errors)
                    ModelState.AddModelError(error.Code, error.Description);
                return View();
            }

            return RedirectToAction("ResetPasswordConfirmation");
        }

        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }
    }
}
