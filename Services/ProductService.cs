using FlowAccount_Workshop.Data;
using FlowAccount_Workshop.Dtos;
using FlowAccount_Workshop.Models;
using Microsoft.EntityFrameworkCore;

namespace FlowAccount_Workshop.Services;

public sealed class ProductService(ProductDbContext dbContext) : IProductService
{
    public Task<bool> SkuExistsAsync(string sku, CancellationToken cancellationToken) =>
        dbContext.Products.AnyAsync(product => product.Sku == sku, cancellationToken);

    public async Task<ProductResponse> CreateAsync(
        CreateProductRequest request,
        CancellationToken cancellationToken)
    {
        var product = new Product
        {
            Name = request.Name!.Trim(),
            Sku = request.Sku!.Trim(),
            Price = request.Price,
            Stock = request.Stock,
            Category = request.Category!,
            CreatedAt = DateTimeOffset.UtcNow
        };

        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new ProductResponse(
            product.Id,
            product.Name,
            product.Sku,
            product.Price,
            product.Stock,
            product.Category,
            product.CreatedAt);
    }
}
