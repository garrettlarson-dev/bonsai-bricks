namespace Identity.Models.ViewModels;

public class ProductListViewModel
{
    public IEnumerable<Product> Products { get; set; }

    public PaginationInfo PaginationInfo { get; set; } = new PaginationInfo();
    
    public string? CurrentCategory { get; set; }
    
    public string? CurrentColor { get; set; }
    
    public int? CurrentPageSize { get; set; }
}