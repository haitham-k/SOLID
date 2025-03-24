using Ex2;
using Moq;
using Xunit;

namespace TestSolid
{
    public class NotificationServiceTests
    {
        [Fact]
        public void SendNotification_WithValidMessage_ShouldCallSendOnNotificationService()
        {
            // Arrange
            var mockNotification = new Mock<INotification>();
            var notificationService = new NotificationService(mockNotification.Object);
            string message = "Test message";

            // Act
            notificationService.SendNotification(message);

            // Assert
            mockNotification.Verify(n => n.Send(message), Times.Once); // Vérifie que Send a été appelé une fois avec le bon message
        }

        [Fact]
        public void SendNotification_WithEmptyMessage_ShouldThrowArgumentException()
        {
            // Arrange
            var mockNotification = new Mock<INotification>();
            var notificationService = new NotificationService(mockNotification.Object);
            string message = "";

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => notificationService.SendNotification(message));
            Assert.Equal("Cannot send an empty message (Parameter 'message')", exception.Message); // Vérifie le message d'erreur
        }

        [Fact]
        public void SendNotification_WithNullMessage_ShouldThrowArgumentException()
        {
            // Arrange
            var mockNotification = new Mock<INotification>();
            var notificationService = new NotificationService(mockNotification.Object);
            string message = null;

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => notificationService.SendNotification(message));
            Assert.Equal("Cannot send an empty message (Parameter 'message')", exception.Message); // Vérifie le message d'erreur
        }

        [Fact]
        public void Constructor_WithNullNotificationService_ShouldThrowArgumentNullException()
        {
            // Arrange
            INotification nullNotification = null;

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => new NotificationService(nullNotification));
            Assert.Equal("Notification service must be specified (Parameter 'service')", exception.Message); // Vérifie le message d'erreur
        }
    }
}
