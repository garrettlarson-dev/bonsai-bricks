namespace Identity.Models.ViewModelsTwo;

public class ProductRecViewModel
{
    public IQueryable<Product> ProductsName { get; set; }
    public Product ProductName2 { get; set; } = new Product();
    public List<Product> RelatedProducts { get; set; } = new List<Product>();
}