namespace Identity.Models.ViewModels;

public class ReviewOrderViewModel
{
    public Cart Cart { get; set; }
    public decimal ShippingCost { get; set; }
    public string ShippingAddress { get; set; }
    public string BillingAddress { get; set; }
    public string PaymentInfo { get; set; }

    // You can add methods to calculate the subtotal, taxes, etc. here.
}