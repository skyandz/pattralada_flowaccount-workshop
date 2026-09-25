using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FlowAccount_Workshop.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FlowAccount_Workshop.Tests;

public sealed class ProductApiTests : IClassFixture<ProductApiFactory>
{
    private readonly HttpClient client;

    public ProductApiTests(ProductApiFactory factory)
    {
        client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateProduct_WithValidData_ReturnsCreatedProduct()
    {
        var response = await client.PostAsJsonAsync("/api/products", ValidProduct("FOOD001"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var product = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("FOOD001", product.GetProperty("sku").GetString());
        Assert.Equal(20, product.GetProperty("stock").GetInt32());
        Assert.Equal(45.00m, product.GetProperty("price").GetDecimal());
        Assert.True(product.GetProperty("id").GetInt32() > 0);
        Assert.True(product.GetProperty("createdAt").GetDateTimeOffset() > DateTimeOffset.MinValue);
    }

    [Theory]
    [InlineData("", "name")]
    public async Task CreateProduct_WithEmptyName_ReturnsValidationError(string name, string field)
    {
        var response = await client.PostAsJsonAsync("/api/products", ValidProduct("NAME001") with { Name = name });

        await AssertValidationError(response, field);
    }

    [Fact]
    public async Task CreateProduct_WithEmptySku_ReturnsValidationError()
    {
        var response = await client.PostAsJsonAsync("/api/products", ValidProduct("") with { Sku = "" });

        await AssertValidationError(response, "sku");
    }

    [Fact]
    public async Task CreateProduct_WithShortSku_ReturnsValidationError()
    {
        var response = await client.PostAsJsonAsync("/api/products", ValidProduct("AB") with { Sku = "AB" });

        await AssertValidationError(response, "sku");
    }

    [Fact]
    public async Task CreateProduct_WithDuplicateSku_ReturnsValidationError()
    {
        var first = await client.PostAsJsonAsync("/api/products", ValidProduct("DUP001"));
        var second = await client.PostAsJsonAsync("/api/products", ValidProduct("DUP001"));

        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        await AssertValidationError(second, "sku");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public async Task CreateProduct_WithInvalidPrice_ReturnsValidationError(decimal price)
    {
        var response = await client.PostAsJsonAsync("/api/products", ValidProduct("PRICE" + price) with { Price = price });

        await AssertValidationError(response, "price");
    }

    [Fact]
    public async Task CreateProduct_WithNegativeStock_ReturnsValidationError()
    {
        var response = await client.PostAsJsonAsync("/api/products", ValidProduct("STOCK001") with { Stock = -1 });

        await AssertValidationError(response, "stock");
    }

    [Fact]
    public async Task CreateProduct_WithInvalidCategory_ReturnsValidationError()
    {
        var response = await client.PostAsJsonAsync("/api/products", ValidProduct("CAT001") with { Category = "invalid" });

        await AssertValidationError(response, "category");
    }

    [Fact]
    public async Task CreateProduct_WithZeroStock_ReturnsCreatedProduct()
    {
        var response = await client.PostAsJsonAsync("/api/products", ValidProduct("ZERO001") with { Stock = 0 });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    private static ProductRequest ValidProduct(string sku) =>
        new("ข้าวผัด", sku, 45.00m, 20, "อาหาร");

    private static async Task AssertValidationError(HttpResponseMessage response, string field)
    {
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains(field, body, StringComparison.OrdinalIgnoreCase);
    }

    private sealed record ProductRequest(string Name, string Sku, decimal Price, int Stock, string Category);
}

public sealed class ProductApiFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection connection = new("Data Source=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            connection.Open();
            var descriptor = services.Single(service => service.ServiceType == typeof(DbContextOptions<ProductDbContext>));
            services.Remove(descriptor);
            services.AddDbContext<ProductDbContext>(options => options.UseSqlite(connection));

            using var scope = services.BuildServiceProvider().CreateScope();
            scope.ServiceProvider.GetRequiredService<ProductDbContext>().Database.EnsureCreated();
        });
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            connection.Dispose();
        }

        base.Dispose(disposing);
    }
}
