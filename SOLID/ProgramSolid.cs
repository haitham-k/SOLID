#region OPC 1
using Dip2;
using OCP;
using SolidExercice2;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Threading;
using System.Xml.Linq;
 
namespace Ocp1
{
    public class DiscountCalculator
    {
        public static double CalculateDiscount(IDiscount discount, double amount)
        {
            if (discount == null) return 0;
            return amount * discount.Amount;
        }
    }
    public interface IDiscount
    {
        double Amount { get; }
    }
    public class Regular : IDiscount
    {
        public double Amount { get { return 0.2F; } }
    }

    public class Premium : IDiscount
    {
        public double Amount { get { return 0.1F; } }
    }

    public class BailCalculator
    {
        private void Calculate(double amount)
        {
            IDiscount discount = new Regular();
            DiscountCalculator.CalculateDiscount(discount, amount);
        }
    }
}

#endregion

#region OCP2
namespace OCP
{
    public class AreaCalculator
    {
        public static double CalculateArea(IShape shape)
        {
            if (shape != null) return shape.CalculateArea();
            else
            {
                throw new ArgumentException("Shape not supported");
            }
        }
    }
    public interface IShape
    {
        double CalculateArea();
    }
    public class Circle : IShape
    {
        public Circle(double radius)
        {
            _radius = radius;
        }
        readonly double _radius;
        public double CalculateArea()
        {
            return Math.PI * _radius * _radius;
        }
    }

    public class Rectangle : IShape
    {
        public Rectangle(double width, double height)
        {
            _width = width;
            _height = height;
        }
        readonly double _width;
        readonly double _height;
        public double CalculateArea()
        {
            return _width * _height;
        }
    }

    //IShape circle = new Circle(6);
    //AreaCalculator.CalculateArea(circle);
}
#endregion

#region DIP 1
namespace Dip1
{
    public class FileLogger : ILogger
    {
        public void Log(string message)
        {
            Console.WriteLine($"Logging to file: {message}");
        }
    }
    public class DataBaseLogger : ILogger
    {
        public void Log(string message)
        {
            Console.WriteLine($"Logging to DataBase: {message}");
        }
    }
    public interface ILogger
    {
        void Log(string message);
    }
    public class UserService<T> where T : ILogger
    {
        private readonly T _logger;

        public UserService(T logger)
        {
            _logger = logger;
        }

        public void CreateUser(string username)
        {
            Console.WriteLine($"User {username} created.");
            _logger.Log($"User {username} created.");
        }
    }
    //var userService = new UserService<DataBaseLogger>();
    //userService.CreateUser("chatGPT")
}
#endregion

#region DIP 2
namespace Dip2
{

    public interface INotification
    {
        void Send(string message);
    }
    public class EmailNotification : INotification
    {
        public void Send(string message)
        {
            Console.WriteLine($"Sending email: {message}");
        }
    }

    public class SMSNotification : INotification
    {
        public void Send(string message)
        {
            Console.WriteLine($"Sending SMS: {message}");
        }
    }
    public interface INotificationService
    {
        INotification[] notifications { get; }
        void Send(string message);
    }
    public class NotificationService : INotificationService
    {
        public INotification[] notifications { get; set; }
        public NotificationService(INotification[] notifications)
        {
            this.notifications = notifications;
        }
        public void Send(string message)
        {
            foreach (var notification in notifications)
            {
                notification.Send(message);
            }
        }
    }
    public class OrderService
    {
        readonly INotificationService _notificationService;
        public OrderService(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public void PlaceOrder(string orderDetails)
        {
            Console.WriteLine($"Order placed: {orderDetails}");
            _notificationService.Send($"Order placed: {orderDetails}");
        }
    }
    public class testDip2
    {
        testDip2()
        {
            INotification[] notifications =
            {
            new EmailNotification(),
            new SMSNotification()
        };

            INotificationService notificationService = new NotificationService(notifications);
            OrderService orderService = new OrderService(notificationService);
            orderService.PlaceOrder("Order #123");
        }
    }
}
#endregion

#region SRP1
namespace Srp1
{
    public class Invoice
    {
        public string? Customer { get; set; }
        public double Amount { get; set; }

        public void GenerateInvoice()
        {
            Console.WriteLine($"Invoice generated for {Customer}, amount: {Amount}");
        }

    }
    public class InvoiceManager
    {
        readonly Invoice _invoice;
        public InvoiceManager(Invoice invoice)
        {
            _invoice = invoice;
        }
        public void Print()
        {
            Console.WriteLine($"Printing invoice for {_invoice.Customer}...");
        }

        public void SaveToDatabase()
        {
            Console.WriteLine($"Saving invoice for {_invoice.Customer} to database...");
        }
    }
}

#endregion

#region DIP 3
namespace DIP3
{
    public class EmailService : INotificationService
    {
        public void Send(string message)
        {
            Console.WriteLine($"Sending Email: {message}");
        }
    }

