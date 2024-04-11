namespace Identity.Models;

public class Cart
{
    public List<CartLine> Lines { get; set; } = new List<CartLine>();

    public void AddItem(Product p, int quantity)
    {
        CartLine line = Lines
            .Where(x => x.Product.ProductId == p.ProductId)
            .FirstOrDefault();
        
        // Has this item already been added to our cart? Add line to quantity
        if (line == null)
        {
            // Corrected syntax to add a new CartLine to Lines
            Lines.Add(new CartLine
            {
                Product = p,
                Quantity = quantity
            });
        }
        else
        {
            line.Quantity += quantity;
        }
    }

    public void RemoveLine(Product p) => Lines.RemoveAll(x => x.Product.ProductId == p.ProductId);

    public void Clear() => Lines.Clear();

    public int CalculateTotal() => Lines.Sum(x => (x.Product?.Price ?? 0) * x.Quantity);


    public class CartLine
    {
        public int CartLineId { get; set; }
        
        // Corrected property name to match with LINQ query
        public Product Product { get; set; }

        public int Quantity { get; set; }
    }
}
