using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
 
namespace DllExercice3
{
    public enum OrderStatus
    {
        Pending, Shipped, Delivered, Cancelled
    }
    public class Order
    {
        public Guid Id { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; }
        public DateTimeOffset OrderDate { get; set; }
    }
    public interface IOrderService
    {
        Task AddOrderAsync(Order order, CancellationToken cancellationToken);
        Task<List<Order>> GetOrdersByStatusAsync(OrderStatus status, CancellationToken cancellationToken);
        Task<decimal> GetTotalRevenueAsync(CancellationToken cancellationToken);
        Task<List<Order>> GetOrdersByCustomerAsync(string customerName, CancellationToken cancellationToken = default);
        Task<List<Order>> GetRecentOrdersAsync(int days, CancellationToken cancellationToken = default);
        IAsyncEnumerable<Order> GetOrdersByCustomerNameAsync(string customerName, CancellationToken cancellationToken = default);
        Task<List<CustomerOrders>> GetTopCustomersAsync(int top, CancellationToken cancellationToken = default);
        IAsyncEnumerable<CustomerOrders> GetTopCustomersMassiveAsync(int top, CancellationToken cancellationToken = default);
        Task<CustomerOrders> GetCustomerSummaryAsync(CancellationToken cancellationToken = default);
    }
    public class OrderService : IOrderService
    {
        public ConcurrentDictionary<Guid, Order> _orders = new();
        //public async Task AddOrderAsync(Order order, CancellationToken cancellationToken = default)
        //{
        //    if (order == null) throw new ArgumentNullException(nameof(order));
        //    if (order.Id == Guid.Empty)
        //        order.Id = Guid.NewGuid();

        //    await Task.Run(() => _orders.TryAdd(order.Id, order), cancellationToken);
        //}
        public async Task AddOrderAsync(Order order, CancellationToken cancellationToken = default)
        {
            if (order == null) throw new ArgumentNullException(nameof(order));

            if (order.Id == Guid.Empty)
                order.Id = Guid.NewGuid();

            if (_orders.TryAdd(order.Id, order))
            {
                OnOrderChanged(new OrderChangedEventArgs(order.Id, order.CustomerName, "Added"));
            }

            await Task.CompletedTask;
        }

        public Task<List<Order>> GetOrdersByStatusAsync(OrderStatus status, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_orders.Values.Where(x => x.Status == status).ToList());
        }