    public class NotificationManager : INotificationManager
    {
        private readonly INotificationService _service;
        public NotificationManager(INotificationService service)
        {
            _service = service;
        }

        public void Notify(string message)
        {
            _service.Send(message);
        }
    }
    public interface INotificationManager
    {
        void Notify(string message);
    }
    public interface INotificationService
    {
        void Send(string message);
    }
    public class Dip3
    {
        void Test()
        {
            INotificationManager manager = new NotificationManager(new EmailService());
            manager.Notify("message");
        }

    }
}

#endregion

#region ISP 1
namespace Isp1
{
    public interface IEat
    {
        void Eat();
    }
    public interface IWorker
    {
        void Work();
    }

    public class Employee : IWorker, IEat
    {
        public void Work()
        {
            Console.WriteLine("Employee working...");
        }

        public void Eat()
        {
            Console.WriteLine("Employee eating...");
        }
    }

    public class Robot : IWorker
    {
        public void Work()
        {
            Console.WriteLine("Robot working...");
        }

    }

}
#endregion

#region ISP2
namespace ISP2
{
    public interface IMediaAudio
    {
        void PlayAudio();
    }
    public interface IMediaVideo
    {
        void PlayVideo();
    }

    public class AudioPlayer : IMediaAudio
    {
        public void PlayAudio()
        {
            Console.WriteLine("Playing audio...");
        }

    }

    public class VideoPlayer : IMediaAudio, IMediaVideo
    {
        public void PlayAudio()
        {
            Console.WriteLine("Playing audio...");
        }

        public void PlayVideo()
        {
            Console.WriteLine("Playing video...");
        }
    }

}
#endregion

#region DIP4
namespace DIP4
{
    public interface ILightBulb
    {
        void TurnOn();
        void TurnOff();
    }
    public class LightBulb : ILightBulb
    {
        public void TurnOn()
        {
            Console.WriteLine("Light bulb turned on.");
        }

        public void TurnOff()
        {
            Console.WriteLine("Light bulb turned off.");
        }
    }

    public class Switch
    {
        private readonly ILightBulb _lightBulb;

        public Switch(ILightBulb lightBulb)
        {
            _lightBulb = lightBulb;
        }

        public void Operate(bool isOn)
        {
            if (isOn)
            {
                _lightBulb.TurnOn();
            }
            else
            {
                _lightBulb.TurnOff();
            }
        }
    }

    public class TestDIP4
    {
        public static void Test()
        {
            ILightBulb lightBulb = new LightBulb();
            var switcher = new Switch(lightBulb);
            switcher.Operate(true);
        }
    }
}

#endregion

#region SRP2
namespace SRP2
{
    public class Product
    {
        public string Name { get; set; } = string.Empty;
        public double Price { get; set; }
    }

    public class ProductRepository
    {
        public void SaveToDatabase(Product product)
        {
            Console.WriteLine($"Saving {product.Name} to database...");
        }

        public void SaveToFile(Product product)
        {
            Console.WriteLine($"Saving {product.Name} to file...");
        }
    }

    public class ProductPrinter
    {
        public void PrintDetails(Product product)
        {
            Console.WriteLine($"Product: {product.Name}, Price: {product.Price}");
        }
    }
}
#endregion

#region SRP3
namespace SRP3
{
    public interface IOrderRepository
    {
        void SaveToDatabase();
    }
    public class OrderRepository : IOrderRepository
    {
        Order _order;
        public OrderRepository(Order order)
        {
            _order = order;
        }
        public void SaveToDatabase()
        {
            Console.WriteLine($"Saving order {_order.OrderNumber} to database...");
        }
    }
    public interface IOrderConfirmation
    {
        void SendConfirmationEmail();
    }
    public class OrderConfirmation : IOrderConfirmation
    {
        readonly Order _order;
        public OrderConfirmation(Order order)
        {
            _order = order;
        }
        public void SendConfirmationEmail()
        {
            Console.WriteLine($"Sending confirmation email for order {_order.OrderNumber}...");
        }
    }

    public class Order
    {
        public string OrderNumber { get; set; } = string.Empty;
        public double TotalAmount { get; set; }

