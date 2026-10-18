using TicketSystem.Domain.Orders;
using TicketSystem.Infrastructure.Parks;
using TicketSystem.Infrastructure.Products;
using TicketSystem.Infrastructure.Shopping;

namespace TicketSystem.Tests.Infrastructure;

public class JsonRepositoryTests
{
    [Fact]
    public void ParkRepository_ReturnsAllSeededParks()
    {
        using var directory = new TemporaryDirectory();
        var repository = new JsonParkRepository(directory.Path, new InMemoryProductRepository());

        var parks = repository.GetAll();

        Assert.Equal(2, parks.Count);
        Assert.Equal("adventurebay", parks[0].UniqueName);
        Assert.Equal("splashharbor", parks[1].UniqueName);
    }

    [Fact]
    public void ParkRepository_ReturnsSplashHarborSeedData()
    {
        using var directory = new TemporaryDirectory();
        var productRepository = new InMemoryProductRepository();
        var repository = new JsonParkRepository(directory.Path, productRepository);

        var park = repository.GetByUniqueName("splashharbor");

        Assert.NotNull(park);
        Assert.Equal(2, park.Id);
        Assert.Equal("Splash Harbor", park.Name);
        Assert.Equal(2, park.Calendar.TicketProducts.Count);
        Assert.Equal(3, park.Calendar.DailyInventories.Count);
        Assert.Equal("splashharbor-entry", park.Calendar.DailyInventories[0].ProductId);
        Assert.Equal(32, park.Calendar.DailyInventories[0].AvailableItemCount);
        Assert.Equal("splashharbor-food-pass", park.Calendar.DailyInventories[1].ProductId);
        Assert.Equal(85, park.Calendar.DailyInventories[1].AvailableItemCount);
    }

    [Fact]
    public void ParkRepository_PersistsInventoryChangesAcrossInstances()
    {
        using var directory = new TemporaryDirectory();
        var productRepository = new InMemoryProductRepository();
        var repository = new JsonParkRepository(directory.Path, productRepository);
        var park = Assert.IsType<Domain.Parks.Park>(repository.GetByUniqueName("adventurebay"));
        var products = park.Calendar.TicketProducts;

        Assert.Equal(3, products.Count);
        Assert.Equal("adventurebay-entry", products[0].Id);
        Assert.Equal("Adventure Bay Entry", products[0].Name);
        Assert.Equal(79.99m, products[0].Price);
        Assert.Equal("adventurebay-plus", products[1].Id);
        Assert.Equal("Adventure Bay Plus", products[1].Name);
        Assert.Equal(99.99m, products[1].Price);
        Assert.Equal("adventurebay-premium", products[2].Id);
        Assert.Equal("Adventure Bay Premium", products[2].Name);
        Assert.Equal(129.99m, products[2].Price);

        var inventory = Assert.IsType<Domain.Parks.DailyInventory>(
            park.GetDailyInventory("adventurebay-entry", new DateOnly(2026, 7, 1)));
        var visitDate = inventory.Date;
        var salesResult = park.RecordTicketsSold(
        [
            CreateTicket("adventurebay-entry", visitDate),
            CreateTicket("adventurebay-entry", visitDate)
        ]);

        Assert.True(salesResult.Success);

        repository.Save(park);
        repository.Commit();

        var reloadedRepository = new JsonParkRepository(directory.Path, productRepository);
        var reloadedPark = Assert.IsType<Domain.Parks.Park>(
            reloadedRepository.GetByUniqueName("adventurebay"));
        var reloadedInventory = Assert.IsType<Domain.Parks.DailyInventory>(
            reloadedPark.GetDailyInventory("adventurebay-entry", new DateOnly(2026, 7, 1)));
        Assert.Equal(22, reloadedInventory.SoldItemCount);
    }

    [Fact]
    public void CartRepository_PersistsPaidCartAcrossInstances()
    {
        using var directory = new TemporaryDirectory();
        var repository = new JsonCartRepository(directory.Path);
        var cart = new Cart(Guid.NewGuid());
        var item = new CartItem(
            "adventurebay-entry",
            "adventurebay",
            new DateOnly(2026, 7, 1),
            1,
            79.99m);
        item.AssignTicketHolder(new TicketHolder("Ryan", "Peterson", "ryan@example.com"));
        cart.AddItem(item);
        cart.MarkPaid();

        repository.Save(cart);
        repository.Commit();

        var reloadedRepository = new JsonCartRepository(directory.Path);
        var reloadedCart = Assert.IsType<Cart>(reloadedRepository.GetById(cart.Id));
        var reloadedItem = Assert.Single(reloadedCart.Items);
        Assert.True(reloadedCart.IsPaid);
        Assert.Equal(item.Id, reloadedItem.Id);
        Assert.Equal("Ryan", reloadedItem.TicketHolder!.FirstName);
        Assert.Equal("Peterson", reloadedItem.TicketHolder.LastName);
        Assert.Equal("ryan@example.com", reloadedItem.TicketHolder.Email);
    }

