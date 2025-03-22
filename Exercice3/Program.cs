using DllExercice3;
using System.Globalization;

var orderService = new OrderService();
// Abonnement à l'événement OrderChanged
orderService.OrderChanged += (sender, e) =>
{
    Console.WriteLine($"[{e.ChangeType}] Order ID: {e.OrderId}, Customer: {e.CustomerName}");
};
 
// Ajout d'une commande
await orderService.AddOrderAsync(new Order
{
    CustomerName = "Alice",
    TotalAmount = 100,
    Status = OrderStatus.Pending,
    OrderDate = DateTime.UtcNow
});

// Mise à jour d'une commande
var orderToUpdate = new Order { Id = orderService._orders.Keys.First(), CustomerName = "Alice", TotalAmount = 200, Status = OrderStatus.Shipped, OrderDate = DateTime.UtcNow };
await orderService.UpdateOrderAsync(orderToUpdate);

// Suppression d'une commande
await orderService.RemoveOrderAsync(orderToUpdate.Id);
await orderService.AddOrderAsync(new Order
{
    Id = Guid.NewGuid(),
    CustomerName = "Alice",
    TotalAmount = 100,
    Status = OrderStatus.Pending,
    OrderDate = DateTime.UtcNow
});

await orderService.AddOrderAsync(new Order
{
    Id = Guid.NewGuid(),
    CustomerName = "Bob",
    TotalAmount = 200,
    Status = OrderStatus.Shipped,
    OrderDate = DateTime.UtcNow.AddDays(-2)
});
await orderService.AddOrderAsync(new Order
{
    Id = Guid.NewGuid(),
    CustomerName = "Mark",
    TotalAmount = 300,
    Status = OrderStatus.Shipped,
    OrderDate = DateTime.UtcNow.AddDays(-3)
});
var ordersToAdd = new List<Order>
{
    new Order
    {
        Id = Guid.NewGuid(),
        CustomerName = "Bob",
        TotalAmount = 500,
        Status = OrderStatus.Shipped,
        OrderDate = DateTime.UtcNow.AddDays(-5)
    },
    new Order
    {
        Id = Guid.NewGuid(),
        CustomerName = "Joy",
        TotalAmount = 500,
        Status = OrderStatus.Shipped,
        OrderDate = DateTime.UtcNow.AddDays(-5)
    },
    new Order
    {
        Id = Guid.NewGuid(),
        CustomerName = "Eric",
        TotalAmount = 500,
        Status = OrderStatus.Shipped,
        OrderDate = DateTime.UtcNow.AddDays(-5)
    }
};
var ct = new CancellationTokenSource();
ct.CancelAfter(5000);
await orderService.AddOrdersAsync(ordersToAdd,ct.Token);

var pendingOrders = await orderService.GetOrdersByStatusAsync(OrderStatus.Pending);
var totalRevenue = await orderService.GetTotalRevenueAsync();

var cts = new CancellationTokenSource();
// Simule une annulation après 2 secondes
cts.CancelAfter(2000);
await foreach (var customerOrder in orderService.GetOrdersByCustomerNameAsync("bob",cts.Token))
{
    Console.WriteLine($"ID : {customerOrder.Id}, CustomerName: {customerOrder.CustomerName}, Status: {customerOrder.Status}, Amount: {customerOrder.TotalAmount}");
}
var recentOrders = await orderService.GetRecentOrdersAsync(1);
Console.WriteLine($"Commandes passées ces 1 derniers jours : {recentOrders.Count}");

var topCustomers = await orderService.GetTopCustomersAsync(3);
foreach (var customer in topCustomers)
{
    Console.WriteLine($"Client: {customer.CustomerName}, Total Dépensé: {customer.TotalSpent.ToString("C", CultureInfo.CreateSpecificCulture("fr-FR"))}");
}

await foreach (var customer in orderService.GetTopCustomersMassiveAsync(5, cts.Token))
{
    Console.WriteLine($"Client: {customer.CustomerName}, Total Dépensé: {customer.TotalSpent}");
}
var customerSummary = await orderService.GetCustomerSummaryAsync();
Console.WriteLine($"Total Revenue: {totalRevenue}");
Console.WriteLine($"Pending Orders: {pendingOrders.Count}");
Console.WriteLine($"GetCustomerSummaryAsync : {customerSummary.CustomerName}");
Console.ReadLine();