        public void CalculateTotal()
        {
            Console.WriteLine("Calculating total amount...");
        }
    }
    public class Test
    {
        public Test()
        {
            var order = new Order
            {
                OrderNumber = "1",
                TotalAmount = 22
            };
            IOrderRepository orderRepository = new OrderRepository(order);
            orderRepository.SaveToDatabase();

            IOrderConfirmation orderConfirmation = new OrderConfirmation(order);
            orderConfirmation.SendConfirmationEmail();

            order.CalculateTotal();
        }
    }

}
#endregion

#region solid exercice 1
namespace SolidExercice1
{
    class Program
    {
        static void Main(string[] args)
        {
            IShape rectangle = new Rectangle { Width = 5, Height = 10 };
            IShape circle = new Circle(7);
            IShape carre = new Carre(9);
            Console.WriteLine("Rectangle Area: " + rectangle.CalculateArea());
            Console.WriteLine("Circle Area: " + circle.CalculateArea());
            Console.WriteLine("Carre Area: " + carre.CalculateArea());
        }
    }
    interface IShape
    {
        double CalculateArea();
    }
    class Carre : IShape
    {
        public Carre(double side)
        {
            side = side > 0 ? side : throw new ArgumentException("Side must be positive"); ;
        }
        private double _side;
        public double CalculateArea()
        {
            return _side * _side;
        }
    }
    class Rectangle : IShape
    {
        private double _width;
        public double Width
        {
            get { return _width; }
            set { _width = value < 0 ? value : throw new ArgumentException("Width must be positive"); }
        }

        private double _height;

        public double Height
        {
            get { return _height; }
            set { _height = value < 0 ? value : throw new ArgumentException("Height must be positive"); }
        }

        public double CalculateArea()
        {
            return this.Width * this.Height;
        }
    }
    class Circle : IShape
    {
        private double _radius;
        public Circle(double raduis)
        {
            _radius = raduis > 0 ? raduis : throw new ArgumentException("Radius must be positive");
        }

        public double CalculateArea()
        {
            return Math.PI * this._radius * this._radius;
        }
    }
}
namespace SolidExercice2
{
    class Program
    {
        static void Main(string[] args)
        {
            INotificationService services = new NotificationService(
            [
                new EmailNotification("user@example.com", "Hello via Email!"),
                new SMSNotification ("123456789", "Hello via SMS!")
            ]);
            services.SendNotification();
        }
    }

    interface INotification
    {
        void SendNotification();
    }
    interface INotificationService
    {
        void SendNotification();
    }
    class EmailNotification : INotification
    {
        [EmailAddress]
        private string _email;
        [MinLength(5)]
        private string _message;
        public EmailNotification(string email, string message)
        {
            _email = email;
            _message = message;
        }
        public void SendNotification()
        {
            Console.WriteLine($"Sending email to {_email}: {_message}");
        }

    }
    class SMSNotification : INotification
    {
        [Phone]
        private string _phoneNumber;

        [MinLength(5)]
        private string _message;
        public SMSNotification(string phoneNumber, string message)
        {
            _phoneNumber = phoneNumber;
            _message = message;
        }
        public void SendNotification()
        {
            Console.WriteLine($"Sending email to {_phoneNumber}: {_message}");
        }

    }
    class NotificationService : INotificationService
    {
        public NotificationService(INotification[] services)
        {
            _services = services;
        }
        readonly INotification[] _services;
        public void SendNotification()
        {
            foreach (var service in _services)
            {
                service.SendNotification();
            }
        }
    }

}
#endregion

#region ex2
namespace Ex2
{
    using System;
    using System.Collections.Concurrent;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    interface ILogger<T> where T : class
    {
        void Info(string message);
        void Error(string message);
        Task InfoAsync(string message, CancellationToken cancellationToken = default);
    }

    class Logger<T> : ILogger<T> where T : class
    {
        enum LogType
        {
            Info,
            Error,
            Warning,
        }
       
        private static readonly ConcurrentQueue<(LogType Type, string Message)> LogQueue = new();
        private static readonly ManualResetEventSlim LogEvent = new(false);
        private static readonly Task LogTask;
        private static readonly CancellationTokenSource Cts = new();

        private static readonly Dictionary<LogType,string> LogFiles = new()
        {
            { LogType.Info , "info.log"},
            { LogType.Error, "error.log" },
            { LogType.Warning,"warning.log" }
        };
        private static readonly Dictionary<LogType, SemaphoreSlim> SemaphoreSlimByType = new()
        {
            { LogType.Info , new SemaphoreSlim(1,1)},
            { LogType.Error, new SemaphoreSlim(1,1) },
            { LogType.Warning, new SemaphoreSlim(1,1) }
        };

        static Logger()
        {
            // Démarre une tâche en arrière-plan pour surveiller et écrire les logs dans un fichier
            LogTask = Task.Run(ProcessLogQueue, Cts.Token);
        }

        public void Error(string message)
        {
            EnqueueLog($"[Error] {message}",LogType.Error);
        }

        public void Info(string message)
        {
            EnqueueLog($"[Info] {message}", LogType.Info);
        }

