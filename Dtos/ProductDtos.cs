namespace FlowAccount_Workshop.Dtos;

/// <summary>Payload used to create a product.</summary>
public sealed record CreateProductRequest
{
    public string? Name { get; init; }
    public string? Sku { get; init; }
    public decimal Price { get; init; }
    public int Stock { get; init; }
    public string? Category { get; init; }
}

/// <summary>Product returned by the API.</summary>
public sealed record ProductResponse(
    int Id,
    string Name,
    string Sku,
    decimal Price,
    int Stock,
    string Category,
    DateTimeOffset CreatedAt);
