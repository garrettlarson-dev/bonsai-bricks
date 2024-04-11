using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Identity.Models;

namespace Identity.Views.Customer;

public class CartModel : PageModel
{
    private AppIdentityDbContext _context;
    
    public CartModel(AppIdentityDbContext temp)
    {
        _context = temp;
    }
    public Cart? Cart { get; set; }

    public void OnGet()
    {
    }

    public void OnPost(int productId)
    {
        Product p = _context.Products.FirstOrDefault(x => x.ProductId == productId);
        Cart ??= new Cart(); // Ensure that the cart instance is not null
        Cart.AddItem(p, 1);
    }
}