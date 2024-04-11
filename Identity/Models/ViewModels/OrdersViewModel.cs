namespace Identity.Models.ViewModels;

public class OrdersViewModel
{
    public IEnumerable<Order> Orders { get; set; }

    public PaginationInfo PaginationInfo { get; set; } = new PaginationInfo();
}