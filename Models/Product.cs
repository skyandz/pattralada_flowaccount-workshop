namespace FlowAccount_Workshop.Models;

public sealed class Product
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Sku { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public required string Category { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
