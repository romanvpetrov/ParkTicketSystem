using TicketSystem.Infrastructure.Products;

namespace TicketSystem.Tests.Infrastructure;

public class ProductRepositoryTests
{
    [Fact]
    public void GetByParkUniqueName_ReturnsDefaultTicketProducts()
    {
        var repository = new InMemoryProductRepository();

        var products = repository.GetByParkUniqueName("adventurebay");

        Assert.Equal(3, products.Count);

        Assert.Equal("adventurebay-entry", products[0].Id);
        Assert.Equal("Adventure Bay Entry", products[0].Name);
        Assert.Equal(50, products[0].DefaultAvailableTicketCount);
        Assert.Equal(79.99m, products[0].Price);

        Assert.Equal("adventurebay-plus", products[1].Id);
        Assert.Equal("Adventure Bay Plus", products[1].Name);
        Assert.Equal(25, products[1].DefaultAvailableTicketCount);
        Assert.Equal(99.99m, products[1].Price);

        Assert.Equal("adventurebay-premium", products[2].Id);
        Assert.Equal("Adventure Bay Premium", products[2].Name);
        Assert.Equal(25, products[2].DefaultAvailableTicketCount);
        Assert.Equal(129.99m, products[2].Price);
    }
}
