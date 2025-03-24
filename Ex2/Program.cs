using Ex2;

public static class main
{
    public static async Task Main()
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