    [Fact]
    public void TicketRepository_PersistsTicketAcrossInstances()
    {
        using var directory = new TemporaryDirectory();
        var repository = new JsonTicketRepository(directory.Path);
        var ticket = new Ticket(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "adventurebay-entry",
            "adventurebay",
            new DateOnly(2026, 7, 1),
            79.99m,
            "Ryan",
            "Peterson",
            "ryan@example.com");

        repository.Save(ticket);
        repository.Commit();

        var reloadedRepository = new JsonTicketRepository(directory.Path);
        var reloadedTicket = Assert.IsType<Ticket>(reloadedRepository.GetById(ticket.Id));
        Assert.Equal(ticket.CartId, reloadedTicket.CartId);
        Assert.Equal(ticket.CartItemId, reloadedTicket.CartItemId);
        Assert.Equal(ticket.ProductId, reloadedTicket.ProductId);
        Assert.Equal(ticket.VisitDate, reloadedTicket.VisitDate);
        Assert.Equal(ticket.Email, reloadedTicket.Email);
    }

    [Fact]
    public void Repositories_StageChangesUntilCommit()
    {
        using var directory = new TemporaryDirectory();
        var productRepository = new InMemoryProductRepository();
        var parkRepository = new JsonParkRepository(directory.Path, productRepository);
        var cartRepository = new JsonCartRepository(directory.Path);
        var ticketRepository = new JsonTicketRepository(directory.Path);
        var visitDate = new DateOnly(2026, 7, 1);
        var park = Assert.IsType<Domain.Parks.Park>(parkRepository.GetByUniqueName("adventurebay"));
        var inventory = Assert.IsType<Domain.Parks.DailyInventory>(
            park.GetDailyInventory("adventurebay-entry", visitDate));
        var originalSoldItemCount = inventory.SoldItemCount;
        var cart = new Cart(Guid.NewGuid());
        var ticket = new Ticket(
            Guid.NewGuid(),
            cart.Id,
            Guid.NewGuid(),
            "adventurebay-entry",
            "adventurebay",
            visitDate,
            79.99m,
            "Ryan",
            "Peterson",
            "ryan@example.com");

        var salesResult = park.RecordTicketsSold([ticket]);

        Assert.True(salesResult.Success);
        cartRepository.Save(cart);
        parkRepository.Save(park);
        ticketRepository.Save(ticket);

        var reloadedParkRepository = new JsonParkRepository(directory.Path, productRepository);
        var reloadedPark = Assert.IsType<Domain.Parks.Park>(
            reloadedParkRepository.GetByUniqueName("adventurebay"));
        var reloadedInventory = Assert.IsType<Domain.Parks.DailyInventory>(
            reloadedPark.GetDailyInventory("adventurebay-entry", visitDate));

        Assert.Equal(originalSoldItemCount, reloadedInventory.SoldItemCount);
        Assert.Null(new JsonCartRepository(directory.Path).GetById(cart.Id));
        Assert.Null(new JsonTicketRepository(directory.Path).GetById(ticket.Id));

        cartRepository.Commit();
        parkRepository.Commit();
        ticketRepository.Commit();

        var committedParkRepository = new JsonParkRepository(directory.Path, productRepository);
        var committedPark = Assert.IsType<Domain.Parks.Park>(
            committedParkRepository.GetByUniqueName("adventurebay"));
        var committedInventory = Assert.IsType<Domain.Parks.DailyInventory>(
            committedPark.GetDailyInventory("adventurebay-entry", visitDate));

        Assert.Equal(originalSoldItemCount + 1, committedInventory.SoldItemCount);
        Assert.NotNull(new JsonCartRepository(directory.Path).GetById(cart.Id));
        Assert.NotNull(new JsonTicketRepository(directory.Path).GetById(ticket.Id));
    }

    private static Ticket CreateTicket(string productId, DateOnly visitDate)
    {
        return new Ticket(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            productId,
            "adventurebay",
            visitDate,
            79.99m,
            "Ryan",
            "Peterson",
            "ryan@example.com");
    }

    private sealed class TemporaryDirectory : IDisposable
    {
        public TemporaryDirectory()
        {
            Path = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                "ParkTicketSystem.Tests",
                Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, true);
            }
        }
    }
}
