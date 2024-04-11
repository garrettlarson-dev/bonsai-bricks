namespace Identity.Models.ViewModels;

public class ProductViewModel
{
    public IEnumerable<Product> Products { get; set; }

    public PaginationInfo PaginationInfo { get; set; } = new PaginationInfo();
}

