using FlowAccount_Workshop.Data;
using FlowAccount_Workshop.Endpoints;
using FlowAccount_Workshop.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<ProductDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Data Source=products.db"));
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddProblemDetails();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var database = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
    database.Database.EnsureCreated();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.MapProductEndpoints();

app.Run();

public partial class Program;
