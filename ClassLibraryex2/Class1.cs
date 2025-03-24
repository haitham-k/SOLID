using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ex2
{
    using System;
    using System.Collections.Concurrent;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    public interface ILogger<T> where T : class
    {
        void Info(string message);
        void Error(string message);
        Task InfoAsync(string message, CancellationToken cancellationToken = default);
    }

    public class Logger<T> : ILogger<T> where T : class
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

        private static readonly Dictionary<LogType, string> LogFiles = new()
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
            EnqueueLog($"[Error] {message}", LogType.Error);
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
    public interface INotification
    {
        void Send(string message);
    }
    public enum NotificationType
    {
        Email,
        SMS,
        Push
    }
    public class NotificationFactory
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
                await logger.InfoAsync($"Création d'une notification de type {notificationType}.", default(CancellationToken));
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
    public class BaseNotification : INotification
    {
        public void Send(string message)
        {
            Console.WriteLine($"{GetType().Name} sent: {message}");
        }
    }
    public class EmailNotification : BaseNotification { }
    public class SMSNotification : BaseNotification { }
    public class PushNotification : BaseNotification { }
    public class NotificationService
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
}