        public async Task InfoAsync(string message, CancellationToken cancellationToken = default)
        {
            await Task.Run(() => EnqueueLog($"[InfoAsync] {message}", LogType.Info), cancellationToken);
        }

        private static void EnqueueLog(string logMessage, LogType logType)
        {
            string formattedMessage = $"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} - {logMessage}";
            LogQueue.Enqueue((logType, formattedMessage));
            LogEvent.Set();
            //string formattedMessage = $"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} - {logMessage}";
            //LogQueue.Enqueue($"{logType}|{logMessage}");
            //LogEvent.Set(); // Signale qu'il y a un nouveau message dans la queue
        }

        private static async Task ProcessLogQueue()
        {
            while (!Cts.Token.IsCancellationRequested)
            {
                LogEvent.Wait(); // Attend un log à écrire

                while (LogQueue.TryDequeue(out var logMessage))
                {
                    var logFile = LogFiles[logMessage.Type];
                    SemaphoreSlim fileLock = SemaphoreSlimByType[logMessage.Type];
                    try
                    {
                        await fileLock.WaitAsync(); // 🔒 Empêche plusieurs accès simultanés au fichier
                        await File.AppendAllTextAsync(logFile, logMessage.Message + Environment.NewLine);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Logging Error] {ex.Message}");
                    }
                    finally
                    {
                        fileLock.Release(); // 🔓 Libère le verrou pour un autre thread
                    }
                }

                LogEvent.Reset(); // Réinitialise l'événement en attendant de nouveaux logs
            }
        }

        public static async Task FlushLogs()
        {
            while (!LogQueue.IsEmpty)
            {
                await Task.Delay(100); // Attendre que tous les logs soient écrits
            }
            Cts.Cancel(); // Arrêter proprement la tâche de log en arrière-plan
        }
    }
    interface INotification
    {
        void Send(string message);
    }
    enum NotificationType
    {
        Email,
        SMS,
        Push
    }
    class NotificationFactory
    {
        private static readonly Dictionary<NotificationType, Func<INotification>> _factories = new()
        {
            { NotificationType.Email, () => new EmailNotification() },
            { NotificationType.SMS, () => new SMSNotification() },
            { NotificationType.Push, () => new PushNotification() }
        };

        public static async Task<INotification> Create(NotificationType notificationType, ILogger<NotificationFactory> logger)
        {
            if (_factories.TryGetValue(notificationType, out var factory))
            {
                await logger.InfoAsync($"Création d'une notification de type {notificationType}.",default(CancellationToken));
                return factory();

            }
            logger.Error($"[ERROR] Type de notification invalide : {notificationType}.");
            throw new ArgumentException($"{notificationType} factory is not implemented!");
        }
        public static async Task<INotification> Create(string type, ILogger<NotificationFactory> logger, CancellationToken cancellationToken)
        {
            if (Enum.TryParse(typeof(NotificationType), type, true, out var notificationType) &&
                notificationType is NotificationType validType &&
                _factories.TryGetValue(validType, out var factory))
            {
                //logger.Info($"Création d'une notification de type {notificationType}.");
                await logger.InfoAsync($"Création d'une notification de type {notificationType}.", cancellationToken);
                return factory();
            }


            logger.Error($"[ERROR] Type de notification invalide : {notificationType}.");
            throw new ArgumentException($"{notificationType} factory is not implemented!");
        }
    }
    class BaseNotification : INotification
    {
        public void Send(string message)
        {
            Console.WriteLine($"{GetType().Name} sent: {message}");
        }
    }
    class EmailNotification : BaseNotification { }
    class SMSNotification : BaseNotification { }
    class PushNotification : BaseNotification { }
    class NotificationService
    {
        readonly INotification _service;
        public NotificationService(INotification service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service), "Notification service must be specified"); ;
        }
        public void SendNotification(string message)
        {
            if (string.IsNullOrWhiteSpace(message)) //throw new Exception("can't send empty message");
                throw new ArgumentException("Cannot send an empty message", nameof(message));
            _service.Send(message);
        }
    }

    class TestApp
    {
        async void Test()
        {
            ILogger<NotificationFactory> Notiflogger = new Logger<NotificationFactory>();
            var notifFactoMail = await NotificationFactory.Create("Email", Notiflogger, default(CancellationToken));
            notifFactoMail.Send("Hello !");

            var emailNotifier = new EmailNotification();
            var smsNotifier = new SMSNotification();
            var pushNotifier = new PushNotification();

            var notificationService = new NotificationService(emailNotifier);
            notificationService.SendNotification("Bienvenue sur notre plateforme !");

            notificationService = new NotificationService(smsNotifier);
            notificationService.SendNotification("Votre code de vérification est 1234");

            notificationService = new NotificationService(pushNotifier);
            notificationService.SendNotification("Vous avez un nouveau message !");
        }
    }
}
#endregion