        public Task<decimal> GetTotalRevenueAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_orders.Values.Sum(x => x.TotalAmount));
        }

        public Task<List<Order>> GetOrdersByCustomerAsync(string customerName, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(customerName))
                throw new ArgumentException("Customer name cannot be null or empty.", nameof(customerName));

            return Task.FromResult(_orders.Values
                .Where(x => string.Equals(x.CustomerName, customerName, StringComparison.InvariantCultureIgnoreCase))
                .ToList());
        }
        public async IAsyncEnumerable<Order> GetOrdersByCustomerNameAsync(
            string customerName,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(customerName))
                throw new ArgumentException("Customer name cannot be null or empty.", nameof(customerName));

            await Task.Yield();

            foreach (var order in _orders.Values)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (string.Equals(order.CustomerName, customerName, StringComparison.InvariantCultureIgnoreCase))
                {
                    yield return order; // Retourne chaque commande correspondante
                }
            }

        }

        public Task<List<Order>> GetRecentOrdersAsync(int days, CancellationToken cancellationToken = default)
        {
            if (days < 0)
                throw new ArgumentException("Days must be greater or equal to 0", nameof(days));
            return Task.FromResult(_orders.Values.Where(x => x.OrderDate >= DateTimeOffset.UtcNow.AddDays(-days)).ToList());
        }

        public async Task<List<CustomerOrders>> GetTopCustomersAsync(int top, CancellationToken cancellationToken = default)
        {
            if (top <= 0) throw new ArgumentException("TopN must be greter than 0", nameof(top));

            #region linq
            //return await Task.FromResult(_orders
            //    .GroupBy(x => x.Value.CustomerName)
            //    .Select(g => new CustomerOrders { CustomerName = g.Key, TotalSpent = g.Sum(s => s.Value.TotalAmount) })
            //    .OrderByDescending(o => o.TotalSpent)
            //    .Take(top)
            //    .ToList());
            #endregion

            #region PLINQ
            return await Task.Run(() =>
            {
                return _orders.Values
                    .AsParallel()
                    .WithDegreeOfParallelism(Environment.ProcessorCount) //Évite une surcharge CPU en limitant le nombre de threads
                    .WithCancellation(cancellationToken)
                    .GroupBy(x => x.CustomerName)
                    .Select(g => new CustomerOrders
                    {
                        CustomerName = g.Key,
                        TotalSpent = g.Sum(s => s.TotalAmount)
                    })
                    .OrderByDescending(o => o.TotalSpent)
                    .Take(top)
                    .ToList();
                //Func<IGrouping<string, Order>, CustomerOrders> customSelector = group => new CustomerOrders
                //{
                //    CustomerName = group.Key,
                //    TotalSpent = group.Sum(s => s.TotalAmount)
                //};
                //// Appeler la méthode avec la fonction de sélection personnalisée
                //var topCustomers = GetTopCustomers(_orders.Values,  cancellationToken, customSelector);
                //return topCustomers.Take(top).ToList();
            }, cancellationToken);
            #endregion
        }

        public async Task<CustomerOrders> GetCustomerSummaryAsync(CancellationToken cancellationToken = default)
        {
            return await Task.Run(() =>
            {
                var topCustomers = GetTopCustomers(_orders.Values, cancellationToken,
                    group => new CustomerOrdersAdvenced
                    {
                        CustomerName = group.Key,
                        TotalSpent = group.Sum(s => s.TotalAmount),
                        LastOrderDate = group.Max(m => m.OrderDate),
                        OrderCount = group.Count()
                    });
                return topCustomers.First();
            }, cancellationToken);
        }
        private ParallelQuery<CustomerOrders> GetTopCustomers(
            ICollection<Order> orders,
            CancellationToken cancellationToken,
             Func<IGrouping<string, Order>, CustomerOrders> selector) // Ajouter le paramètre pour la fonction de sélection
        {
            var query = orders
                .AsParallel()
                .WithDegreeOfParallelism(Environment.ProcessorCount) // Évite une surcharge CPU en limitant le nombre de threads
                .WithCancellation(cancellationToken)
                .GroupBy(x => x.CustomerName)
                .Select(selector) // Utiliser la fonction de sélection paramétrable
                .OrderByDescending(o => o.TotalSpent);
                
            return query;
        }

        public async IAsyncEnumerable<CustomerOrders> GetTopCustomersMassiveAsync(int top, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (top <= 0)
                throw new ArgumentException("TopN must be greater than 0", nameof(top));

            var groupedCustomers = _orders.Values
                .AsParallel()
                .WithDegreeOfParallelism(Environment.ProcessorCount)
                .WithCancellation(cancellationToken)
                .GroupBy(x => x.CustomerName)
                .OrderByDescending(g => g.Sum(o => o.TotalAmount)) // Trie ici pour éviter un `.ToList()`
                .Take(top); // On prend les `topN` groupes sans les matérialiser en mémoire

            foreach (var group in groupedCustomers)
            {
                cancellationToken.ThrowIfCancellationRequested(); // Annulation possible

                await Task.Yield(); // Laisse d'autres tâches s'exécuter pour éviter de bloquer un thread

                yield return new CustomerOrders
                {
                    CustomerName = group.Key,
                    TotalSpent = group.Sum(o => o.TotalAmount) // Calcul directement sur l'énumération
                };
            }
        }

        
        public async Task AddOrdersAsync(IEnumerable<Order> orders, CancellationToken cancellationToken = default)
        {
            if (orders == null || !orders.Any())
                throw new ArgumentException("The order list cannot be null or empty.", nameof(orders));

            await Parallel.ForEachAsync(orders, cancellationToken, async (order, token) =>
            {
                if (order == null) return;

                // Générer un GUID si l'ID est vide
                if (order.Id == Guid.Empty)
                    order.Id = Guid.NewGuid();

                // Tente d'ajouter la commande au dictionnaire concurrent
                if (_orders.TryAdd(order.Id, order))
                {
                    OnOrderChanged(new OrderChangedEventArgs(order.Id, order.CustomerName, "Added"));
                }

                // Permet de laisser un peu de temps aux autres tâches (évite la surcharge CPU)
                await Task.Yield();
            });
        }
        public async Task<bool> UpdateOrderAsync(Order updatedOrder, CancellationToken cancellationToken = default)
        {
            if (updatedOrder == null || !_orders.ContainsKey(updatedOrder.Id))
                return false;

            _orders[updatedOrder.Id] = updatedOrder; // Mise à jour
            OnOrderChanged(new OrderChangedEventArgs(updatedOrder.Id, updatedOrder.CustomerName, "Updated"));

            return await Task.FromResult(true);
        }

        public async Task<bool> RemoveOrderAsync(Guid orderId, CancellationToken cancellationToken = default)
        {
            if (_orders.TryRemove(orderId, out var removedOrder))
            {
                OnOrderChanged(new OrderChangedEventArgs(orderId, removedOrder.CustomerName, "Deleted"));
                return await Task.FromResult(true);
            }
            return false;
        }


        // Déclaration de l'événement
        public event EventHandler<OrderChangedEventArgs>? OrderChanged;

        protected virtual void OnOrderChanged(OrderChangedEventArgs e)
        {
            OrderChanged?.Invoke(this, e);
        }



    }

    public class CustomerOrders
    {
        public string CustomerName { get; set; } = string.Empty;
        public decimal TotalSpent { get; set; }
    }
    public class CustomerOrdersAdvenced : CustomerOrders
    {
        public int OrderCount { get; set; }
        public DateTimeOffset LastOrderDate { get; set; }
    }


    public class OrderChangedEventArgs : EventArgs
    {
        public Guid OrderId { get; }
        public string CustomerName { get; }
        public string ChangeType { get; } // "Added", "Updated", "Deleted"

        public OrderChangedEventArgs(Guid orderId, string customerName, string changeType)
        {
            OrderId = orderId;
            CustomerName = customerName;
            ChangeType = changeType;
        }
    }

}
