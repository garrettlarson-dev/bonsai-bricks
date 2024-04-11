namespace Identity.Models.ViewModels;

public class ProductListViewModel
{
    public IQueryable<Product> Products { get; set; }

    public PaginationInfo PaginationInfo { get; set; } = new PaginationInfo();
    
    public string? CurrentCategory { get; set; }
    
    public string? CurrentColor { get; set; }
}