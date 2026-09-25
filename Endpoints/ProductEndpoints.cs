using FlowAccount_Workshop.Dtos;
using FlowAccount_Workshop.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FlowAccount_Workshop.Endpoints;

public static class ProductEndpoints
{
    private static readonly string[] AllowedCategories =
        ["อาหาร", "เครื่องดื่ม", "ของใช้", "เสื้อผ้า"];

    public static void MapProductEndpoints(this WebApplication app)
    {
        app.MapPost("/api/products", CreateProduct)
            .WithName("CreateProduct")
            .WithSummary("Create a product")
            .WithDescription("Creates a product after validating its name, SKU, price, stock, and category.")
            .Produces<ProductResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem();
    }

    private static async Task<Results<Created<ProductResponse>, ValidationProblem>> CreateProduct(
        CreateProductRequest request,
        IProductService productService,
        CancellationToken cancellationToken)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            errors["name"] = ["Name must not be empty."];
        }

        if (string.IsNullOrWhiteSpace(request.Sku))
        {
            errors["sku"] = ["SKU must not be empty."];
        }
        else if (request.Sku.Trim().Length < 3)
        {
            errors["sku"] = ["SKU must have at least 3 characters."];
        }

        if (request.Price <= 0)
        {
            errors["price"] = ["Price must be greater than 0."];
        }

        if (request.Stock < 0)
        {
            errors["stock"] = ["Stock must be greater than or equal to 0."];
        }

        if (string.IsNullOrWhiteSpace(request.Category) ||
            !AllowedCategories.Contains(request.Category.Trim(), StringComparer.Ordinal))
        {
            errors["category"] = ["Category must be one of: อาหาร, เครื่องดื่ม, ของใช้, เสื้อผ้า."];
        }

        if (errors.Count > 0)
        {
            return TypedResults.ValidationProblem(errors);
        }

        var sku = request.Sku!.Trim();
        if (await productService.SkuExistsAsync(sku, cancellationToken))
        {
            return TypedResults.ValidationProblem(new Dictionary<string, string[]>
            {
                ["sku"] = ["SKU must be unique."]
            });
        }

        var product = await productService.CreateAsync(request, cancellationToken);
        return TypedResults.Created($"/api/products/{product.Id}", product);
    }
}
