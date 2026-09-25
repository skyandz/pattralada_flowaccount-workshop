using FlowAccount_Workshop.Dtos;

namespace FlowAccount_Workshop.Services;

public interface IProductService
{
    Task<bool> SkuExistsAsync(string sku, CancellationToken cancellationToken);
    Task<ProductResponse> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken);
}
